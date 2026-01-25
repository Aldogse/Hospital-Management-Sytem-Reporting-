using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast;
using APIResponses.forecast_results;
using APIResponses.Historical_report.training_models_prediction;
using APIResponses.Training_Models;
using Microsoft.ML;
using Microsoft.ML.Data;

public class monthInsuranceClaimsStatusPredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);

    private ITransformer _approvedModel;
    private ITransformer _deniedModel;

    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ApprovedModelFile = "month_claims_approved_model.zip";
    private const string DeniedModelFile = "month_claims_denied_model.zip";

    public monthInsuranceClaimsStatusPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModels();
    }

    // =====================================================
    // LOAD MODELS
    // =====================================================
    private void LoadModels()
    {
        string approvedPath = Path.Combine(ModelDir, ApprovedModelFile);
        string deniedPath = Path.Combine(ModelDir, DeniedModelFile);

        if (File.Exists(approvedPath))
        {
            using var stream = new FileStream(approvedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _approvedModel = _mlContext.Model.Load(stream, out _);
        }

        if (File.Exists(deniedPath))
        {
            using var stream = new FileStream(deniedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _deniedModel = _mlContext.Model.Load(stream, out _);
        }
    }

    // =====================================================
    // TRAIN MODELS (THREAD-SAFE)
    // =====================================================
    public async Task TrainAsync(
        IEnumerable<month_hospital_claims_status_entity> data,
        CancellationToken cancellationToken)
    {
        if (!data.Any()) return;

        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                var dataView = _mlContext.Data.LoadFromEnumerable(data);

                var featurePipeline = _mlContext.Transforms
                    .Concatenate(
                        "Features",
                        nameof(month_hospital_claims_status_entity.insurance_provider_id),
                        nameof(month_hospital_claims_status_entity.month),
                        nameof(month_hospital_claims_status_entity.year),
                        nameof(month_hospital_claims_status_entity.total_claims),
                        nameof(month_hospital_claims_status_entity.last_month_approved_claims),
                        nameof(month_hospital_claims_status_entity.last_month_denied_claims))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"));

                // =========================
                // APPROVED CLAIMS MODEL
                // =========================
                var approvedPipeline = featurePipeline.Append(
                    _mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_hospital_claims_status_entity.total_claim_approved),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _approvedModel = approvedPipeline.Fit(dataView);

                _mlContext.Model.Save(
                    _approvedModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, ApprovedModelFile));

                // =========================
                // DENIED CLAIMS MODEL
                // =========================
                var deniedPipeline = featurePipeline.Append(
                    _mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_hospital_claims_status_entity.total_claim_denied),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _deniedModel = deniedPipeline.Fit(dataView);

                _mlContext.Model.Save(
                    _deniedModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, DeniedModelFile));

            }, cancellationToken);
        }
        finally
        {
            _trainingLock.Release();
        }
    }

    // =====================================================
    // FORECAST SINGLE MONTH
    // =====================================================
    public monthNumberOfClaimPrediction ForecastSingleMonth(
        month_hospital_claims_status_entity input)
    {
        if (_approvedModel == null || _deniedModel == null)
            throw new InvalidOperationException("Models are not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });

        float approved = Predict(_approvedModel, dv);
        float denied = Predict(_deniedModel, dv);

        return new monthNumberOfClaimPrediction
        {
            total_claims = input.total_claims,
            total_claim_approved = approved,
            total_claim_denied = denied
        };
    }

    // =====================================================
    // INTERNAL PREDICTION HELPER
    // =====================================================
    private float Predict(ITransformer model, IDataView dv)
    {
        var transformed = model.Transform(dv);

        return _mlContext.Data
            .CreateEnumerable<SinglePrediction>(transformed, reuseRowObject: false)
            .FirstOrDefault()?.Score ?? 0f;
    }

    // =====================================================
    // INTERNAL SCORE CLASS
    // =====================================================
    private class SinglePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

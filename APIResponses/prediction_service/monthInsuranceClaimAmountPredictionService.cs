using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast;
using Microsoft.ML;
using Microsoft.ML.Data;

public class monthInsuranceClaimAmountPredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);

    private ITransformer _approvedAmountModel;
    private ITransformer _declinedAmountModel;

    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ApprovedAmountModelFile = "month_claim_amount_approved_model.zip";
    private const string DeclinedAmountModelFile = "month_claim_amount_declined_model.zip";

    public monthInsuranceClaimAmountPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModels();
    }

    // =====================================================
    // LOAD MODELS
    // =====================================================
    private void LoadModels()
    {
        string approvedPath = Path.Combine(ModelDir, ApprovedAmountModelFile);
        string declinedPath = Path.Combine(ModelDir, DeclinedAmountModelFile);

        if (File.Exists(approvedPath))
        {
            using var stream = new FileStream(approvedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _approvedAmountModel = _mlContext.Model.Load(stream, out _);
        }

        if (File.Exists(declinedPath))
        {
            using var stream = new FileStream(declinedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _declinedAmountModel = _mlContext.Model.Load(stream, out _);
        }
    }

    // =====================================================
    // TRAIN MODELS
    // =====================================================
    public async Task TrainAsync(
        IEnumerable<month_hospital_claims_amount_entity> data,
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
                        nameof(month_hospital_claims_amount_entity.insurance_provider_id),
                        nameof(month_hospital_claims_amount_entity.month),
                        nameof(month_hospital_claims_amount_entity.year),

                        // ✅ ADDED
                        nameof(month_hospital_claims_amount_entity.total_claim_amount_submitted),

                        nameof(month_hospital_claims_amount_entity.last_month_total_claim_approved_amount),
                        nameof(month_hospital_claims_amount_entity.last_month_total_claim_declined_amount))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"));

                // =========================
                // APPROVED AMOUNT MODEL
                // =========================
                var approvedPipeline = featurePipeline.Append(
                    _mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_hospital_claims_amount_entity.total_claim_approved_amount),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _approvedAmountModel = approvedPipeline.Fit(dataView);
                _mlContext.Model.Save(
                    _approvedAmountModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, ApprovedAmountModelFile));

                // =========================
                // DECLINED AMOUNT MODEL
                // =========================
                var declinedPipeline = featurePipeline.Append(
                    _mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_hospital_claims_amount_entity.total_claim_declined_amount),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _declinedAmountModel = declinedPipeline.Fit(dataView);
                _mlContext.Model.Save(
                    _declinedAmountModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, DeclinedAmountModelFile));

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
    public monthClaimAmountPrediction ForecastSingleMonth(
        month_hospital_claims_amount_entity input)
    {
        if (_approvedAmountModel == null || _declinedAmountModel == null)
            throw new InvalidOperationException("Models are not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });

        float approvedAmount = Predict(_approvedAmountModel, dv);
        float declinedAmount = Predict(_declinedAmountModel, dv);

        return new monthClaimAmountPrediction
        {

            total_claim_approved_amount = approvedAmount,
            total_claim_declined_amount = declinedAmount
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

    private class SinglePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast;
using APIResponses.forecast_results;
using APIResponses.Training_Models;
using Microsoft.ML;
using Microsoft.ML.Data;

public class monthPatientAdmissionPredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);

    private ITransformer _admissionModel;
    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ModelFile = "month_patient_admission_model.zip";

    public monthPatientAdmissionPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModel();
    }

    private void LoadModel()
    {
        string path = Path.Combine(ModelDir, ModelFile);
        if (!File.Exists(path)) return;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        _admissionModel = _mlContext.Model.Load(stream, out _);
    }

    // =====================================================
    // TRAIN MODEL (THREAD-SAFE)
    // =====================================================
    public async Task TrainAsync(IEnumerable<month_patient_admission_forecasting_entity> data, CancellationToken cancellationToken)
    {
        if (!data.Any()) return;

        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                var dataView = _mlContext.Data.LoadFromEnumerable(data);

                var pipeline = _mlContext.Transforms
                    .Concatenate(
                        "Features",
                        nameof(month_patient_admission_forecasting_entity.month),
                        nameof(month_patient_admission_forecasting_entity.year),
                        nameof(month_patient_admission_forecasting_entity.prev_month_admission),
                        nameof(month_patient_admission_forecasting_entity.last_three_month_admission),
                        nameof(month_patient_admission_forecasting_entity.last_sixth_month_admission))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_patient_admission_forecasting_entity.total_admission),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _admissionModel = pipeline.Fit(dataView);

                // Save the model
                string fullPath = Path.Combine(ModelDir, ModelFile);
                _mlContext.Model.Save(_admissionModel, dataView.Schema, fullPath);
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
    public monthPatientAdmissionForecast ForecastSingleMonth(month_patient_admission_forecasting_entity input)
    {
        if (_admissionModel == null)
            throw new InvalidOperationException("Admission model is not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });
        var transformed = _admissionModel.Transform(dv);

        float predictedAdmission = _mlContext.Data
            .CreateEnumerable<SinglePrediction>(transformed, reuseRowObject: false)
            .FirstOrDefault()?.Score ?? 0f;

        return new monthPatientAdmissionForecast
        {
            total_admission = predictedAdmission
        };
    }

    // =====================================================
    // INTERNAL SINGLE PREDICTION CLASS
    // =====================================================
    private class SinglePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

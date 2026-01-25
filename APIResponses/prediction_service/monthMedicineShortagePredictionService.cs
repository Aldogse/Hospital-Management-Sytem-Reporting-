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

public class monthMedicineShortagePredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);
    private ITransformer _shortageModel;
    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ShortageModelFile = "month_medicine_shortage_model.zip";

    public monthMedicineShortagePredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModels();
    }

    // =====================================================
    // LOAD MODEL
    // =====================================================
    private void LoadModels()
    {
        string modelPath = Path.Combine(ModelDir, ShortageModelFile);

        if (File.Exists(modelPath))
        {
            using var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _shortageModel = _mlContext.Model.Load(stream, out _);
        }
    }

    // =====================================================
    // TRAIN MODEL (THREAD-SAFE)
    // =====================================================
    public async Task TrainAsync(
        IEnumerable<month_medicine_shortage_prediction_entity> trainingData,
        CancellationToken cancellationToken)
    {
        if (!trainingData.Any()) return;

        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                // ✅ Convert bool features to float in a concrete ML class
                var preparedData = trainingData.Select(d => new month_medicine_shortage_ml_entity
                {
                    med_id = d.med_id,
                    month = d.month,
                    year = d.year,
                    current_stock = d.current_stock,
                    avg_daily_use = d.avg_daily_use,
                    total_dispensed_month = d.total_dispensed_month,
                    expiring_within_30_days = d.expiring_within_30_days ? 1f : 0f,
                    shortage_occured = d.shortage_occured
                }).ToList();

                var dataView = _mlContext.Data.LoadFromEnumerable(preparedData);

                var featurePipeline = _mlContext.Transforms
                    .Concatenate(
                        "Features",
                        nameof(month_medicine_shortage_ml_entity.med_id),
                        nameof(month_medicine_shortage_ml_entity.month),
                        nameof(month_medicine_shortage_ml_entity.year),
                        nameof(month_medicine_shortage_ml_entity.current_stock),
                        nameof(month_medicine_shortage_ml_entity.avg_daily_use),
                        nameof(month_medicine_shortage_ml_entity.total_dispensed_month),
                        nameof(month_medicine_shortage_ml_entity.expiring_within_30_days))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"));

                var pipeline = featurePipeline.Append(
                    _mlContext.BinaryClassification.Trainers.FastTree(
                        labelColumnName: nameof(month_medicine_shortage_ml_entity.shortage_occured),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _shortageModel = pipeline.Fit(dataView);

                _mlContext.Model.Save(
                    _shortageModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, ShortageModelFile));

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
    public monthMedicineShortagePrediction ForecastSingleMonth(
        month_medicine_shortage_prediction_entity input)
    {
        if (_shortageModel == null)
            throw new InvalidOperationException("Model is not trained.");

        var mlInput = new month_medicine_shortage_ml_entity
        {
            med_id = input.med_id,
            month = input.month,
            year = input.year,
            current_stock = input.current_stock,
            avg_daily_use = input.avg_daily_use,
            total_dispensed_month = input.total_dispensed_month,
            expiring_within_30_days = input.expiring_within_30_days ? 1f : 0f,
            shortage_occured = input.shortage_occured
        };

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { mlInput });
        var transformed = _shortageModel.Transform(dv);

        var result = _mlContext.Data
            .CreateEnumerable<ShortagePredictionResult>(transformed, reuseRowObject: false)
            .FirstOrDefault();

        return new monthMedicineShortagePrediction
        {
            avg_daily_use = input.avg_daily_use,
            shortage_occured = result?.PredictedLabel == true ? 1f : 0f,
            Score = new[] { result?.Probability ?? 0f }
        };
    }

    // =====================================================
    // INTERNAL ML CLASS
    // =====================================================
    private class month_medicine_shortage_ml_entity
    {
        public float med_id { get; set; }
        public float month { get; set; }
        public float year { get; set; }
        public float current_stock { get; set; }
        public float avg_daily_use { get; set; }
        public float total_dispensed_month { get; set; }
        public float expiring_within_30_days { get; set; } // float now
        public bool shortage_occured { get; set; }
    }

    // =====================================================
    // INTERNAL PREDICTION RESULT
    // =====================================================
    private class ShortagePredictionResult
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }
        public float Score { get; set; }
    }
}

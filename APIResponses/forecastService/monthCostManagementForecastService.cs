using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast_results;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

public class monthCostForecastService
{
    private readonly MLContext _mlContext = new(seed: 42);
    private ITransformer _model;

    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ModelFile = "month_cost_forecast_model.zip";

    private readonly ILogger<monthCostForecastService> _logger;

    public monthCostForecastService(ILogger<monthCostForecastService> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(ModelDir);
        LoadModel();
    }

    private void LoadModel()
    {
        string path = Path.Combine(ModelDir, ModelFile);
        if (!File.Exists(path)) return;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        _model = _mlContext.Model.Load(stream, out _);
        _logger.LogInformation("Month cost forecast model loaded from {path}", path);
    }

    // =====================================================
    // TRAIN MODEL (THREAD-SAFE + BACKGROUND SERVICE READY)
    // =====================================================
    public async Task TrainAsync(IEnumerable<month_cost_training_entity> data, CancellationToken cancellationToken)
    {
        if (!data.Any())
        {
            _logger.LogWarning("No training data provided for cost forecasting.");
            return;
        }

        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                var trainingDataView = _mlContext.Data.LoadFromEnumerable(data);

                var pipeline = _mlContext.Transforms
                    .Concatenate(
                        "Features",
                        nameof(month_cost_training_entity.month),
                        nameof(month_cost_training_entity.year),
                        nameof(month_cost_training_entity.previous_month_operational_cost),
                        nameof(month_cost_training_entity.last_three_months_cost),
                        nameof(month_cost_training_entity.last_six_months_cost))
                    .Append(_mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_cost_training_entity.total_month_operational_cost),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _model = pipeline.Fit(trainingDataView);

                // Save model
                string fullPath = Path.Combine(ModelDir, ModelFile);
                _mlContext.Model.Save(_model, trainingDataView.Schema, fullPath);
                _logger.LogInformation("Cost forecast model trained and saved to {path}", fullPath);

            }, cancellationToken);
        }
        finally
        {
            _trainingLock.Release();
        }
    }

    // =====================================================
    // PREDICT SINGLE MONTH
    // =====================================================
    public month_cost_management_forecast_result Predict(month_cost_training_entity input)
    {
        if (_model == null)
            throw new InvalidOperationException("Cost forecast model is not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });
        var transformed = _model.Transform(dv);

        float predictedCost = _mlContext.Data.CreateEnumerable<SinglePrediction>(transformed, reuseRowObject: false)
            .FirstOrDefault()?.Score ?? 0f;

        _logger.LogInformation("Prediction for Month={month}, Year={year}: {cost}", input.month, input.year, predictedCost);

        return new month_cost_management_forecast_result
        {
            month = (int)input.month,
            year = (int)input.year,
            month_forecasted_cost = predictedCost
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

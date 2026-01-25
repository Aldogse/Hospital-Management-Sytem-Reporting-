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

public class monthRevenueForecastPredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);

    private ITransformer _revenueModel;
    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string ModelFile = "month_revenue_forecast_model.zip";

    public monthRevenueForecastPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModel();
    }

    private void LoadModel()
    {
        string path = Path.Combine(ModelDir, ModelFile);
        if (!File.Exists(path)) return;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        _revenueModel = _mlContext.Model.Load(stream, out _);
    }

    // =====================================================
    // TRAIN MODEL (THREAD-SAFE)
    // =====================================================
    public async Task TrainAsync(IEnumerable<month_revenue_report_forecast_entity> data, CancellationToken cancellationToken)
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
                        nameof(month_revenue_report_forecast_entity.month),
                        nameof(month_revenue_report_forecast_entity.year),
                        nameof(month_revenue_report_forecast_entity.total_patient),
                        nameof(month_revenue_report_forecast_entity.pharmacy_total_transactions),
                        nameof(month_revenue_report_forecast_entity.average_pharmacy_sale_per_transaction))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.Regression.Trainers.FastTree(
                        labelColumnName: nameof(month_revenue_report_forecast_entity.total_revenue),
                        featureColumnName: "Features",
                        numberOfLeaves: 20,
                        numberOfTrees: 100,
                        minimumExampleCountPerLeaf: 2));

                _revenueModel = pipeline.Fit(dataView);

                // Save the model
                string fullPath = Path.Combine(ModelDir, ModelFile);
                _mlContext.Model.Save(_revenueModel, dataView.Schema, fullPath);
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
    public monthRevenueForecast ForecastSingleMonth(month_revenue_report_forecast_entity input)
    {
        if (_revenueModel == null)
            throw new InvalidOperationException("Revenue model is not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });
        var transformed = _revenueModel.Transform(dv);

        float predictedRevenue = _mlContext.Data
            .CreateEnumerable<SinglePrediction>(transformed, reuseRowObject: false)
            .FirstOrDefault()?.Score ?? 0f;

        // ✅ Compute average_bill_amount from total_revenue / total_patient
        decimal avgBill = input.total_patient > 0
            ? (decimal)predictedRevenue / (decimal)input.total_patient
            : 0m;

        return new monthRevenueForecast
        {
            total_revenue = predictedRevenue,
            pharmacy_total_transactions = input.pharmacy_total_transactions,
            average_bill_amount = (float)avgBill
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast;
using APIResponses.Training_Models;
using Microsoft.ML;
using Microsoft.ML.Data;

public class BedOccupancyPredictionService
{
    private readonly MLContext _mlContext = new(seed: 1);

    private ITransformer _occupiedBedsModel;
    private ITransformer _dischargedModel;
    private ITransformer _occupancyRateModel;
    private ITransformer _brokenRateModel;

    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";

    public BedOccupancyPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModels();
    }

    private void LoadModels()
    {
        _occupiedBedsModel = LoadModel("occupied_beds.zip");
        _dischargedModel = LoadModel("recently_discharged.zip");
        _occupancyRateModel = LoadModel("bed_occupancy_rate.zip");
        _brokenRateModel = LoadModel("broken_bed_rate.zip");
    }

    private ITransformer LoadModel(string fileName)
    {
        string path = Path.Combine(ModelDir, fileName);
        if (!File.Exists(path)) return null;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return _mlContext.Model.Load(stream, out _);
    }

    // =====================================================
    // ASYNC TRAIN (SAFE FOR BACKGROUND SERVICE)
    // =====================================================
    public async Task TrainAsync(
        IEnumerable<month_bed_occupancy_training_entity> data,
        CancellationToken cancellationToken)
    {
        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                TrainSingle(
                    data,
                    nameof(month_bed_occupancy_training_entity.occupied_beds),
                    "occupied_beds.zip",
                    ref _occupiedBedsModel);

                TrainSingle(
                    data,
                    nameof(month_bed_occupancy_training_entity.recently_discharged),
                    "recently_discharged.zip",
                    ref _dischargedModel);

                TrainSingle(
                    data,
                    nameof(month_bed_occupancy_training_entity.bed_occupancy_rate),
                    "bed_occupancy_rate.zip",
                    ref _occupancyRateModel);

                TrainSingle(
                    data,
                    nameof(month_bed_occupancy_training_entity.broken_bed_rate),
                    "broken_bed_rate.zip",
                    ref _brokenRateModel);

            }, cancellationToken);
        }
        finally
        {
            _trainingLock.Release();
        }
    }

    // =====================================================
    // SINGLE MODEL TRAIN
    // =====================================================
    private void TrainSingle(
        IEnumerable<month_bed_occupancy_training_entity> data,
        string labelColumn,
        string fileName,
        ref ITransformer model)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms
            .Concatenate("Features",
                nameof(month_bed_occupancy_training_entity.month),
                nameof(month_bed_occupancy_training_entity.year),
                nameof(month_bed_occupancy_training_entity.total_beds))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: labelColumn,
                featureColumnName: "Features",
                maximumNumberOfIterations: 100));

        model = pipeline.Fit(dataView);

        _mlContext.Model.Save(model, dataView.Schema, Path.Combine(ModelDir, fileName));
    }

    // =====================================================
    // FORECAST SINGLE MONTH
    // =====================================================
    public monthBedOccupancyForecast ForecastSingleMonth(
        month_bed_occupancy_training_entity input)
    {
        return new monthBedOccupancyForecast
        {
            PredictedOccupiedBeds = Predict(_occupiedBedsModel, input),
            PredictedRecentlyDischarged = Predict(_dischargedModel, input),
            PredictedBedOccupancyRate = Predict(_occupancyRateModel, input),
            PredictedBrokenBedRate = Predict(_brokenRateModel, input)
        };
    }

    private float Predict(
        ITransformer model,
        month_bed_occupancy_training_entity input)
    {
        if (model == null)
            throw new InvalidOperationException("Model not loaded.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });
        var transformed = model.Transform(dv);

        return _mlContext.Data
            .CreateEnumerable<SinglePrediction>(transformed, reuseRowObject: false)
            .First()
            .Score;
    }

    private class SinglePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

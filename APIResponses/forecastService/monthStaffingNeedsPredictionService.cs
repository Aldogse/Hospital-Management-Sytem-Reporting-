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

public class monthStaffingNeedsPredictionService
{
    private readonly MLContext _mlContext = new(seed: 42);

    private ITransformer _hoursModel;
    private ITransformer _staffModel;

    private static readonly SemaphoreSlim _trainingLock = new(1, 1);

    private const string ModelDir = "Models";
    private const string HoursModelFile = "month_staffing_hours_model.zip";
    private const string StaffModelFile = "month_staffing_staff_model.zip";

    public monthStaffingNeedsPredictionService()
    {
        Directory.CreateDirectory(ModelDir);
        LoadModels();
    }

    private void LoadModels()
    {
        string hoursPath = Path.Combine(ModelDir, HoursModelFile);
        string staffPath = Path.Combine(ModelDir, StaffModelFile);

        if (File.Exists(hoursPath))
            _hoursModel = _mlContext.Model.Load(hoursPath, out _);

        if (File.Exists(staffPath))
            _staffModel = _mlContext.Model.Load(staffPath, out _);
    }

    // =====================================================
    // SHARED FEATURE PIPELINE
    // =====================================================
    private IEstimator<ITransformer> BuildFeaturePipeline()
    {
        return _mlContext.Transforms.Conversion.ConvertType(
                "month_f",
                nameof(month_staffing_needs_training_entity.month),
                DataKind.Single)

            .Append(_mlContext.Transforms.Conversion.ConvertType(
                "year_f",
                nameof(month_staffing_needs_training_entity.year),
                DataKind.Single))

            .Append(_mlContext.Transforms.Categorical.OneHotEncoding(
                "DepartmentEncoded",
                nameof(month_staffing_needs_training_entity.department)))

            .Append(_mlContext.Transforms.Concatenate(
                "Features",
                "month_f",
                "year_f",
                nameof(month_staffing_needs_training_entity.avg_staff_present),
                nameof(month_staffing_needs_training_entity.avg_working_hours),
                nameof(month_staffing_needs_training_entity.avg_overtime_hours),
                "DepartmentEncoded"))

            .Append(_mlContext.Transforms.NormalizeMinMax("Features"));
    }

    // =====================================================
    // TRAIN BOTH MODELS (THREAD-SAFE)
    // =====================================================
    public async Task TrainAsync(
        IEnumerable<month_staffing_needs_training_entity> data,
        CancellationToken cancellationToken)
    {
        if (!data.Any()) return;

        await _trainingLock.WaitAsync(cancellationToken);
        try
        {
            await Task.Run(() =>
            {
                var dataView = _mlContext.Data.LoadFromEnumerable(data);

                // -------- Model A: Working hours --------
                var hoursPipeline =
                    BuildFeaturePipeline()
                    .Append(_mlContext.Regression.Trainers.Sdca(
                        labelColumnName:
                            nameof(month_staffing_needs_training_entity.total_working_hours_needed),
                        featureColumnName: "Features"));

                _hoursModel = hoursPipeline.Fit(dataView);

                _mlContext.Model.Save(
                    _hoursModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, HoursModelFile));

                // -------- Model B: Staff needed --------
                var staffPipeline =
                    BuildFeaturePipeline()
                    .Append(_mlContext.Regression.Trainers.Sdca(
                        labelColumnName:
                            nameof(month_staffing_needs_training_entity.total_staff_needed),
                        featureColumnName: "Features"));

                _staffModel = staffPipeline.Fit(dataView);

                _mlContext.Model.Save(
                    _staffModel,
                    dataView.Schema,
                    Path.Combine(ModelDir, StaffModelFile));

            }, cancellationToken);
        }
        finally
        {
            _trainingLock.Release();
        }
    }

    // =====================================================
    // PREDICT (COMBINED RESULT)
    // =====================================================
    public monthStaffingNeedsForecast Predict(
        month_staffing_needs_training_entity input)
    {
        if (_hoursModel == null || _staffModel == null)
            throw new InvalidOperationException("Models are not trained.");

        var dv = _mlContext.Data.LoadFromEnumerable(new[] { input });

        float hours =
            _mlContext.Data
                .CreateEnumerable<SinglePrediction>(
                    _hoursModel.Transform(dv), false)
                .First().Score;

        float staff =
            _mlContext.Data
                .CreateEnumerable<SinglePrediction>(
                    _staffModel.Transform(dv), false)
                .First().Score;

        return new monthStaffingNeedsForecast
        {
            total_working_hours_needed = hours,
            total_staff_needed = staff
        };
    }

    private class SinglePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}

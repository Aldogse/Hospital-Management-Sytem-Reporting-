using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.forecastService
{
    public class MonthPatientAdmissionForecastingService : BackgroundService
    {
        private readonly ILogger<MonthPatientAdmissionForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MonthPatientAdmissionForecastingService(
            ILogger<MonthPatientAdmissionForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Patient admission forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<monthPatientAdmissionPredictionService>();

                    // 1️⃣ Fetch training data from DB
                    var rawData = await db.month_patient_admission_forecasting_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ Map EF entity → ML.NET DTO (all floats)
                    var trainingData = rawData.Select(d => new month_patient_admission_forecasting_entity
                    {
                        month = d.month,
                        year = d.year,
                        prev_month_admission = d.prev_month_admission,
                        last_three_month_admission = d.last_three_month_admission,
                        last_sixth_month_admission = d.last_sixth_month_admission,
                        total_admission = d.total_admission
                    }).ToList();

                    // 3️⃣ Retrain the ML.NET model daily
                    await mlService.TrainAsync(trainingData, stoppingToken);
                    _logger.LogInformation("ML.NET patient admission model retrained at {time}", DateTime.UtcNow);

                    // 4️⃣ Forecast current month on the 5th
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool alreadyExists = await db.month_patient_admission_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear, stoppingToken);

                        if (!alreadyExists)
                        {
                            // Build forecast input
                            var lastMonth = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .FirstOrDefault();

                            var lastThreeMonthsAvg = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .Take(3)
                                .Average(x => x.total_admission);

                            var lastSixMonthsAvg = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .Take(6)
                                .Average(x => x.total_admission);

                            var input = new month_patient_admission_forecasting_entity
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                prev_month_admission = lastMonth?.total_admission ?? 0f,
                                last_three_month_admission = lastThreeMonthsAvg,
                                last_sixth_month_admission = lastSixMonthsAvg,
                                total_admission = 0f // target is ignored for prediction
                            };

                            // Predict
                            var prediction = mlService.ForecastSingleMonth(input);

                            var result = new month_patient_admission_forecast_result
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                total_admission = (int)prediction.total_admission
                            };

                            await db.month_patient_admission_forecast_result.AddAsync(result, stoppingToken);
                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Patient admission forecast saved for {month}/{year}", forecastMonth, forecastYear);
                        }
                        else
                        {
                            _logger.LogInformation("Patient admission forecast already exists for {month}/{year}", forecastMonth, forecastYear);
                        }
                    }

                    // 5️⃣ Wait 24 hours before next iteration
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in patient admission forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

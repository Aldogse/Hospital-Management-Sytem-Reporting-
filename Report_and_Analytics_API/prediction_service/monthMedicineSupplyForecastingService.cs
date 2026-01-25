using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast_results;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.forecastService
{
    public class monthMedicineSupplyForecastingService : BackgroundService
    {
        private readonly ILogger<monthMedicineSupplyForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public monthMedicineSupplyForecastingService(
            ILogger<monthMedicineSupplyForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Medicine supply forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<monthMedicineShortagePredictionService>();

                    // 1️⃣ FETCH TRAINING DATA
                    var rawData = await db.month_medicine_shortage_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No medicine shortage training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ MAP EF ENTITY → ML DTO
                    var trainingData = rawData.Select(d => new month_medicine_shortage_prediction_entity
                    {
                        training_id = (float)d.training_id,
                        med_id = (float)d.med_id,
                        month = (float)d.month,
                        year = (float)d.year,
                        current_stock = (float)d.current_stock,
                        avg_daily_use = (float)(d.avg_daily_use ?? 0),
                        total_dispensed_month = (float)(d.total_dispensed_month ?? 0),
                        expiring_within_30_days = d.expiring_within_30_days,
                        shortage_occured = d.shortage_occured ?? false
                    }).ToList();

                    // 3️⃣ RETRAIN ML MODEL DAILY
                    await mlService.TrainAsync(trainingData, stoppingToken);
                    _logger.LogInformation("Medicine shortage ML.NET model retrained at {time}", DateTime.UtcNow);

                    // 4️⃣ FORECAST CURRENT MONTH (RUN AFTER 5TH)
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool alreadyExists = await db.month_medicine_supply_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear, stoppingToken);

                        if (!alreadyExists)
                        {
                            // ✅ FIX: Get latest known data per medicine without SQL Join
                            var latestPerMed = rawData
                                .GroupBy(d => d.med_id)
                                .Select(g => new
                                {
                                    med_id = g.Key,
                                    latestYear = g.Max(x => x.year),
                                    latestMonth = g.Max(x => x.month)
                                })
                                .ToList();

                            // Filter client-side for latest record per medicine
                            var medicines = rawData
                                .Where(m => latestPerMed.Any(l =>
                                    l.med_id == m.med_id &&
                                    l.latestYear == m.year &&
                                    l.latestMonth == m.month))
                                .ToList();

                            foreach (var med in medicines)
                            {
                                var input = new month_medicine_shortage_prediction_entity
                                {
                                    training_id = (float)med.training_id,
                                    med_id = (float)med.med_id,
                                    month = forecastMonth,
                                    year = forecastYear,
                                    current_stock = (float)med.current_stock,
                                    avg_daily_use = (float)(med.avg_daily_use ?? 0),
                                    total_dispensed_month = (float)(med.total_dispensed_month ?? 0),
                                    expiring_within_30_days = med.expiring_within_30_days,
                                    shortage_occured = false // label unknown
                                };

                                var prediction = mlService.ForecastSingleMonth(input);

                                var result = new month_medicine_supply_forecast_result
                                {
                                    med_id = med.med_id,
                                    month = forecastMonth,
                                    year = forecastYear,
                                    avg_daily_use = (decimal?)prediction.avg_daily_use,
                                    shortage_occured = prediction.shortage_occured >= 0.5f
                                };

                                await db.month_medicine_supply_forecast_result.AddAsync(result, stoppingToken);
                            }

                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Medicine supply forecast saved for {month}/{year}", forecastMonth, forecastYear);
                        }
                        else
                        {
                            _logger.LogInformation("Medicine supply forecast already exists for {month}/{year}", forecastMonth, forecastYear);
                        }
                    }

                    // 5️⃣ WAIT 24 HOURS
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in medicine supply forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

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
    public class monthBedOccupancyForecastingService : BackgroundService
    {
        private readonly ILogger<monthBedOccupancyForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public monthBedOccupancyForecastingService(
            ILogger<monthBedOccupancyForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bed occupancy forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<BedOccupancyPredictionService>();

                    // 1️⃣ Fetch training data from DB
                    var rawData = await db.month_bed_occupancy_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No training data found. Waiting 1 hour before retry.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ Map EF entity to ML.NET DTO (all floats)
                    var trainingData = rawData.Select(d => new month_bed_occupancy_training_entity
                    {
                        month = d.month,
                        year = d.year,
                        total_beds = d.total_beds,
                        occupied_beds = d.occupied_beds,
                        recently_discharged = d.recently_discharged,
                        bed_occupancy_rate = d.bed_occupancy_rate,
                        broken_bed_rate = d.broken_bed_rate
                    }).ToList();

                    // 3️⃣ Retrain the ML.NET model daily
                    await mlService.TrainAsync(trainingData,stoppingToken);
                    _logger.LogInformation("ML.NET model retrained at {time}", DateTime.UtcNow);

                    // 4️⃣ Forecast current month on the 5th
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool alreadyExists = await db.month_bed_occupancy_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear, stoppingToken);

                        if (!alreadyExists)
                        {
                            // Total beds for forecasting
                            var totalBeds = await db.p_beds.CountAsync(b => b.status != null, stoppingToken);

                            var input = new month_bed_occupancy_training_entity
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                total_beds = totalBeds
                            };

                            // Predict next month
                            var prediction = mlService.ForecastSingleMonth(input);

                            var result = new month_bed_occupancy_forecast_result
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                predicted_occupied_beds = (int)prediction.PredictedOccupiedBeds,
                                predicted_recently_discharged = (int)prediction.PredictedRecentlyDischarged,
                                predicted_bed_occupancy_rate = prediction.PredictedBedOccupancyRate,
                                predicted_broken_bed_rate = prediction.PredictedBrokenBedRate
                            };

                            await db.month_bed_occupancy_forecast_result.AddAsync(result, stoppingToken);
                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Forecast saved for {month}/{year}", forecastMonth, forecastYear);
                        }
                        else
                        {
                            _logger.LogInformation("Forecast already exists for {month}/{year}", forecastMonth, forecastYear);
                        }
                    }

                    // 5️⃣ Wait 24 hours before next iteration
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in bed occupancy forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

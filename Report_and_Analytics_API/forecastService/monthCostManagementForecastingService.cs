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
    public class monthCostManagementForecastingService : BackgroundService
    {
        private readonly ILogger<monthCostManagementForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly SemaphoreSlim _trainingLock = new(1, 1);

        public monthCostManagementForecastingService(
            ILogger<monthCostManagementForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monthly cost forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<monthCostForecastService>();

                    // 1️⃣ Fetch training data
                    var rawData = await db.month_cost_management_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No cost training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ Map DB entity → ML.NET entity
                    var trainingData = rawData.Select(d => new month_cost_training_entity
                    {
                        month = d.month,
                        year = d.year,
                        previous_month_operational_cost = d.previous_month_operational_cost,
                        last_three_months_cost = d.last_three_months_cost,
                        last_six_months_cost = d.last_six_months_cost,
                        total_month_operational_cost = d.total_month_operational_cost
                    }).ToList();

                    // 3️⃣ Retrain model (async + thread-safe)
                    await _trainingLock.WaitAsync(stoppingToken);
                    try
                    {
                        await mlService.TrainAsync(trainingData, stoppingToken);
                        _logger.LogInformation("Cost prediction model retrained at {time}", DateTime.UtcNow);
                    }
                    finally
                    {
                        _trainingLock.Release();
                    }

                    // 4️⃣ Forecast current month (run after 5th day)
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool exists = await db.month_cost_management_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear,
                                stoppingToken);

                        if (!exists)
                        {
                            // Build forecast input
                            var previousMonthCost = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .FirstOrDefault()?.total_month_operational_cost ?? 0f;

                            var lastThreeMonthsAvg = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .Take(3)
                                .Average(x => x.total_month_operational_cost);

                            var lastSixMonthsAvg = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .Take(6)
                                .Average(x => x.total_month_operational_cost);

                            var input = new month_cost_training_entity
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                previous_month_operational_cost = previousMonthCost,
                                last_three_months_cost = lastThreeMonthsAvg,
                                last_six_months_cost = lastSixMonthsAvg,
                                total_month_operational_cost = 0f // target ignored
                            };

                            var prediction = mlService.Predict(input);

                            var result = new month_cost_management_forecast_result
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                month_forecasted_cost = prediction.month_forecasted_cost
                            };

                            await db.month_cost_management_forecast_result
                                .AddAsync(result, stoppingToken);

                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation(
                                "Monthly cost forecast saved for {month}/{year}",
                                forecastMonth,
                                forecastYear);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Monthly cost forecast already exists for {month}/{year}",
                                forecastMonth,
                                forecastYear);
                        }
                    }

                    // 5️⃣ Wait 24 hours before next iteration
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monthly cost forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

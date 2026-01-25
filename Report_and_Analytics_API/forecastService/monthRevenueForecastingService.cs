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
    public class MonthRevenueForecastingService : BackgroundService
    {
        private readonly ILogger<MonthRevenueForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MonthRevenueForecastingService(
            ILogger<MonthRevenueForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Revenue forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<monthRevenueForecastPredictionService>();

                    // 1️⃣ Fetch training data from DB
                    var rawData = await db.month_revenue_forecasting_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ Map EF entity → ML.NET DTO (all floats)
                    var trainingData = rawData.Select(d => new month_revenue_report_forecast_entity
                    {
                        month = (float)d.month,
                        year = (float)d.year,
                        total_patient = (float)d.total_patient,
                        pharmacy_total_transactions = (float)d.pharmacy_total_transactions,
                        average_pharmacy_sale_per_transaction = (float)d.average_pharmacy_sale_per_transaction,
                        total_revenue = (float)(d.total_revenue ?? 0),
                        average_bill_amount = (float)(d.average_bill_amount ?? 0)
                    }).ToList();

                    // 3️⃣ Retrain the ML.NET model daily
                    await mlService.TrainAsync(trainingData, stoppingToken);
                    _logger.LogInformation("ML.NET revenue model retrained at {time}", DateTime.UtcNow);

                    // 4️⃣ Forecast current month on the 5th
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool alreadyExists = await db.month_revenue_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear, stoppingToken);

                        if (!alreadyExists)
                        {
                            // Build forecast input using previous months
                            var lastMonth = trainingData
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .FirstOrDefault();

                            var input = new month_revenue_report_forecast_entity
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                total_patient = lastMonth?.total_patient ?? 0f,
                                pharmacy_total_transactions = lastMonth?.pharmacy_total_transactions ?? 0f,
                                average_pharmacy_sale_per_transaction = lastMonth?.average_pharmacy_sale_per_transaction ?? 0f,
                                total_revenue = 0f // ML target ignored for prediction
                            };

                            // Predict
                            var prediction = mlService.ForecastSingleMonth(input);


                            var result = new month_revenue_forecast_result
                            {
                                month = forecastMonth,
                                year = forecastYear,
                                total_revenue = (decimal)prediction.total_revenue,
                                pharmacy_total_transactions = (int)prediction.pharmacy_total_transactions,
                                average_bill_amount = (decimal)prediction.average_bill_amount
                            };

                            await db.month_revenue_forecast_result.AddAsync(result,stoppingToken);
                            await db.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Revenue forecast saved for {month}/{year}", forecastMonth, forecastYear);
                        }
                        else
                        {
                            _logger.LogInformation("Revenue forecast already exists for {month}/{year}", forecastMonth, forecastYear);
                        }
                    }

                    // 5️⃣ Wait 24 hours before next iteration
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in revenue forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

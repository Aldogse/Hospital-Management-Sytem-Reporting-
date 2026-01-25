using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast;
using APIResponses.forecast_results;
using APIResponses.Historical_report.training_models_prediction;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.forecastService
{
    public class MonthInsuranceClaimsStatusForecastingService : BackgroundService
    {
        private readonly ILogger<MonthInsuranceClaimsStatusForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MonthInsuranceClaimsStatusForecastingService(
            ILogger<MonthInsuranceClaimsStatusForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Insurance claims forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider
                        .GetRequiredService<monthInsuranceClaimsStatusPredictionService>();

                    // =====================================================
                    // 1️⃣ FETCH TRAINING DATA
                    // =====================================================
                    var rawData = await db.month_insurance_claims_status_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No insurance claims training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // =====================================================
                    // 2️⃣ MAP EF ENTITY → ML DTO (FLOATS)
                    // =====================================================
                    var trainingData = rawData.Select(d => new month_hospital_claims_status_entity
                    {
                        insurance_provider_id = d.insurance_provider_id,
                        month = d.month,
                        year = d.year,
                        total_claims = d.total_claims,
                        last_month_approved_claims = d.last_month_approved_claims,
                        last_month_denied_claims = d.last_month_denied_claims,
                        total_claim_approved = d.total_claim_approved ?? 0,
                        total_claim_denied = d.total_claim_denied ?? 0
                    }).ToList();

                    // =====================================================
                    // 3️⃣ TRAIN ML MODELS (DAILY)
                    // =====================================================
                    await mlService.TrainAsync(trainingData, stoppingToken);
                    _logger.LogInformation(
                        "Insurance claims ML model retrained at {time}",
                        DateTime.UtcNow);

                    // =====================================================
                    // 4️⃣ FORECAST CURRENT MONTH (AFTER DAY 5)
                    // =====================================================
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        var providers = await db.insurance_provider
                            .AsNoTracking()
                            .Select(p => p.insurance_provider_id)
                            .ToListAsync(stoppingToken);

                        foreach (var providerId in providers)
                        {
                            bool alreadyExists = await db.month_insurance_claims_status_forecast_result
                                .AnyAsync(r =>
                                    r.insurance_provider_id == providerId &&
                                    r.month == forecastMonth &&
                                    r.year == forecastYear,
                                    stoppingToken);

                            if (alreadyExists)
                            {
                                _logger.LogInformation(
                                    "Forecast already exists for provider {provider}, {month}/{year}",
                                    providerId, forecastMonth, forecastYear);
                                continue;
                            }

                            // Get latest data for this provider
                            var lastRecord = trainingData
                                .Where(x => x.insurance_provider_id == providerId)
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .FirstOrDefault();

                            if (lastRecord == null) continue;

                            var input = new month_hospital_claims_status_entity
                            {
                                insurance_provider_id = providerId,
                                month = forecastMonth,
                                year = forecastYear,
                                total_claims = lastRecord.total_claims,
                                last_month_approved_claims = lastRecord.last_month_approved_claims,
                                last_month_denied_claims = lastRecord.last_month_denied_claims,
                                total_claim_approved = lastRecord.total_claim_approved,
                                total_claim_denied = lastRecord.total_claim_denied,
                            };

                            var prediction = mlService.ForecastSingleMonth(input);

                            var result = new month_insurance_claims_status_forecast_result
                            {
                                insurance_provider_id = providerId,
                                month = forecastMonth,
                                year = forecastYear,
                                total_claims = (int)prediction.total_claims,
                                total_claim_approved = (int)Math.Round(prediction.total_claim_approved),
                                total_claim_denied = (int)Math.Round(prediction.total_claim_denied)
                            };

                            await db.month_insurance_claims_status_forecast_result
                                .AddAsync(result, stoppingToken);
                        }

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Insurance claims forecast completed for {month}/{year}",
                            forecastMonth, forecastYear);
                    }

                    // =====================================================
                    // 5️⃣ WAIT 24 HOURS
                    // =====================================================
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in insurance claims forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

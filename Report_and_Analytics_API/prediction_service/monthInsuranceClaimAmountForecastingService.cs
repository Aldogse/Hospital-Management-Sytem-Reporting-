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
    public class monthInsuranceClaimAmountForecastingService : BackgroundService
    {
        private readonly ILogger<monthInsuranceClaimAmountForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public monthInsuranceClaimAmountForecastingService(
            ILogger<monthInsuranceClaimAmountForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Insurance claim AMOUNT forecasting service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider
                        .GetRequiredService<monthInsuranceClaimAmountPredictionService>();

                    // =====================================================
                    // 1️⃣ FETCH TRAINING DATA
                    // =====================================================
                    var rawData = await db.month_insurance_claim_amount_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No claim amount training data found. Retrying in 1 hour.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // =====================================================
                    // 2️⃣ MAP EF ENTITY → ML ENTITY (FLOATS)
                    // =====================================================
                    var trainingData = rawData.Select(d => new month_hospital_claims_amount_entity
                    {
                        insurance_provider_id = d.insurance_provider_id,
                        month = d.month,
                        year = d.year,
                        total_claim_approved_amount = (float)d.total_claim_approved_amount,
                        total_claim_declined_amount = (float)d.total_claim_declined_amount,
                        last_month_total_claim_approved_amount = (float)d.last_month_total_claim_approved_amount,
                        last_month_total_claim_declined_amount = (float)d.last_month_total_claim_declined_amount,
                        total_claim_amount_submitted = (float)d.total_claim_amount_submitted
                    }).ToList();

                    // =====================================================
                    // 3️⃣ TRAIN MODEL (DAILY)
                    // =====================================================
                    await mlService.TrainAsync(trainingData, stoppingToken);

                    _logger.LogInformation(
                        "Insurance claim amount ML model retrained at {time}",
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
                            bool alreadyExists =
                                await db.month_insurance_claim_amount_forecast_result
                                    .AnyAsync(r =>
                                        r.insurance_provider_id == providerId &&
                                        r.month == forecastMonth &&
                                        r.year == forecastYear,
                                        stoppingToken);

                            if (alreadyExists)
                            {
                                _logger.LogInformation(
                                    "Claim amount forecast already exists for provider {provider}, {month}/{year}",
                                    providerId, forecastMonth, forecastYear);
                                continue;
                            }

                            // Get latest historical record
                            var lastRecord = trainingData
                                .Where(x => x.insurance_provider_id == providerId)
                                .OrderByDescending(x => x.year)
                                .ThenByDescending(x => x.month)
                                .FirstOrDefault();

                            if (lastRecord == null) continue;

                            var input = new month_hospital_claims_amount_entity
                            {
                                insurance_provider_id = providerId,
                                month = forecastMonth,
                                year = forecastYear,
                                last_month_total_claim_approved_amount = lastRecord.total_claim_approved_amount,
                                last_month_total_claim_declined_amount = lastRecord.total_claim_declined_amount,
                                total_claim_amount_submitted = lastRecord.total_claim_amount_submitted,
                                total_claim_approved_amount = lastRecord.total_claim_approved_amount,
                                total_claim_declined_amount = lastRecord.total_claim_declined_amount
                            };

                            var prediction = mlService.ForecastSingleMonth(input);

                            var result = new month_insurance_claim_amount_forecast_result
                            {
                                insurance_provider_id = providerId,
                                month = forecastMonth,
                                year = forecastYear,
                                total_claim_approved_amount =
                                    (decimal)Math.Round(prediction.total_claim_approved_amount, 2),
                                total_claim_declined_amount =
                                    (decimal)Math.Round(prediction.total_claim_declined_amount, 2)
                            };

                            await db.month_insurance_claim_amount_forecast_result
                                .AddAsync(result, stoppingToken);
                        }

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Insurance claim amount forecast completed for {month}/{year}",
                            forecastMonth, forecastYear);
                    }

                    // =====================================================
                    // 5️⃣ WAIT 24 HOURS
                    // =====================================================
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in insurance claim amount forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

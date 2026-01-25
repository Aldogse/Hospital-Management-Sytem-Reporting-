using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIResponses.DTO;
using APIResponses.forecast_results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.forecastService
{
    public class monthStaffingNeedsForecastingService : BackgroundService
    {
        private readonly ILogger<monthStaffingNeedsForecastingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public monthStaffingNeedsForecastingService(
            ILogger<monthStaffingNeedsForecastingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Staffing forecasting background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var mlService = scope.ServiceProvider.GetRequiredService<monthStaffingNeedsPredictionService>();

                    // 1️⃣ Fetch training data from DB
                    var rawData = await db.month_staffing_needs_forecast_training_data
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    if (!rawData.Any())
                    {
                        _logger.LogInformation("No training data found. Waiting 1 hour before retry.");
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                        continue;
                    }

                    // 2️⃣ Map EF entity to ML.NET DTO
                    var trainingData = rawData.Select(d => new month_staffing_needs_training_entity
                    {
                        month = d.month,
                        year = d.year,
                        department = d.department,
                        avg_staff_present = (float)d.avg_staff_present,
                        avg_working_hours = (float)d.avg_working_hours,
                        avg_overtime_hours = (float)d.avg_overtime_hours,
                        total_working_hours_needed = (float)d.total_working_hours_needed,
                        total_staff_needed = (float)d.total_staff_needed
                    }).ToList();

                    // 3️⃣ Retrain the ML.NET model daily
                    await mlService.TrainAsync(trainingData, stoppingToken);
                    _logger.LogInformation("Staffing ML.NET model retrained at {time}", DateTime.UtcNow);

                    // 4️⃣ Forecast current month on the 5th
                    var today = DateTime.UtcNow;
                    if (today.Day >= 5)
                    {
                        int forecastMonth = today.Month;
                        int forecastYear = today.Year;

                        bool alreadyExists = await db.month_staffing_needs_forecast_result
                            .AnyAsync(r => r.month == forecastMonth && r.year == forecastYear, stoppingToken);

                        if (!alreadyExists)
                        {
                            // Prepare sample inputs for each department
                            var departments = await db.month_staffing_needs_forecast_training_data
                                .Select(d => d.department)
                                .Distinct()
                                .ToListAsync(stoppingToken);

                            foreach (var department in departments)
                            {
                                var avgStats = await db.month_staffing_needs_forecast_training_data
                                    .Where(d => d.department == department)
                                    .GroupBy(d => 1)
                                    .Select(g => new
                                    {
                                        avgStaff = g.Average(x => x.avg_staff_present),
                                        avgHours = g.Average(x => x.avg_working_hours),
                                        avgOvertime = g.Average(x => x.avg_overtime_hours)
                                    }).FirstOrDefaultAsync(stoppingToken);

                                var input = new month_staffing_needs_training_entity
                                {
                                    month = forecastMonth,
                                    year = forecastYear,
                                    department = department,
                                    avg_staff_present = (float)avgStats?.avgStaff,
                                    avg_working_hours = (float)avgStats?.avgHours,
                                    avg_overtime_hours = (float)avgStats?.avgOvertime,
                                };

                                // Predict staffing needs
                                var prediction = mlService.Predict(input);

                                var result = new month_staffing_needs_forecast_result
                                {
                                    month = forecastMonth,
                                    year = forecastYear,
                                    department = department,
                                    total_working_hours = (decimal)prediction.total_working_hours_needed,
                                    total_staff_needed = (int)prediction.total_staff_needed
                                };

                                await db.month_staffing_needs_forecast_result.AddAsync(result, stoppingToken);
                            }

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
                    _logger.LogError(ex, "Error in staffing forecasting service.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }
    }
}

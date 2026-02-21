
using APIResponses.Training_Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthBedOccupancyTrainingDataExtraction : BackgroundService
    {
        private readonly ILogger<monthBedOccupancyTrainingDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthBedOccupancyTrainingDataExtraction(ILogger<monthBedOccupancyTrainingDataExtraction>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IpropertyRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 1)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthBedOccupancyTrainingDataExtraction", date.Month, date.Year))
                        {
                            await MonthBedOccupancyTrainingDataExtraction(database, repo);
                            await jobRepo.markAsRunThisMonth("MonthBedOccupancyTrainingDataExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month.");
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }

        }

        private async Task MonthBedOccupancyTrainingDataExtraction(ReportDbContext reportDb,IpropertyRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    int year = DateTime.Now.Year - 1;

                    var monthData = await repository.getMonthsAdmissionData(year);

                    if (monthData == null || !monthData.Any())
                    {
                        _logger.LogCritical($"Expecting data but nothing was extracted for {year}");
                        return;
                    }

                    var report = monthData.Select(i => new month_bed_occupancy_training_data
                    {
                        month = i.month,
                        year = i.year,
                        occupied_beds = i.occupied_beds,
                        total_beds = i.total_beds,
                        recently_discharged = i.recently_discharged,
                        bed_occupancy_rate = ((float)i.occupied_beds / i.total_beds) * 100,
                        broken_bed_rate = ((float)i.broken_beds / i.total_beds) * 100
                    }).ToList();


                    await reportDb.month_bed_occupancy_training_data.AddRangeAsync(report);
                    await reportDb.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Attempt {attempts}: {ex.Message}");

                    if(attempts >= maxAttempts)
                    {
                        _logger.LogError($"Maximum number of attempt has been reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));

                }
            }
        }
    }
}

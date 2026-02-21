
using APIResponses.Historical_report.Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearAdmissionAndBeddingSummary : BackgroundService
    {
        private readonly ILogger<yearAdmissionAndBeddingSummary> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public yearAdmissionAndBeddingSummary(ILogger<yearAdmissionAndBeddingSummary>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IpropertyRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    int year = DateTime.Now.Year - 1;

                    if (DateTime.Now.Month == 1 && DateTime.Now.Day >= 10)
                    {
                        if (!await jobRepo.hasRunThisYear("YearAdmissionAndBeddingDataService",year))
                        {
                            await YearAdmissionAndBeddingDataService(database, repo);
                            await jobRepo.markAsRunThisYear("YearAdmissionAndBeddingDataService",year);
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Job failed: {ex.Message} ");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //runs every 10th of january
        private async Task YearAdmissionAndBeddingDataService(ReportDbContext reportDb,IpropertyRepository repository)
        {
            int att = 0;
            int maxAtt = 5;

            while (att < maxAtt)
            {
                try
                {
                    att++;
                    var preYear = DateTime.Now.AddYears(-1);

                    var reportData = await repository.getYearAdmissionsAndDischargeReport(preYear.Year);


                    var dataNeeded = new yearly_admission_and_discharge_report()
                    {
                        year = preYear.Year,
                        available_beds = ((float)reportData.available_beds / reportData.total_beds) * 100,
                        broken_beds = ((float)reportData.broken_beds / reportData.total_beds) * 100,
                        total_beds = reportData.total_beds,
                        occupied_beds = ((float)reportData.occupied_beds / reportData.total_beds) * 100
                    };

                    if (reportData == null)
                    {
                        _logger.LogInformation($"Expecting data but nothing was extracted for {preYear.Year}");
                        return;
                    }

                    await reportDb.yearly_admission_and_discharge_report.AddAsync(dataNeeded);
                    await reportDb.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");

                    if (att == maxAtt)
                    {
                        _logger.LogError("Maximum attempts has been reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

    }
}

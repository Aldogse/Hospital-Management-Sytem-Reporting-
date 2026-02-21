
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearlyClaimReportDataExtraction : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ILogger<yearlyClaimReportDataExtraction> _logger;

        public yearlyClaimReportDataExtraction(IServiceScopeFactory serviceScope,ILogger<yearlyClaimReportDataExtraction>logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IinsuranceClaimRepository>();
                    var joblogRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    var date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if (!await joblogRepository.hasRunThisYear("YearlyClaimReportDataExtraction",date.Year))
                        {
                            await YearlyClaimReportDataExtraction(database,repository);
                            await joblogRepository.markAsRunThisYear("YearlyClaimReportDataExtraction",date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the year");
                            await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }   

        }

        //RUNS EVERY 5TH OF JANUARY
        private async Task YearlyClaimReportDataExtraction(ReportDbContext reportDbContext,IinsuranceClaimRepository claimRepository)
        {
            int attempts = 0;
            int maxAtt = 5;
            while (attempts < maxAtt)
            {
                try
                {
                    attempts++;
                    DateTime prevYear = DateTime.Now.AddYears(-1);
                    var yearReport = await claimRepository.getYearClaimReport(prevYear.Year);

                    if(yearReport == null)
                    {
                        _logger.LogInformation($"Expecting data but nothing was extracted for {prevYear.Year}.");
                        return;
                    }
                    await reportDbContext.yearly_claim_report.AddAsync(yearReport);
                    await reportDbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempt {attempts}: {ex.Message}");

                    if(attempts == maxAtt)
                    {
                        _logger.LogError("Max attempts has been reached");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}

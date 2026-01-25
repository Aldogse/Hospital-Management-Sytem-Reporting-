
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearBillingSummaryReport : BackgroundService
    {
        private readonly ILogger<yearBillingSummaryReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public yearBillingSummaryReport(ILogger<yearBillingSummaryReport>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    int year = DateTime.Now.Year;

                    if (DateTime.Now.Month == 1 && DateTime.Now.Day >= 10)
                    {
                        if (!await jobRepo.hasRunThisYear("YearSummaryReportService",year))
                        {
                            await YearSummaryReportService(database,repo);
                            await jobRepo.markAsRunThisYear("YearSummaryReportService",year);
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
                _logger.LogError($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //runs every 10th of january
        private async Task YearSummaryReportService(ReportDbContext reportDb,IjournalRepository repository)
        {
            try
            {
                int year = DateTime.Now.Year - 1;

                var yearReport = await repository.getYearBillingReport(year);

                if (yearReport == null)
                {
                    {
                        _logger.LogInformation($"Expecting data but nothing was received for {year}");
                        return;
                    }
                }

                await reportDb.yearly_billing_report.AddAsync(yearReport);
                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }

        }
    }
}

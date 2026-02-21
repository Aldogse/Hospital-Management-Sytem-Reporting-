
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthBillingSummaryReportService : BackgroundService
    {
        private readonly ILogger<monthBillingSummaryReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthBillingSummaryReportService(ILogger<monthBillingSummaryReportService>logger,IServiceScopeFactory serviceScope)
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
                    var repo = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    if (DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthBillingSummaryReportGenerator", date.Month, date.Year))
                        {
                            await MonthBillingSummaryReportGenerator(database, repo);
                            await jobRepo.markAsRunThisMonth("MonthBillingSummaryReportGenerator", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month.");
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }

            }
        }

        //RUNS EVERY 5TH OF THE FOLLOWING MONTH
        private async Task MonthBillingSummaryReportGenerator(ReportDbContext database,IjournalRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;
            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    DateTime prevMonth = DateTime.Now.AddMonths(-1);
                    var monthReport = await repository.getMonthBillingReport(prevMonth.Month, prevMonth.Year);

                    if (monthReport == null)
                    {
                        _logger.LogCritical($"Expecting data but nothing was found for {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await database.month_billing_report.AddAsync(monthReport);
                    await database.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempt{attempts}: {ex.Message}");

                    if(attempts >= maxAttempts)
                    {
                        _logger.LogError("Maximum attempts has been reached");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}

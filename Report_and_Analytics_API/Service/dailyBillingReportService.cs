
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class dailyBillingReportService : BackgroundService
    {
        private readonly ILogger<dailyBillingReportService> _logger;
        private readonly IServiceScopeFactory _scope;

        public dailyBillingReportService(ILogger<dailyBillingReportService>logger,IServiceScopeFactory scope)
        {
            _logger = logger;
            _scope = scope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IjournalRepository>();

                    DateTime now = DateTime.Now;
                    DateTime nextMidnight = now.Date.AddDays(1);
                    TimeSpan delay = now - nextMidnight;                 

                    await DailyBillingSummaryReport(database, repo);
                    await Task.Delay(delay,stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        //RUNS EVERY TIME THE DAY ENDS
        private async Task DailyBillingSummaryReport(ReportDbContext reportDb,IjournalRepository repo)
        {
            int attempts = 0;
            int maxAttempts = 5;
            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    DateOnly prevDay = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);
                    var report = await repo.getDailyBillingReport(prevDay);

                    if (report == null)
                    {
                        _logger.LogCritical($"Expecting data but nothing was extracted for {prevDay}");
                        return;
                    }

                    await reportDb.daily_billing_report.AddAsync(report);
                    await reportDb.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempt {attempts}:{ex.Message}");

                    if (attempts >= maxAttempts)
                    {
                        _logger.LogInformation(message: "Maximum attempts has been reached");
                        throw;
                    }                   
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
           
        }
    }
}

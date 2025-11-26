
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class dailyBedsUtilizationReportService : BackgroundService
    {
        private readonly ILogger<dailyBedsUtilizationReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public dailyBedsUtilizationReportService(ILogger<dailyBedsUtilizationReportService>logger,IServiceScopeFactory serviceScope)
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


                    DateTime now = DateTime.Now;
                    DateTime nextMidnight = DateTime.Now.AddDays(1);
                    TimeSpan delay = now - nextMidnight;
                    await Task.Delay(delay);

                    await DailyBedsUtilizationReportService(database,repo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        //RUNS EVERYDAY
        private async Task DailyBedsUtilizationReportService(ReportDbContext reportDb,IpropertyRepository repository)
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(-1);
                var results = await repository.getDailyBedsUtilizationReport(date);

                if (results == null)
                {
                    _logger.LogWarning($"Expecting data but nothing was extracted for {date}");
                    return;
                }

                await reportDb.daily_beds_utilization_report.AddAsync(results);
                await reportDb.SaveChangesAsync();
                _logger.LogInformation($"Records successfully stored for {date}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

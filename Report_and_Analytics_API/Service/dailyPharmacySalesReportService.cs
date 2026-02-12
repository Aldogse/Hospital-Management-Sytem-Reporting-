
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class dailyPharmacySalesReportService : BackgroundService
    {
        private readonly ILogger<dailyPharmacySalesReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public dailyPharmacySalesReportService(ILogger<dailyPharmacySalesReportService>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var repo = scope.ServiceProvider.GetRequiredService<IjournalRepository>();

                while (!stoppingToken.IsCancellationRequested)
                {

                    DateTime now = DateTime.Now;
                    DateTime nextMidnight = DateTime.Now.AddDays(1);
                    TimeSpan delay = now - nextMidnight;
                    await Task.Delay(delay, stoppingToken);

                    await DailyPharmacySalesReportService(db,repo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //RUNS EVERY DAY 
        private async Task DailyPharmacySalesReportService(ReportDbContext reportDb, IjournalRepository repository)
        {
            DateTime date = DateTime.Now.AddDays(-1);
            try
            {
                var salesReport = await repository.getDailyPharmacySalesReport(date.Month,date.Year,date.Day);

                if(salesReport == null)
                {
                    _logger.LogInformation($"No sales report extracted for {date}");
                    return;
                }

                await reportDb.daily_pharmacy_sales.AddAsync(salesReport);
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

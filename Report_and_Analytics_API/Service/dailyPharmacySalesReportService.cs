
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
                    DateTime nextMidnight = now.Date.AddDays(1);
                    TimeSpan delay = now - nextMidnight;
                    
                    await DailyPharmacySalesReportService(db,repo);
                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        //RUNS EVERY DAY 
        private async Task DailyPharmacySalesReportService(ReportDbContext reportDb, IjournalRepository repository)
        {
            DateTime date = DateTime.Now.AddDays(-1);
            int attempts = 0;
            int maxAttempts = 5;
            while (attempts <= maxAttempts)
            {
                try
                {
                    attempts++;

                    var salesReport = await repository.getDailyPharmacySalesReport(date.Month, date.Year, date.Day);

                    if (salesReport == null)
                    {
                        _logger.LogInformation($"No sales report extracted for {date}");
                        return;
                    }

                    await reportDb.daily_pharmacy_sales.AddAsync(salesReport);
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

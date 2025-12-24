
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearPharmacySalesReport : BackgroundService
    {
        private readonly ILogger<yearPharmacySalesReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public yearPharmacySalesReport(ILogger<yearPharmacySalesReport>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var repository = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                int year = DateTime.Now.Year - 1;

                if (DateTime.Now.Month == 1 && DateTime.Now.Day >= 5) 
                {
                    if (!await jobRepo.hasRunThisYear("YearPharmacySalesReportService", year))
                    {
                        await YearPharmacySalesReportService(repository, database);
                        await jobRepo.markAsRunThisYear("YearPharmacySalesReportService", year);
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error: {ex.Message}");
                return;
            }
        }

        //RUNS EVERTY 5TH of the following year
        private async Task YearPharmacySalesReportService(IjournalRepository repository,ReportDbContext database)
        {
            try
            {
                int year = DateTime.Now.Year - 1;

                var yearSaleData = await repository.getYearPharmacySales(year);

                if(yearSaleData == null)
                {
                    _logger.LogCritical($"Expecting data but nothing was extracted for {year}");
                    return;
                }

                await database.yearly_pharmacy_sales_report.AddAsync(yearSaleData);
                await database.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

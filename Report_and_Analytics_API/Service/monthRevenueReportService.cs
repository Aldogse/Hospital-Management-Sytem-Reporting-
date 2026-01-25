
using APIResponses.Historical_report.Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthRevenueReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ILogger<monthRevenueReportService> _logger;

        public monthRevenueReportService(IServiceScopeFactory serviceScope, ILogger<monthRevenueReportService>logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthRevenueDataExtraction", date.Month, date.Year))
                        {
                            await MonthRevenueDataExtraction(database,repository);
                            await jobRepo.markAsRunThisMonth("MonthRevenueDataExtraction", date.Month, date.Year);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task MonthRevenueDataExtraction(ReportDbContext reportDb, IjournalRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.Now.AddMonths(-1);
                var servicesRevenue = await repository.getMonthBillRevenueReport(prevMonth.Month, prevMonth.Year);
                var pharmacyRevenue = await repository.getMonthPharmacyTotalSales(prevMonth.Month, prevMonth.Year);

                if (servicesRevenue == null || pharmacyRevenue == null)
                {
                    _logger.LogInformation($"Expecting data but nothing was extracted for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                var monthReport = new month_revenue_report()
                {
                    last_update_date = DateTime.Now,
                    month = prevMonth.Month,
                    year = prevMonth.Year,
                    pharmacy_revenue = pharmacyRevenue,
                    service_revenue = servicesRevenue,
                    total_revenue = pharmacyRevenue + servicesRevenue
                };

                await reportDb.month_revenue_report.AddAsync(monthReport);
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

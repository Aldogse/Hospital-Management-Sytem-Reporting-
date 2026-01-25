
using APIResponses.Historical_report.Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthOperationalCostReportService : BackgroundService
    {
        private readonly ILogger<monthOperationalCostReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthOperationalCostReportService(ILogger<monthOperationalCostReportService>logger,IServiceScopeFactory serviceScope)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    //this is gonna be on 5, 2 is for testing purposed
                    if(DateTime.UtcNow.Day >= 2)
                    {
                        if(!await jobRepository.hasRunThisMonth("MonthOperationalCostReportService",date.Month,date.Year))
                        {
                            await MonthOperationalCostReportService(database,repository);
                            await jobRepository.markAsRunThisMonth("MonthOperationalCostReportService", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:$"Job already run this month");
                            return;
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
                _logger.LogError(message:$"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //runs every 5th of the following month
        private async Task MonthOperationalCostReportService(ReportDbContext database,IjournalRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                var disposedMedicineCost = await repository.getMonthTotalMedicineDisposedCost(prevMonth.Month, prevMonth.Year);
                var totalReceiptsRecorded = await repository.getMonthTotalReceiptRecorded(prevMonth.Month, prevMonth.Year);
                var totalSalaryPaid = await repository.getMonthTotalGrossPaid(prevMonth.Month, prevMonth.Year);
                var monthTotalOperationalCost = disposedMedicineCost + totalReceiptsRecorded + totalSalaryPaid;

                var operationalCostReport = new month_operational_records_report()
                {
                    created_at = DateTime.UtcNow,
                    total_disposed_medicine_cost = disposedMedicineCost,
                    total_gross_paid = totalSalaryPaid,
                    total_receipts_vendor_cost = totalReceiptsRecorded,
                    total_operational_cost = monthTotalOperationalCost,
                };

                await database.month_operational_records_report.AddAsync(operationalCostReport);
                await database.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }
    }
}

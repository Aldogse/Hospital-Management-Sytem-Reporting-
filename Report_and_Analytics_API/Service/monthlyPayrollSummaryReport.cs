
using APIResponses.Historical_report;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class monthlyPayrollSummaryReport : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<monthlyPayrollSummaryReport> _logger;

        public monthlyPayrollSummaryReport(IServiceScopeFactory serviceScopeFactory,ILogger<monthlyPayrollSummaryReport>logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var reportDB = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var hrRepo = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();

                await monthlyPayrollExtraction(reportDB, hrRepo);

                await Task.Delay(TimeSpan.FromDays(12), stoppingToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //EXTRACTION AFTER END OF THE CURRENT MONTH
        private async Task monthlyPayrollExtraction(ReportDbContext reportDb,IhrEmployeeInformation hrRepo)
        {
            try
            {
                var prevMonth = DateTime.Now;
                var payrolls = await reportDb.payrollinformation.ToListAsync();

                foreach (var item in payrolls)
                {
                    bool exist = await reportDb.employeePayrollMonthReports
                        .AnyAsync(i => i.month == item.payPeriodStartDate.Month && i.year == item.payPeriodStartDate.Year);

                    if (!exist)
                    {
                        var monthReport = new employeePayrollMonthReport()
                        {
                            employeeId = item.employeeId,
                            month = prevMonth.Month,
                            year = prevMonth.Year,
                            monthOvertimeHours = await hrRepo.getMonthOvertimeHours(item.employeeId,prevMonth.Month,prevMonth.Year),
                            monthTotalHoursWorked = await hrRepo.getMonthTotalHoursWorked(item.employeeId, prevMonth.Month, prevMonth.Year),
                            monthTotalWage = await hrRepo.getMonthTotalWage(item.employeeId, prevMonth.Month, prevMonth.Year)
                        };  

                        await reportDb.employeePayrollMonthReports.AddAsync(monthReport);
                    }
                    await reportDb.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error extracting");
                throw new Exception(ex.Message);
            }
        }
    }
}

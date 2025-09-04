
using APIResponses.Historical_report;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class yearlyPayrollSummaryReport : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<yearlyPayrollSummaryReport> _logger;

        public yearlyPayrollSummaryReport(IServiceScopeFactory serviceScopeFactory,ILogger<yearlyPayrollSummaryReport> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var reportDb = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
            var hrRepo = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();

            await yearlyPayrollExtraction(reportDb,hrRepo);

            await Task.Delay(TimeSpan.FromDays(172), stoppingToken);
        }


        //EXTRACTION AFTER THE END OF YEAR 
        private async Task yearlyPayrollExtraction(ReportDbContext reportDb,IhrEmployeeInformation hrRepo)
        {
            try
            {
                var yearToExtract = DateTime.Now.AddYears(-1);

                var yearPayrollRecords = await reportDb.employeePayrollMonthReports
                    .Where(i => i.year == yearToExtract.Year).ToListAsync();

                foreach(var item in yearPayrollRecords)
                {
                    var employeePayrollYearRecords = new employeeAnnualPayrollReport()
                    {
                        employeeId = item.employeeId,
                        year = item.year,
                        yearTotalHoursWorked = await hrRepo.yearTotalHoursWorked(item.employeeId,item.year),
                        yearTotalOvertimeHoursWorked = await hrRepo.yearTotalOvertimeHoursWorked(item.employeeId,item.year),
                        yearTotalWage = await hrRepo.yearTotalWage(item.employeeId, item.year)
                    };

                    await reportDb.employeeAnnualPayrollReports.AddAsync(employeePayrollYearRecords);
                }

                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error during extraction");
                throw new Exception(ex.Message);
            }
        }
    }
}

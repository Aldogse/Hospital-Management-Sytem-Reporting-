
using APIResponses.Historical_report;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.Repository;

namespace Report_and_Analytics_API.Service
{
    public class payCycleInformationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ILogger<payCycleInformationService> _logger;

        public payCycleInformationService(IServiceScopeFactory serviceScope,ILogger<payCycleInformationService>logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var reportDbContext =  scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var employeeInformation2 = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();
                var historicalDbContext = scope.ServiceProvider.GetRequiredService<HistoricalReportDbContext>();
                var employeeInformation1 = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();

                if (DateTime.Now.Day >= 13)
                {
                    await payrollExtraction(reportDbContext,historicalDbContext,employeeInformation1,employeeInformation2,stoppingToken);
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }
        }



        private async Task payrollExtraction(ReportDbContext reportDbContext,
                                             HistoricalReportDbContext historicalReportDbContext,
                                             IhrEmployeeInformation employeeInformation1,
                                             IhrEmployeeInformation employeeInformation2,
                                             CancellationToken token)
        {
            try
            {
                var payrolls = await reportDbContext.hr_payroll.Where(i => i.pay_period_start.Month == DateTime.Now.Month)
                    .ToListAsync();

                foreach(var item in payrolls)
                {
                    var employee = await employeeInformation1.getEmployeeInformation(item.employee_id);

                    //check if the date is already stored in the database
                    bool exist = await historicalReportDbContext.payrollInformation
                        .AnyAsync(i => i.payPeriodStartDate == item.pay_period_start);

                    if (!exist)
                    {
                        var payrollData = new employeeReportFinalData()
                        {
                            employeeId = item.employee_id,
                            employeeName = $"{employee.first_name} {employee.middle_name} {employee.last_name}",
                            payPeriodStartDate = item.pay_period_start,
                            overtimePay = await employeeInformation1.payCycleOvertimeHoursPaidAmount(item.employee_id,item.pay_period_start),
                            overtimeHours = await employeeInformation2.payCycleOvertimeHours(item.employee_id,item.pay_period_start),
                            payCycleGrossPay = await employeeInformation1.payCycleGrossPay(item.employee_id,item.pay_period_start),
                            GrossPay = await employeeInformation2.yearToDateGrossPay(item.employee_id,item.pay_period_start.Year),
                            payCycleTotalDeductions = await employeeInformation1.payCycleTotalDeductions(item.employee_id,item.pay_period_start),
                            ytdTotalDeductions = await employeeInformation2.yearToDateTotalDeductions(item.employee_id,item.pay_period_start.Year),
                            payCycleNetpay = await employeeInformation1.payCycleNetPay(item.employee_id,item.pay_period_start),
                            ytdNetPay = await employeeInformation2.yearToDateNetPay(item.employee_id,item.pay_period_start.Year),
                            payCycleSssDeduction = await employeeInformation1.payCycleSSSDeductions(item.employee_id,item.pay_period_start),
                            ytdsssDeductions = await employeeInformation2.yearToDateSSSDeductions(item.employee_id,item.pay_period_start.Year),
                            payCyclePhilHealthDeduction = await employeeInformation1.payCyclephilHealthDeductions(item.employee_id,item.pay_period_start),
                            ytdphilHealthDeductions = await employeeInformation2.yearToDatephilHealthDeductions(item.employee_id,item.pay_period_start.Year),
                            payCycleLoanDeduction = await employeeInformation1.payCycleLoanDeductions(item.employee_id,item.pay_period_start),
                            ytdLoanDeductions = await employeeInformation2.yearToDateLoanDeductions(item.employee_id,item.pay_period_start.Year),
                            payCycleAbsenceDeduction = await employeeInformation1.payCycleAbsenceDeduction(item.employee_id,item.pay_period_start),
                            ytdAbsenceDeductions = await employeeInformation2.yearToDateAbsenceDeduction(item.employee_id,item.pay_period_start.Year),
                            payCyclePagibigDeductions = await employeeInformation1.payCyclePagibigDeductions(item.employee_id,item.pay_period_start),
                            ytdPagibigDeductions = await employeeInformation2.yearToDatePagibigDeductions(item.employee_id,item.pay_period_start.Year),
                            dateGenerated = DateTime.Now.ToShortDateString(),
                        };
                        await historicalReportDbContext.payrollInformation.AddAsync(payrollData);
                    }
                }
                await historicalReportDbContext.SaveChangesAsync(token);
                    
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
            }
        }
    }
}

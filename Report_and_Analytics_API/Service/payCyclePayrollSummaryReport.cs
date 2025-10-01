
using APIResponses.Historical_report;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class payCyclePayrollSummaryReport : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<payCyclePayrollSummaryReport> _logger;

        public payCyclePayrollSummaryReport(IServiceScopeFactory serviceScopeFactory,ILogger<payCyclePayrollSummaryReport> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var reportDbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var employeeInformation1 = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();
                var employeeInformation2 = scope.ServiceProvider.GetRequiredService<IhrEmployeeInformation>();

                if(DateTime.Now.Day <= 13)
                {
                    _logger.LogInformation("Payroll extraction starting......");
                    await extractPaycleRecords(reportDbContext,employeeInformation1,employeeInformation2);
                    _logger.LogInformation("Payroll extraction finished......");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private async Task extractPaycleRecords(ReportDbContext reportDb,IhrEmployeeInformation employeeInformation1,IhrEmployeeInformation employeeInformation2)
        {
            try
            {
                var payroll = await reportDb.hr_payroll.Include(i => i.hr_Employees).ToListAsync();

                foreach (var item in payroll)
                {
                    bool exist = await reportDb.payrollinformation
                        .AnyAsync(i => i.payPeriodStartDate == item.pay_period_start);

                    if (!exist)
                    {
                        var employeeInfo = await employeeInformation1.getEmployeeInformation(item.employee_id);
                        var payrollInformation = new employeeReportFinalData()
                        {
                            employeeId = item.employee_id,
                            employeeName = $"{employeeInfo.first_name} {employeeInfo.middle_name} {employeeInfo.last_name}",
                            payPeriodStartDate = item.pay_period_start,
                            overtimeHours = await employeeInformation1.payCycleOvertimeHours(item.employee_id,item.pay_period_start),
                            overtimePay = await employeeInformation1.payCycleOvertimeHoursPaidAmount(item.employee_id,item.pay_period_start),
                            payCycleGrossPay = await employeeInformation1.payCycleGrossPay(item.employee_id,item.pay_period_start),
                            GrossPay = await employeeInformation2.yearToDateGrossPay(item.employee_id,item.pay_period_start.Year),
                            payCycleTotalDeductions = await employeeInformation1.payCycleTotalDeductions(item.employee_id, item.pay_period_start),
                            ytdTotalDeductions = await employeeInformation2.yearToDateTotalDeductions(item.employee_id, item.pay_period_start.Year),
                            payCycleNetpay = await employeeInformation1.payCycleNetPay(item.employee_id, item.pay_period_start),
                            ytdNetPay = await employeeInformation2.yearToDateNetPay(item.employee_id, item.pay_period_start.Year),
                            payCycleSssDeduction = await employeeInformation1.payCycleSSSDeductions(item.employee_id, item.pay_period_start),
                            ytdsssDeductions = await employeeInformation2.yearToDateSSSDeductions(item.employee_id, item.pay_period_start.Year),
                            payCyclePhilHealthDeduction = await employeeInformation1.payCyclephilHealthDeductions(item.employee_id, item.pay_period_start),
                            ytdphilHealthDeductions = await employeeInformation2.yearToDatephilHealthDeductions(item.employee_id, item.pay_period_start.Year),
                            payCyclePagibigDeductions = await employeeInformation1.payCyclePagibigDeductions(item.employee_id, item.pay_period_start),
                            ytdPagibigDeductions = await employeeInformation2.yearToDatePagibigDeductions(item.employee_id, item.pay_period_start.Year),
                            payCycleAbsenceDeduction = await employeeInformation1.payCycleAbsenceDeduction(item.employee_id, item.pay_period_start),
                            ytdAbsenceDeductions = await employeeInformation2.yearToDateAbsenceDeduction(item.employee_id, item.pay_period_start.Year),
                            dateGenerated = DateTime.Now.ToShortDateString()
                        };
                        await reportDb.payrollinformation.AddAsync(payrollInformation);
                    }
                    await reportDb.SaveChangesAsync();
                }
            }
            catch (Exception ex) 
            {
                _logger.LogInformation("Error doing extraction");
                throw new Exception(ex.Message);
            }

        }
    }


}

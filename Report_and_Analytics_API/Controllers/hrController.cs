using APIResponses;
using APIResponses.Historical_report;
using APIResponses.Historical_report.Payroll_Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("Hr/")]
    public class hrController : ControllerBase
    {
        private readonly IhrPayrollRepository _payrollRepository;
        private readonly IhrEmployeeInformation _employeeInformation;
        private readonly ReportDbContext _reportDbContext;

        public hrController(IhrPayrollRepository payrollRepository, IhrEmployeeInformation employeeInformation,
            ReportDbContext reportDbContext)
        {

            _payrollRepository = payrollRepository;
            _employeeInformation = employeeInformation;
            _reportDbContext = reportDbContext;
        }

        //CURRENT DEDUCTIONS
        [HttpGet("getPayrollInformation/{employeeId}/{payPeriodStartDate}")]
        public async Task<IActionResult> GetCurrentDeductions(int employeeId, DateOnly payPeriodStartDate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var employeeInformation = await _employeeInformation.getEmployeeInformation(employeeId);

                if (employeeInformation == null)
                {
                    return Ok(new { });
                }

                var payroll = await _reportDbContext.payrollinformation.Where(i => i.employeeId == employeeId &&
                   i.payPeriodStartDate == payPeriodStartDate).FirstOrDefaultAsync();

                var payrollInformation = new payrollStatementResponses()
                {
                    employeeName = $"{employeeInformation.first_name} {employeeInformation.middle_name} {employeeInformation.last_name}",
                    payPeriodStartDate = payPeriodStartDate,
                    overtimeHours = payroll.overtimeHours,
                    overtimePay = payroll.overtimePay,
                    payCycleGrossPay = payroll.payCycleGrossPay,
                    GrossPay = payroll.GrossPay,
                    payCycleTotalDeductions = payroll.payCycleTotalDeductions,
                    ytdTotalDeductions = payroll.ytdTotalDeductions,
                    ytdNetPay = payroll.ytdNetPay,
                    payCycleNetpay = payroll.payCycleNetpay,
                    payCycleSssDeduction = payroll.payCycleSssDeduction,
                    ytdsssDeductions = payroll.ytdsssDeductions,
                    payCyclePhilHealthDeduction = payroll.payCyclePhilHealthDeduction,
                    ytdphilHealthDeductions = payroll.ytdphilHealthDeductions,
                    payCycleLoanDeduction = payroll.payCycleLoanDeduction,
                    ytdLoanDeductions = payroll.ytdLoanDeductions,
                    payCycleAbsenceDeduction = payroll.payCycleAbsenceDeduction,
                    ytdAbsenceDeductions = payroll.ytdAbsenceDeductions,
                    payCyclePagIbigDeductions = payroll.payCyclePagibigDeductions,
                    ytdPagIbigDeductions = payroll.ytdPagibigDeductions,
                    dateGenerated = DateTime.Now.ToShortDateString(),
                };

                return Ok(payrollInformation);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("getPayperiodStartDates/{employeeId}")]
        public async Task<IActionResult> GetPayrollDates(int employeeId)
        {
            try
            {
                var dates = await _payrollRepository.payrollStatementDates(employeeId);

                if (dates == null || dates.Count == 0)
                {
                    return Ok(new { });
                }
                else
                {
                    return Ok(dates);
                }
            }
            catch (Exception ex)
            {
                throw new NullReferenceException(ex.Message);
            }
        }

        //THIS ENDPOINTS IS USED TO POPULATE PAYROLL ANNUAL SUMMARY REPORT
        //YEAR endpoint for total hours worked,over time hours and total wage
        [HttpGet("getYearPayrollInformation/{employeeId}/{year}")]
        public async Task<IActionResult> getYearPayrollInformation(int employeeId, int year)
        {
            try
            {
                var employeeName = await _employeeInformation.getEmployeeInformation(employeeId);

                if (employeeName == null)
                {
                    return NotFound();
                }

                var employeeAnnualBreakdownReport = new employeeAnnualSalaryReportResponse()
                {
                    employeeName = $"{employeeName.first_name} {employeeName.middle_name} {employeeName.last_name}",
                    yearTotalHoursWorked = await _employeeInformation.yearTotalHoursWorked(employeeId, year),
                    yearTotalOvertimeHoursWorked = await _employeeInformation.yearTotalOvertimeHoursWorked(employeeId, year),
                    yearTotalWage = await _employeeInformation.yearTotalWage(employeeId, year),
                };

                return Ok(employeeAnnualBreakdownReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
       
        //YEAR TO DATE DEDUCTIONS
        [HttpGet("getYearToDatePayrollInformation/{employeeId}")]
        public async Task<IActionResult> GetYTDDeductions(int employeeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payrolls = await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                    .Where(i => i.employee_id == employeeId).ToListAsync();

                var deductions = new ytdDeductions();

                foreach (var item in payrolls)
                {
                    deductions.ytdGrossPay += item.gross_pay;
                    deductions.netPay += item.net_pay;
                    deductions.deductions += item.total_deductions;
                    deductions.absenceDeduction += item.absence_deduction;
                    deductions.sssDeduction += item.sss_deduction;
                    deductions.philHealthDeduction += item.philhealth_deduction;
                }

                return Ok(deductions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getEmployee")]
        public async Task<IActionResult> getEmployees()
        {
            try
            {
                var emp = await _reportDbContext.hr_employees.ToListAsync();

                var response = emp.Select(i => new employeeDetailsResponse
                {
                    employee_id = i.employee_id,
                    fullname = $"{i.first_name} {i.middle_name} {i.last_name}"
                }).ToList();

                return Ok(response);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //ENDPOINTS TO POPULATE EMPLOYEE MONTHLY PAYROLL INFORMATION
        [HttpGet("getHospitalMonthlyPayrollReport/{month}/{year}")]
        public async Task<IActionResult> getHospitalMonthlyPayrollReport(int month ,int year)
        {
            try
            {
                var monthPayrollReport = await (
                    from hr_employees in _reportDbContext.hr_employees
                    join employeeMonthReport in _reportDbContext.employeePayrollMonthReports
                    on hr_employees.employee_id equals employeeMonthReport.employeeId
                    join hr_payroll in _reportDbContext.hr_payroll
                    on employeeMonthReport.employeeId equals hr_payroll.employee_id
                    where employeeMonthReport.month == month && employeeMonthReport.year == year
                    group new { hr_employees, hr_payroll, employeeMonthReport } by hr_employees.employee_id into x
                    select new
                    {
                        employee_id = x.Key,
                        firstName = x.Select(i => i.hr_employees.first_name).FirstOrDefault(),
                        middleName = x.Select(i => i.hr_employees.middle_name).FirstOrDefault(),
                        lastName = x.Select(i => i.hr_employees.last_name).FirstOrDefault(),
                        department = x.Select(i => i.hr_employees.department).FirstOrDefault(),
                        role = x.Select(i => i.hr_employees.role).FirstOrDefault(),
                        basicSalary = x.Select(i => i.hr_payroll.basic_pay).FirstOrDefault(),
                        overtimePay = x.Where(i => i.hr_payroll.pay_period_start.Day == 16).Select(i => i.hr_payroll.overtime_pay).FirstOrDefault(),
                        deductions = x.Where(i => i.hr_payroll.employee_id == x.Key && i.hr_payroll.pay_period_start.Day == 16)
                        .Sum(i => i.hr_payroll.sss_deduction),
                        netPay = x.Where(i => i.employeeMonthReport.employeeId == x.Key && i.employeeMonthReport.month == month && 
                        i.employeeMonthReport.year == year)
                        .Select(i => i.employeeMonthReport.monthTotalWage).FirstOrDefault()
                    }).ToListAsync();
               
                if(monthPayrollReport == null || monthPayrollReport.Count == 0)
                {
                    return Ok(new {});
                }

                var response = monthPayrollReport.Select(i => new hospitalPayrollReportMonthResponse
                {
                    employeeId = i.employee_id,
                    fullName = $"{i.firstName} {i.middleName} {i.lastName}",
                    basicSalary = i.basicSalary,
                    deductions = i.deductions,
                    department = i.department,
                    role = i.role,
                    netPay = i.netPay,
                    overtimePay = i.overtimePay,
                    totalSalaryPaid = monthPayrollReport.Sum(i => i.netPay)
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex) 
            {
                return StatusCode (500,ex.Message);
            }
        }
    }
}


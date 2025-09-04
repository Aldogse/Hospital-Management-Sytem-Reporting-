using APIResponses;
using APIResponses.Historical_report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

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

                if (dates == null)
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

        //ENDPOINT TO POPULATE YEARLY SECTION IN ANNual summary report
        [HttpGet("getMonthPayrollInformation/{employeeId}/{month}/{year}")]
        public async Task<IActionResult> getMonthPayrollInformation(int employeeId, int month, int year)
        {
            try
            {
                var employeeMonthReport = await _reportDbContext.employeePayrollMonthReports
                    .Where(i => i.employeeId == employeeId && i.month == month && i.year == year).ToListAsync();


                var monthsInformation = new employeeMonthsBreakdownResponse();

                foreach (var item in employeeMonthReport) 
                {
                    monthsInformation.monthOvertimeHours = item.monthOvertimeHours;
                    monthsInformation.monthTotalHoursWorked = item.monthTotalHoursWorked;
                    monthsInformation.monthTotalHoursWage = item.monthTotalWage;
                }

                if (monthsInformation == null)
                {
                    return Ok(new { });
                }

                return Ok(monthsInformation);
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

                return Ok(emp);
            }
            catch
            (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}

using APIResponses;
using APIResponses.Historical_report;
using APIResponses.Historical_report.Payroll_Responses;
using APIResponses.PayrollResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("payroll/")]
    public class payrollController : ControllerBase
    {
        private readonly IhrPayrollRepository _payrollRepository;
        private readonly IhrEmployeeInformation _employeeInformation;
        private readonly ReportDbContext _reportDbContext;

        public payrollController(IhrPayrollRepository payrollRepository, IhrEmployeeInformation employeeInformation,
            ReportDbContext reportDbContext)
        {

            _payrollRepository = payrollRepository;
            _employeeInformation = employeeInformation;
            _reportDbContext = reportDbContext;
        }


        //ENDPOINT FOR PAYROLL DATA SUMMARY
        [HttpGet("getMonthPayrollSummary/{month}/{year}/{pageSize}/{currentPage}")]
        public async Task<IActionResult> getMonthPayrollSummary(int month, int year,int pageSize,int currentPage)
        {
            try
            {
                var payrollSummaryList = await _payrollRepository.individualPayrollSummaryReports(month, year, pageSize,currentPage);

                if(payrollSummaryList == null ||  payrollSummaryList.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No employee data extracted",
                        data = (object?)null
                    });
                }
                var payrollReport = await _payrollRepository.monthPayrollSummaryResponse(month,year);

                payrollReport.summaryList = payrollSummaryList;
                if(payrollReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted",
                        data = (object?)null
                    });
                }
                return Ok(payrollReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
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

        //ENDPOINT FOR YEAR PAYROLL SUMMARY
        [HttpGet("getYearPayrollSummary/{year}")]
        public async Task<IActionResult> getYearPayrollSummary(int year)
        {
            try
            {
                var payrollReport = await _payrollRepository.yearSummaryPayrollResponses(year);

                if(payrollReport == null || payrollReport.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Year report is Empty",
                        data = (object?)null
                    });
                }

                return Ok(payrollReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}


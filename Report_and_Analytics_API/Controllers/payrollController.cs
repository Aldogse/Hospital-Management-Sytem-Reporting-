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
        [HttpGet("getHospitalMonthlyPayrollReport")]
        public async Task<IActionResult> getHospitalMonthlyPayrollReport([FromQuery]int month,[FromQuery]int year)
        {
            try
            {
                var monthPayrollReport = await _payrollRepository.hospitalMonthPayrollReport(month,year);
               
                if(monthPayrollReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No report for the month yet"
                    });
                }
            
                return Ok(monthPayrollReport);
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


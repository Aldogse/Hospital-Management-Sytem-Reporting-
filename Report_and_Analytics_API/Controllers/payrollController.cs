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
        private readonly ILogger<payrollController> _logger;

        public payrollController(IhrPayrollRepository payrollRepository, IhrEmployeeInformation employeeInformation,
            ReportDbContext reportDbContext,ILogger<payrollController>logger)
        {

            _payrollRepository = payrollRepository;
            _employeeInformation = employeeInformation;
            _reportDbContext = reportDbContext;
            _logger = logger;
        }


        //ENDPOINT FOR PAYROLL DATA SUMMARY
        [HttpGet("getMonthPayrollSummary")]
        public async Task<IActionResult> getMonthPayrollSummary([FromQuery]int month,[FromQuery]int year)
        {
            try
            {
                var monthReport = await _payrollRepository.monthPayrollSummaryResponse(month,year);

                if(monthReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No report for the month",
                        data = (object?)null
                    });
                }
                
                return Ok(monthReport);
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

        [HttpGet("monthPayrollComparisonEndpoint")]
        public async Task<IActionResult> monthPayrollComparisonEndpoint([FromQuery]int month, [FromQuery]int year,
            [FromQuery]int partnerMonth, [FromQuery]int partnerYear)
        {
            try
            {
                if(month == partnerMonth &&  year == partnerYear)
                {
                    return StatusCode(400,"Cannot compare same month and year");
                }

                var comparisonResponse = await _payrollRepository.monthPayrollComparisonResult(month,year,partnerMonth,partnerYear);

                if(comparisonResponse == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No reports for the month.",
                        Data = (object?)null
                    });
                }
                return Ok(comparisonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("yearHospitalPayrollReport")]
        public async Task<IActionResult> yearHospitalPayrollReport([FromQuery]int year)
        {
            try
            {

                var exist = await _reportDbContext.year_hospital_payroll_report
                    .AnyAsync(i => i.year == year);

               

                if (!exist)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No report yet for the year"
                    });
                }
                var yearPayroll = await _payrollRepository.yearHospitalPayrollSummary(year);
              
                return Ok(yearPayroll);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //NEW ENDPOINTS FOR PAYROLL
        [HttpGet("monthPayrollRangeQueryAsync")]
        public async Task<IActionResult> monthPayrollRangeQueryAsync([FromQuery] int startmonth, [FromQuery]int startyear,
            [FromQuery]int endmonth, [FromQuery]int endyear)
        {
            try
            {
                if (startmonth > endmonth && startyear >= endyear)
                {
                    return StatusCode(400,"Start cannot be greater than the End");
                }

                var response = await _payrollRepository.monthPayrollRangeQueryAsync(startmonth,startyear,endmonth,endyear);

                if(response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No report yet for {startmonth}/{startyear} and {endmonth}/{endyear}"
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}


using APIResponses.PayrollResponse;
using APIResponses.PropertyAndManagementResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("property/")]
    public class propertyController : ControllerBase
    {
        private readonly IpropertyRepository _propertyRepo;
        private readonly ILogger<propertyController> _logger;

        public propertyController(IpropertyRepository propertyRepo, ILogger<propertyController>logger)
        {
            _propertyRepo = propertyRepo;
            _logger = logger;
        }

        //ENDPOINTS FOR Patient and admission summary
        [HttpGet("getMonthAdmission/{month}/{year}")]
        public async Task<IActionResult> getMonthAdmission(int month, int year)
        {
            try
            {
                var admissionReport = await _propertyRepo.getAdmissionReport(month,year);

                if(admissionReport == null || admissionReport.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted for {month}/{year}",
                        data = (object?)null
                    });
                }
                else
                {
                    return Ok(admissionReport);
                }
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500,$"Error: {ex.Message}");
            }
        }

        [HttpGet("getMonthSummaryAdmissionAndDischargeReport")]
        public async Task<IActionResult> getMonthSummaryAdmissionAndDischargeReport([FromQuery]int month,[FromQuery]int year)
        {
            try
            {
                var monthSummaryReport = await _propertyRepo.getMonthAdmissionAndDischargeReport(month,year);

                if(monthSummaryReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted for {month}/{year}",
                        data = (object?)null
                    });
                }
                else
                {
                    return Ok(monthSummaryReport);
                }
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("getMonthDischargeReport/{month}/{year}")]
        public async Task<IActionResult> getMonthDischargeReport(int month,int year)
        {
            try
            {
                var monthDischargeReport = await _propertyRepo.getDischargeReport(month,year);

                if (monthDischargeReport == null || monthDischargeReport.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data extracted for {month}/{year}",
                        data = (object?)null
                    });
                }
                else
                {
                    return Ok(monthDischargeReport);
                }
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet  ("getYearBedsDistributionReport/{year}")]
        public async Task<IActionResult> getYearBedsDistributionReport(int year)
        {
            try
            {
                var yearData = await _propertyRepo.yearlyAdmissionAndDischargeReport(year);
                var monthData = await _propertyRepo.monthBedsDistribution(year);
             
                if(yearData == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data for {year}"
                    });
                }

                var response = new yearBedsDistributionSummaryResponse()
                {
                    available_beds = yearData.available_beds,
                    occupied_beds = yearData.occupied_beds,
                    broken_beds = yearData.broken_beds,
                    total_beds = yearData.total_beds,
                    year = year,
                    monthsAdmissionReport = monthData,
                };
                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}

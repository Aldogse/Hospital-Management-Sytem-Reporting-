using APIResponses.PayrollResponse;
using APIResponses.PropertyAndManagementResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("property/")]
    public class propertyController : ControllerBase
    {
        private readonly IpropertyRepository _propertyRepo;
        private readonly ILogger<propertyController> _logger;
        private readonly ReportDbContext _reportDbContext;

        public propertyController(IpropertyRepository propertyRepo, ILogger<propertyController>logger,ReportDbContext reportDbContext)
        {
            _propertyRepo = propertyRepo;
            _logger = logger;
            _reportDbContext = reportDbContext;
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
                var totalBeds = await _propertyRepo.numberOfBeds();
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
                    total_beds = totalBeds,
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

        //FORECAST ENDPOINT
        [HttpGet("getMonthForecastResult")]
        public async Task<IActionResult> getMonthForecastResult([FromQuery]int month, [FromQuery]int year)
        {
            try
            {
                var report = await _propertyRepo.monthForecastedBedOccupancyRate(month,year);

                if(report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No forecast yet for the month",
                        data = (object)null
                    });
                }

                return Ok(report);
            }
            catch (SqlException ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getMonthsOccupiedBeds")]
        public async Task<IActionResult> getMonthsOccupiedBeds()
        {
            try
            {
                var prevMonth = DateTime.UtcNow.AddMonths(-1);
                var report = await _propertyRepo.getYearAdmissionDataReport(prevMonth.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No date for the year",
                        data = (object?)null
                    });
                }

                return Ok(report);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //NEW ENDPOINTS
        [HttpGet("monthBedsAndDischargedRangeQuery")]
        public async Task<IActionResult> monthBedsAndDischargedRangeQuery([FromQuery] int start, [FromQuery] int startYear,
            [FromQuery] int endMonth, [FromQuery] int endYear)
        {
            try
            {
                if (start > endMonth && startYear >= endYear)
                {
                    return StatusCode(400, "Start cannot be greater than the End");
                }

                var response = await _propertyRepo.monthBedsAndDishcargeRangeQuery(start,startYear,endMonth,endYear);
                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("inventoryReport")]
        public async Task<IActionResult> inventoryReport()
        {
            try
            {

                var response = (await _reportDbContext.inventory                  
                    .ToListAsync()).DistinctBy(i => i.item_name).ToList();
                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

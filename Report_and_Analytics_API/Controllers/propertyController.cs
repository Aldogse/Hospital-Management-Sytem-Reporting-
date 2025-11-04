using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("getMonthSummaryAdmissionAndDischargeReport/{month}/{year}")]
        public async Task<IActionResult> getMonthSummaryAdmissionAndDischargeReport(int month,int year)
        {
            try
            {
                var monthSummaryReport = await _propertyRepo.getMonthAdmissionAndDischargeReport(year,month);

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

    }
}

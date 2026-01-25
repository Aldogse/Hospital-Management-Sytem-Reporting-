using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("insurance/")]
    public class insuranceController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly IinsuranceClaimRepository _claimRepository;

        public insuranceController(ReportDbContext reportDbContext,IinsuranceClaimRepository claimRepository)
        {
            _reportDbContext = reportDbContext;
            _claimRepository = claimRepository;
        }


        //ENDPOINT FOR MONTH INSURANCE REPORT WITH TOTAL PAID AMOUNT
        [HttpGet("monthInsuranceClaimsReport/{month}/{year}")]
        public async Task<IActionResult> getMonthInsuranceClaimsReport(int month, int year)
        {

            try
            {
                var monthInsuranceClaimReport = await (
                    from logs in _reportDbContext.insurance_logs
                    join patient in _reportDbContext.patientinfo
                    on logs.patient_id equals patient.patient_id
                    join claims in _reportDbContext.insurance_claims
                    on patient.patient_id equals claims.patient_id
                    join provider in _reportDbContext.insurance_provider
                    on claims.insurance_provider_id equals provider.insurance_provider_id
                    join request in _reportDbContext.insurance_request
                    on logs.patient_id equals request.patient_id
                    where logs.date_transact.Month == month && logs.date_transact.Year == year
                    group new { logs, patient, claims, provider, request } by logs.log_id into x
                    select new
                    {
                        logId = x.Key,
                        patientName = $"{x.Select(i => i.patient.fname).FirstOrDefault()} " +
                        $"{x.Select(i => i.patient.mname).FirstOrDefault()} {x.Select(i => i.patient.lname).FirstOrDefault()}",
                        insuranceProvider = x.Select(i => i.provider.name).FirstOrDefault(),
                        insuranceNumber = x.Select(i => i.request.insurance_number).FirstOrDefault(),
                        remarks = x.Select(i => i.request.notes).FirstOrDefault(),
                        dateOfService = x.Select(i => i.logs.date_transact).FirstOrDefault(),
                        status = x.Select(i => i.logs.status).FirstOrDefault(),
                        insuranceCovered = x.Where(i => i.logs.status == "Approved").Select(i => i.request.insurance_covered).FirstOrDefault(),
                        claimAmount = x.Select(i => i.claims.claim_amount).FirstOrDefault(),
                        percentageCovered = x.Where(i => i.logs.status == "Approved")
                        .Select(i => (i.request.insurance_covered / i.claims.claim_amount) * 100),
                    }).ToListAsync();


                if (monthInsuranceClaimReport == null || monthInsuranceClaimReport.Count == 0)
                {
                    return Ok(new { });
                }

                return Ok(monthInsuranceClaimReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("claimStatusMonthReport/{month}/{year}")]
        public async Task<IActionResult> claimStatusMonthReport(int month, int year)
        {
            try
            {
                var statusReport = await _claimRepository.getMonthClaimReports(month,year);

                if (statusReport == null)
                {
                    return Ok(new { });
                }

                return Ok(statusReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthProviderPerformance")]
        public async Task<IActionResult> getMonthProviderPerformance()
        {
            try
            {
                var prevMonth = DateTime.UtcNow.AddMonths(-1);
                var statusReport = await _claimRepository.getProvidersMonthPerformance(prevMonth.Month,prevMonth.Year);

                if (statusReport == null)
                {
                    return Ok(new 
                    { 
                        success = true,
                        message = $"No data has been fetched",
                        data = (object?)null
                    });
                }

                return Ok(statusReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthProviderStatusForecast")]
        public async Task<IActionResult> getMonthProviderStatusForecast()
        {
            try
            {
                var date = DateTime.UtcNow;
                var statusReport = await _claimRepository.getMonthProviderClaimStatusForecast(date.Year);

                if (statusReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data has been fetched",
                        data = (object?)null
                    });
                }

                return Ok(statusReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthProviderAmountForecast")]
        public async Task<IActionResult> getMonthProviderAmountForecast()
        {
            try
            {
                var date = DateTime.UtcNow;
                var statusReport = await _claimRepository.getMonthProviderClaimsAmountForecast(date.Year);

                if (statusReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data has been fetched",
                        data = (object?)null
                    });
                }

                return Ok(statusReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

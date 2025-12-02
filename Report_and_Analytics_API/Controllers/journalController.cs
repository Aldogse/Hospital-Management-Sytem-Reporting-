using APIResponses.journal_responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("journal/")]
    public class journalController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly IjournalRepository _journalRepo;
        private readonly ILogger<journalController> _logger;

        public journalController(ReportDbContext reportDbContext,IjournalRepository journalRepo,ILogger<journalController>logger)
        {
            _reportDbContext = reportDbContext;
            _journalRepo = journalRepo;
            _logger = logger;
        }

        //THIS ENDPOINTS IS FOR HOSPITAL REVENUE REPORT
        [HttpGet("getYearRevenue/{year}")]
        public async Task<IActionResult> getYearRevenue(int year)
        {
            try
            {
                var yearRevenue = await _journalRepo.getYearRevenue(year);

                if (yearRevenue == null) 
                {
                    return Ok(new {});
                }

                return Ok(yearRevenue);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getQuarterRevenues/{year}/{quarter}")]
        public async Task<IActionResult> getQuarterRevenues(int year,int quarter)
        {
            try
            {
                var quarterRevenues = await _reportDbContext.quarter_revenue
                    .Where(i => i.year == year && i.quarter == quarter)
                    .SumAsync(i => i.totalRevenue);

                if(quarterRevenues == null)
                {
                    return Ok(new {});
                }

                return Ok(quarterRevenues);

            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("availableYears")]
        public async Task<IActionResult> years()
        {
            try
            {
                var years = await _reportDbContext.quarter_revenue
                    .Select(t => t.year).ToListAsync();

                List<int> year = new List<int>();

                foreach (var item in years)
                {
                    bool exist = year.Contains(item);

                    if (!exist)
                    {
                        year.Add(item);
                    }
                }
                return Ok(year);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getQuarterOneRevenueDetails/{year}")]
        public async Task<IActionResult> getQuarterOneRevenueDetails(int year)
        {
            try
            {
                var details = await _journalRepo.getQuarterOneBreakdown(year);

                if(details == null || details.Count == 0)
                {
                    return Ok(new {});
                }

                var response = details.Select(i => new monthRevenueBreakdownResponse
                {
                    report_id = i.report_id,
                    description = i.description,
                    month = i.month,
                    year = i.year,
                    amount = i.amount,
                }).ToList();

                return Ok(response);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getQuarterTwoRevenueDetails/{year}")]
        public async Task<IActionResult> getQuarterTwoRevenueDetails(int year)
        {
            try
            {
                var details = await _journalRepo.getQuarterTwoBreakdown(year);

                if (details == null || details.Count == 0)
                {
                    return Ok(new { });
                }

                var response = details.Select(i => new monthRevenueBreakdownResponse
                {
                    report_id = i.report_id,
                    description = i.description,
                    month = i.month,
                    year = i.year,
                    amount = i.amount,
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("getQuarterThreeRevenueDetails/{year}")]
        public async Task<IActionResult> getQuarterThreeRevenueDetails(int year)
        {
            try
            {
                var details = await _journalRepo.getQuarterThreeBreakdown(year);

                if (details == null || details.Count == 0)
                {
                    return Ok(new { });
                }

                var response = details.Select(i => new monthRevenueBreakdownResponse
                {
                    report_id = i.report_id,
                    description = i.description,
                    month = i.month,
                    year = i.year,
                    amount = i.amount,
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getQuarterFourRevenueDetails/{year}")]
        public async Task<IActionResult> getQuarterFourRevenueDetails(int year)
        {
            try
            {
                var details = await _journalRepo.getQuarterOneBreakdown(year);

                if (details == null || details.Count == 0)
                {
                    return Ok(new { });
                }

                var response = details.Select(i => new monthRevenueBreakdownResponse
                {
                    report_id = i.report_id,
                    description = i.description,
                    month = i.month,
                    year = i.year,
                    amount = i.amount,
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        //PHARMACY SALES ENDPOINT
        [HttpGet("getRangeSalesReport/{startDate}/{endDate}")]
        public async Task<IActionResult> getRangeSalesReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var salesReport = await _journalRepo.getRangePharmacySalesReport(startDate,endDate);

                if(salesReport == null)
                {
                    return Ok(new 
                    {
                        success = true,
                        message = "No date fetched from database",
                        data = (object?)null
                    });
                }

                return Ok(salesReport);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        //BILLING SUMMARY END POINTS
        [HttpGet("getMonthBillingReport/{month}/{year}")]
        public async Task<IActionResult> getMonthBillingReport(int month,int year)
        {
            try
            {
                var monthBillingReport = await _journalRepo.monthBillingReport(month,year);

                if(monthBillingReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No billing report for {month}/{year}",
                    });
                }
                return Ok(monthBillingReport);
            }
            catch (SqlException ex)
            {
                return StatusCode(500,$"Sql error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getDailyBillingReport/{date}")]
        public async Task<IActionResult> getDailyBillingReport(DateOnly date)
        {
            try
            {               
                var dailyReport = await _journalRepo.dailyBillingReport(date);

                if(dailyReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data available for {date}"
                    });
                }
                else
                {
                    var response = new dailyBillingReportResponse()
                    {
                        report_date = dailyReport.report_date.ToShortDateString(),
                        total_billed = dailyReport.total_billed,
                        total_insurance_covered = dailyReport.total_insurance_covered,
                        total_oop_collected = dailyReport.total_oop_collected,
                        total_paid = dailyReport.total_paid,
                        total_pending_amount = dailyReport.total_pending_amount,
                        total_pending_transactions = dailyReport.total_pending_transactions,
                    };
                    return Ok(response);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500,$"Sql error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getMonthTransactions/{month}/{year}/{page}/{size}")]
        public async Task<IActionResult> getMonthTransactions(int month,int year,int page,int size)
        {
            try
            {
                var monthTransactions = await _journalRepo.monthBillingTransactionSummary(month,year,page,size);
             

                if(monthTransactions == null || monthTransactions.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data available for {month}/{year}"
                    });
                }

                var response = monthTransactions.Select(i => new dailyBillingReportResponse
                {
                    report_date = i.report_date.ToShortDateString(),
                    total_billed = i.total_billed,
                    total_insurance_covered = i.total_insurance_covered,
                    total_oop_collected = i.total_oop_collected,
                    total_paid = i.total_paid,
                    total_pending_amount = i.total_pending_amount,
                    total_pending_transactions = i.total_pending_transactions,
                }).ToList();

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

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

        [HttpGet("getYearPharmacySalesReport/{year}")]
        public async Task<IActionResult> getYearPharmacySalesReport(int year)
        {
            try
            {
                var yearReport = await _journalRepo.yearPharmacySales(year);
                var monthSales = await _journalRepo.monthsPharmacySales(year);

                if(yearReport == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data fetched from database for {year}",
                        data = (object?)null
                    });
                }

                var response = new yearPharmacySalesResponse()
                {
                    topSellingItem = yearReport.topSellingItem,
                    totalSales = yearReport.totalSales,
                    totalTransactions = yearReport.totalTransactions,
                    year = year,
                    monthSales = monthSales,
                };
                
                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500,ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthPharmacySales/{month}/{year}")]
        public async Task<IActionResult> getMonthPharmacySales(int month,int year)
        {
            try
            {
                var monthSales = await _journalRepo.monthPharmacySales(month,year);

                if(monthSales == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data available for {month}/{year}",
                        data = (object?)null
                    });
                }

                return Ok(monthSales);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
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

        [HttpGet("getDailyBillingReport")]
        public async Task<IActionResult> getDailyBillingReport([FromQuery]DateOnly date)
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

        [HttpGet("getMonthTransactions/{month}/{year}")]
        public async Task<IActionResult> getMonthTransactions(int month,int year,[FromQuery]int page,[FromQuery]int size)
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

        [HttpGet("getYearBillSummaryReport/{year}")]
        public  async Task<IActionResult> getYearBillSummaryReport(int year)
        {
            try
            {
                var report = await _journalRepo.yearBillingReport(year);

                var monthReports = await _journalRepo.monthsBillingReport(year);

                if(report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data available for {year}"
                    });
                }

                var response = new yearlyBillingReportResponse()
                {
                    year = year,
                    total_pending_amount = report.total_pending_amount,
                    total_billed = report.total_billed,
                    total_insurance_covered = report.total_insurance_covered,
                    total_oop_collected = report.total_oop_collected,
                    total_paid = report.total_paid,
                    total_pending_transactions = report.total_pending_transaction,
                };

                var monthRep = monthReports.Select(i => new monthBillingReportResponse
                {
                    month = i.month,
                    year = i.year,
                    totalBilled = i.total_billed,
                    totalPaid = i.total_paid,
                }).ToList();

                response.monthReports = monthRep;

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

        //REVENUE REPORT ENDPOINTS
        [HttpGet("getMonthRevenueReport")]
        public async Task<IActionResult> getMonthRevenueReport([FromQuery]int month, [FromQuery]int year)
        {
            try
            {
                var monthRevenue = await _reportDbContext.month_revenue_report.Where(i => i.month == month && i.year == year)
                    .FirstOrDefaultAsync();

                if(monthRevenue == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data for {month}/{year}",
                    });
                }

                return Ok(monthRevenue);
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
 
using APIResponses.BillingResponse;
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
        [HttpGet("getMonthBillingReport")]
        public async Task<IActionResult> getMonthBillingReport([FromQuery]int month,[FromQuery]int year)
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

        [HttpGet("getYearBillSummaryReport")]
        public  async Task<IActionResult> getYearBillSummaryReport([FromQuery]int year,[FromQuery]int comparedYear)
        {
            try
            {
                var baseYearReport = await _journalRepo.baseYearBillingReport(year);
                var comparedYearReport = await _journalRepo.comparedYearBillingReport(comparedYear);

                var response = new yearsBillingReportComparisons()
                {
                    baseYear = baseYearReport.year,
                    total_oop_collected = baseYearReport.total_oop_collected,
                    total_pending_amount = baseYearReport.total_pending_amount,
                    total_billed = baseYearReport.total_billed,
                    total_paid =baseYearReport.total_paid,
                    total_pending_transaction = baseYearReport.total_pending_transaction,
                    comparedYear = comparedYearReport.year,
                    prev_total_pending_amount = comparedYearReport.total_pending_amount,
                    prev_total_billed = comparedYearReport.total_billed,
                    prev_total_paid = comparedYearReport.total_paid,
                    prev_total_insurance_covered = comparedYearReport.total_insurance_covered,
                    prev_total_oop_collected = comparedYearReport.total_oop_collected,
                    prev_total_pending_transaction = comparedYearReport.total_pending_transaction,
                    total_insurance_covered = baseYearReport.total_insurance_covered
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

        [HttpGet("getMonthsRevenueReport")]
        public async Task<IActionResult> getMonthsRevenueReport()
        {
            try
            {
                DateTime date = DateTime.Now;
                var monthsRevenue = await _journalRepo.getMonthsRevenueReport(date.Year);

                if (monthsRevenue == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data for {date.Year}",
                    });
                }

                return Ok(monthsRevenue);
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

        [HttpGet("monthRevenueComparisonEndpoint")]
        public async Task<IActionResult> monthRevenueComparisonEndpoint([FromQuery]int month,[FromQuery]int year,
            [FromQuery]int partnerMonth,[FromQuery]int partnerYear)
        {
            try
            {
                if(month == partnerMonth &&  year == partnerYear)
                {
                    return StatusCode(400,"Cannot compare same month and year");
                }

                var comparisonResponse = await _journalRepo.monthRevenueComparisonResponse(month,year,partnerMonth,partnerYear);

                if(comparisonResponse == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No report for the months yer",
                        Data = (object?)null
                    });
                }
                return Ok(comparisonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500,$"Error:{ex.Message}");
            }
        }

        //FORECAST ENDPOINTS
        [HttpGet("getMonthTotalCostForecast")]
        public async Task<IActionResult> getMonthTotalCostForecast([FromQuery]int month, [FromQuery]int year)
        {
            try
            {
                var report = await _journalRepo.getMonthCostForecast(month,year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("getMonthTotalRevenueForecast")]
        public async Task<IActionResult> getMonthTotalRevenueForecast([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                var report = await _journalRepo.getMonthRevenueForecast(month, year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthMedicineShortageResult")]
        public async Task<IActionResult> getMonthMedicineShortageResult()
        {
            try
            {
                var date = DateTime.UtcNow;
                var report = await _journalRepo.getMonthMedicineShortageForecast(date.Month, date.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getPreviousMonthMedicineDispensed")]
        public async Task<IActionResult> getPreviousMonthMedicineDispensed()
        {
            try
            {
                var date = DateTime.UtcNow.AddMonths(-1);
                var report = await _journalRepo.getMedicineMonthDispensed(date.Month, date.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getPreviousMonthsCostManagement")]
        public async Task<IActionResult> getPreviousMonthsCostManagement()
        {
            try
            {
                var date = DateTime.UtcNow.AddMonths(-1);
                var report = await _journalRepo.getPreviousMonthOperationalCostReport(date.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthCostManagementForecast")]
        public async Task<IActionResult> getMonthCostManagementForecast()
        {
            try
            {
                var date = DateTime.UtcNow;
                var report = await _journalRepo.getMonthForecastResult(date.Month,date.Year);

                if (report == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No forecast yet for the month"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthOutcomeReport")]
        public async Task<IActionResult> getMonthOutcomeReport([FromQuery]int month, [FromQuery]int year)
        {
            try
            {
                var response = await _journalRepo.monthTreatmentOutcomeReport(month,year);

                if (response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No data yet for the month",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getMonthsDetailsRevenueReport")]
        public async Task<IActionResult> getMonthsDetailsRevenueReport([FromQuery] int year)
        {
            try
            {
                var report = await _journalRepo.monthsRevenueReport(year);

                var response = report.Select(i => new yearRevenueBreakdownResponse
                {
                    year = year,
                    yearTotalRevenue = report.Select(i => i.total_revenue).Sum(),
                    monthsRevenue = report,

                    //FIX ISSUE NOT SHOWING ON THE FRONT END BUT ALL GOOD IN THE BACK END
                    pharmacy_revenue = report.Select(i => i.pharmacy_revenue).Sum(),
                    serviceRevenue = report.Select(i => i.service_revenue).Sum()
                }).FirstOrDefault();

                if (response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No data yet for the month", 
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("monthPharmacySalesComparisonEndpoint")]
        public async Task<IActionResult> monthPharmacySalesComparisonEndpoint([FromQuery]int firstMoth,[FromQuery]int firstYear
            ,[FromQuery]int secondMonth,[FromQuery]int secondYear)
        {
            try
            {
                if(firstMoth == secondMonth &&  firstYear == secondYear)
                {
                    return StatusCode(400,"Same month and year cannot be compared");
                }

                var response = await _journalRepo.monthPharmacySalesComparison(firstMoth,firstYear,secondMonth,secondYear);

                if(response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No report for the month yet",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,$"Error: {ex.Message}");
            }
        }

        [HttpGet("monthBillingReportComparisonEndpoint")]
        public async Task<IActionResult> monthBillingReportComparisonEndpoint([FromQuery] int firstMoth, [FromQuery] int firstYear
           , [FromQuery] int secondMonth, [FromQuery] int secondYear)
        {
            try
            {
                if (firstMoth == secondMonth && firstYear == secondYear)
                {
                    return StatusCode(400, "Same month and year cannot be compared");
                }

                var response = await _journalRepo.monthBillingComparisonReport(firstMoth,firstYear,secondMonth,secondYear);

                if (response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No report for the month yet",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("yearBillingReportSummary")]
        public async Task<IActionResult> yearBillingReportSummary([FromQuery]int year)
        {
            try
            {
                bool exist = await _reportDbContext.yearly_billing_report.AnyAsync(i => i.year == year);

                if (!exist)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No report for the year has been generated."
                    });
                }

                var billSummary = await _journalRepo.yearBillingReportSummary(year);

                return Ok(billSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("yearDepartmentBudgetSummaryReport")]
        public async Task<IActionResult> yearDepartmentBudgetSummaryReport([FromQuery]int year)
        {
            try
            {
                var exist = await _reportDbContext.department_budget_year_report.AnyAsync(i => i.year == year);

                if (!exist)
                {
                    return StatusCode(404,"Year report not found.");
                }

                var yearReport = await _journalRepo.departmentBudgetYearSummary(year);
                return Ok(yearReport);

            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpGet("yearPendingBudgetSummary")]
        public async Task<IActionResult> yearPendingBudgetSummary([FromQuery] int year)
        {
            try
            {
                var exist = await _reportDbContext.department_budget_year_report.AnyAsync(i => i.year == year);

                if (!exist)
                {
                    return StatusCode(404, "Year report not found.");
                }

                var yearReport = await _journalRepo.pendingMonthBudgetRequest(year);
                return Ok(yearReport);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("monthDepartmentBudgetSummaryReport")]
        public async Task<IActionResult> monthDepartmentBudgetSummaryReport([FromQuery]int month,[FromQuery] int year)
        {
            try
            {                
                var yearReport = await _journalRepo.monthDepartmentBudgetSummaryReport(month,year);

                if(yearReport == null)
                {
                    return StatusCode(404,$"No {month} report found.");
                }

                return Ok(yearReport);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("monthBudgetComparitor")]
        public async Task<IActionResult> monthBudgetComparitor([FromQuery] int month, [FromQuery] int year
            , [FromQuery] int partnerMonth, [FromQuery] int partnerYear)
        {
            try
            {
                if (month == partnerMonth && year == partnerYear)
                {
                    return StatusCode(400, "Same month and year cannot be compared");
                }
                bool firstMonthExist = await _reportDbContext.department_budgets.AnyAsync(i => i.request_date.Month == month 
                && i.request_date.Year == year);

                bool secondMonthExist = await _reportDbContext.department_budgets.AnyAsync(i => i.request_date.Month == partnerMonth
                && i.request_date.Year == partnerYear);

                if(!firstMonthExist || !secondMonthExist)
                {
                    return StatusCode(404,"One of the compared month does not exist.");
                }

                var comparisonResponse = await _journalRepo.monthDepartmentBudgetComparisonResponse(month, year, partnerMonth, partnerYear);

                if (comparisonResponse == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No records for one of the month."
                    });
                }
                return Ok(comparisonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
 
using APIResponses.BillingResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("billing/")]
    public class billingController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<billingController> _logger;

        public billingController(ReportDbContext reportDbContext,ILogger<billingController> logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }

        //ENDPOINT FOR PHARMACY SALES REPORT
        [HttpGet("getMonthPharmacySalesReport/{month}/{year}")]
        public async Task<IActionResult> getMonthPharmacySalesReport(int month,int year)
        {
            try
            {
                var monthSalesReport = await _reportDbContext.month_pharmacy_sales
                    .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

                var monthSalesDetails = await (
                    from bi in _reportDbContext.billing_items
                    join br in _reportDbContext.billing_records
                    on bi.billing_id equals br.billing_id
                    where br.billing_date.Month == month && 
                    br.billing_date.Year == year && 
                    bi.item_type == "Pharmacy"
                    group new { bi , br } by bi.item_id into x
                    select new monthPharmacySalesDetailsResponse
                    {
                        itemId = x.Key,
                        description = x.Select(x => x.bi.item_description).FirstOrDefault() ?? "",
                        quantity = x.Select(x => x.bi.quantity).FirstOrDefault(),
                        paymentMethod = x.Select(x => x.br.payment_method).FirstOrDefault() ?? "",
                        totalAmount = x.Select(x => x.br.total_amount).FirstOrDefault(),
                        billingDate = x.Select(x => x.br.billing_date).FirstOrDefault()
                    }).ToListAsync();

                var response = new monthPharmacySalesReportResponse()
                {
                    totalTransactions = monthSalesReport?.totalTransactions ?? 0,
                    totalSales = monthSalesReport?.totalSales ?? 0,
                    topSellingItem = monthSalesReport?.topSellingItem ?? "",
                    items = monthSalesDetails
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        //END POINT FOR DEPARTMENT BUDGET REPORT
        [HttpGet("getYearDepartmentBudgetDetails/{year}")]
        public async Task<IActionResult> getYearDepartmentBudgetDetails(int year)
        {
            try
            {
                var budget = await _reportDbContext.department_budgets
                    .Where(i => i.request_date.Year == year)
                    .OrderBy(i => i.month)
                    .ToListAsync();


                var response = await (
                    from year_report in _reportDbContext.department_budget_year_report
                    where year_report.year == year
                    group year_report by 1 into x
                    select new departmentBudgetYearSummaryResponse
                    {
                        totalAllocated = x.Select(x => x.total_allocated).FirstOrDefault(),
                        totalApproved =  x.Select(x => x.total_approved).FirstOrDefault(),
                        totalRequested = x.Select(x => x.total_requested).FirstOrDefault(), 
                        budgets = budget
                    }).FirstOrDefaultAsync();


                if(response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data has been extracted..",
                        data = (object?)null
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }
    }
}

using APIResponses.BillingResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Controllers
{
    [ApiController]
    [Route("billing/")]
    public class billingController : ControllerBase
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<billingController> _logger;
        private readonly IjournalRepository _repository;

        public billingController(ReportDbContext reportDbContext,ILogger<billingController> logger,IjournalRepository repository)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
            _repository = repository;
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
                    br.billing_date.Year == year
                    group new { bi , br } by bi.item_id into x
                    select new monthPharmacySalesDetailsResponse
                    {
                        itemId = x.Key,
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
        [HttpGet("compareYearBudgets")]
        public async Task<IActionResult> compareYearBudgets([FromQuery]int year,[FromQuery]int partnerYear)
        {
            try
            {
                if (year.Equals(partnerYear))
                {
                    return StatusCode(400,"cannot compare same year");
                }

                var response = await _repository.departmentBudgetComparisonOutcome(year,partnerYear);

                if(response == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"No data found for selected year"
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

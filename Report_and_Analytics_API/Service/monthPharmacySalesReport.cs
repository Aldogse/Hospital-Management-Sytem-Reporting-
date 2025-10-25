
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class monthPharmacySalesReport : BackgroundService
    {
        private readonly ILogger<monthPharmacySalesReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthPharmacySalesReport(ILogger<monthPharmacySalesReport>logger, IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    if(DateTime.Now.Day > 5)
                    {
                        await MonthPharmacySalesReportExtraction(database);
                        await Task.Delay(TimeSpan.FromDays(1));
                    }
                    await Task.Delay(TimeSpan.FromDays(1));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //MONTHLY EXTRACTION THAT HAPPENS EVERY 5TH OF THE FOLLOWING MONTH
        private async Task MonthPharmacySalesReportExtraction(ReportDbContext database)
        {
            DateTime prevMonth = DateTime.Now.AddMonths(-1);
            try
            {
                //Count month total transaction
                var monthTotalTransactions = await (
                    from billingRecords in database.billing_records
                    join billingItems in database.billing_items
                    on billingRecords.billing_id equals billingItems.billing_id
                    where billingRecords.billing_date.Month == prevMonth.Month &&
                    billingRecords.billing_date.Year == prevMonth.Year &&
                    billingItems.item_type == "Pharmacy"
                    select billingRecords.billing_id
                    ).Distinct().CountAsync();

                //Query the best selling item for the month
                var topSellingItem = await
                     (
                         from bi in database.billing_items
                         join br in database.billing_records
                         on bi.billing_id equals br.billing_id
                         where br.billing_date.Month == prevMonth.Month
                         && br.billing_date.Year == prevMonth.Year
                         && bi.item_type == "Pharmacy"
                         && br.status == "Paid"
                         group bi by bi.item_description into x 
                         orderby x.Sum(x => x.total_price) descending
                         select x.Key
                     ).FirstOrDefaultAsync();

                var monthSalesReport = await (
                    from billingRecords in database.billing_records
                    join billingItems in database.billing_items
                    on billingRecords.billing_id equals billingItems.billing_id
                    where billingRecords.billing_date.Month == prevMonth.Month &&
                    billingRecords.billing_date.Year == prevMonth.Year &&
                    billingItems.item_type == "Pharmacy" 
                    group new { billingRecords,billingItems } by 1 into x
                    select new month_pharmacy_sales               
                    {
                        year = prevMonth.Year,
                        month = prevMonth.Month,
                        totalSales = x.Select(x => x.billingRecords.total_amount).Sum(),
                        totalTransactions = monthTotalTransactions, 
                        topSellingItem = topSellingItem,
                    }).FirstOrDefaultAsync();

                if(monthSalesReport == null)
                {
                    _logger.LogWarning($"Expecting data but none was found for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                await database.month_pharmacy_sales.AddAsync(monthSalesReport);
                await database.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogInformation($"Error: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}

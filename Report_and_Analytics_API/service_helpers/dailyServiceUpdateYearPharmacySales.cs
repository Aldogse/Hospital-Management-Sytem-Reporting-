using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.service_helpers
{
    public class dailyServiceUpdateYearPharmacySales : BackgroundService
    {
        private readonly ILogger<dailyServiceUpdateYearPharmacySales> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public dailyServiceUpdateYearPharmacySales(
            ILogger<dailyServiceUpdateYearPharmacySales> logger,
            IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

                    await DailyServiceUpdateYearPharmacySales(database);
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error:{ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }
        }


        private async Task DailyServiceUpdateYearPharmacySales(ReportDbContext reportDbContext)
        {
            DateTime now = DateTime.UtcNow;
            int att = 0;
            int max = 5;

            while (att < max)
            {
                try
                {
                    att++;

                    int month = now.Month;
                    int year = now.Year;

                    // ============================================================
                    // 1. LOAD MONTH REPORT
                    // ============================================================
                    var monthReport = await reportDbContext.month_pharmacy_sales
                        .FirstOrDefaultAsync(i => i.month == month && i.year == year);

                    int lastProcessedMonthlySaleId = monthReport?.lastProcessedSaleId ?? 0;


                    // ============================================================
                    // 2. LOAD YEAR REPORT
                    // ============================================================
                    var yearReport = await reportDbContext.yearly_pharmacy_sales_report
                        .FirstOrDefaultAsync(i => i.year == year);

                    int lastProcessedYearlySaleId = yearReport?.lastProcessedSaleId ?? 0;


                    // ============================================================
                    // 3. FETCH ONLY NEW SALES BASED ON sale_id
                    // ============================================================
                    var newSales = await reportDbContext.pharmacy_sales
                        .Where(i => i.sale_id > lastProcessedMonthlySaleId)
                        .OrderBy(i => i.sale_id)
                        .ToListAsync();

                    if (newSales.Count == 0)
                    {
                        _logger.LogInformation("No new sales to process since last update.");
                        return;
                    }

                    var addedTotalSales = newSales.Sum(i => i.total_price);
                    var addedTotalTransactions = newSales.Count;
                    int maxSaleId = newSales.Max(i => i.sale_id);


                    // ============================================================
                    // ========== PART A: UPDATE OR INSERT MONTHLY REPORT ==========
                    // ============================================================
                    if (monthReport != null)
                    {
                        monthReport.totalSales += addedTotalSales;
                        monthReport.totalTransactions += addedTotalTransactions;
                        monthReport.topSellingItem = await GetTopSellingItem(reportDbContext, month, year);
                        monthReport.lastProcessedSaleId = maxSaleId;

                        reportDbContext.month_pharmacy_sales.Update(monthReport);
                    }
                    else
                    {
                        monthReport = new month_pharmacy_sales
                        {
                            month = month,
                            year = year,
                            totalSales = addedTotalSales,
                            totalTransactions = addedTotalTransactions,
                            topSellingItem = await GetTopSellingItem(reportDbContext, month, year),
                            lastProcessedSaleId = maxSaleId
                        };

                        await reportDbContext.month_pharmacy_sales.AddAsync(monthReport);
                    }

                    await reportDbContext.SaveChangesAsync();


                    // ============================================================
                    // =========== PART B: UPDATE OR INSERT YEARLY REPORT ==========
                    // ============================================================
                    if (yearReport != null)
                    {
                        yearReport.totalSales += addedTotalSales;
                        yearReport.totalTransactions += addedTotalTransactions;
                        yearReport.topSellingItem = await GetTopSellingItem(reportDbContext, null, year);
                        yearReport.lastProcessedSaleId = maxSaleId;

                        reportDbContext.yearly_pharmacy_sales_report.Update(yearReport);
                    }
                    else
                    {
                        yearReport = new yearly_pharmacy_sales_report
                        {
                            year = year,
                            totalSales = addedTotalSales,
                            totalTransactions = addedTotalTransactions,
                            topSellingItem = await GetTopSellingItem(reportDbContext, null, year),
                            lastProcessedSaleId = maxSaleId
                        };

                        await reportDbContext.yearly_pharmacy_sales_report.AddAsync(yearReport);
                    }

                    await reportDbContext.SaveChangesAsync();

                    return;
                }
                catch (Exception ex)
                {
                    att++;
                    _logger.LogError($"Attempt {att}: {ex.Message}");

                    if (att == max)
                        throw;

                    return;
                }
            }
        }

        private async Task<string> GetTopSellingItem(ReportDbContext db, int? month, int year)
        {
            var query = db.pharmacy_sales.Where(i => i.sale_date.Year == year);

            if (month.HasValue)
                query = query.Where(i => i.sale_date.Month == month.Value);

            var grouped = await query
                .GroupBy(i => i.med_name)
                .Select(g => new
                {
                    med_name = g.Key,
                    qty = g.Sum(x => x.quantity_sold)
                })
                .ToListAsync();

            if (grouped.Count == 0)
                return "None";

            int maxQty = grouped.Max(i => i.qty);
            var tied = grouped.Where(i => i.qty == maxQty).ToList();

            return tied.Count > 1 ? "None" : tied.First().med_name;
        }
    }
}
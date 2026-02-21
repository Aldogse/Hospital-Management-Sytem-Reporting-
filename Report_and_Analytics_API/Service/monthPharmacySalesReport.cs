
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.job_logs;

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
            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthPharmacySalesReportExtraction", date.Month, date.Year))
                        {
                            await MonthPharmacySalesReportExtraction(database);
                            await jobRepo.markAsRunThisMonth("MonthPharmacySalesReportExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month.");
                            await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Job failed: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }


        //MONTHLY EXTRACTION THAT HAPPENS EVERY 5TH OF THE FOLLOWING MONTH
        private async Task MonthPharmacySalesReportExtraction(ReportDbContext database)
        {
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    DateTime prevMonth = DateTime.Now;

                    // Count month total transactions
                    var monthTotalTransactions = await database.pharmacy_sales
                        .Where(i => i.sale_date.Month == prevMonth.Month
                                 && i.sale_date.Year == prevMonth.Year)
                        .CountAsync();

                    // Total sales for the month
                    var totalSales = await database.pharmacy_sales
                        .Where(i => i.sale_date.Month == prevMonth.Month
                                 && i.sale_date.Year == prevMonth.Year)
                        .Select(i => i.total_price)
                        .SumAsync();

                    // -------------------------------
                    // TOP SELLING ITEM (With Tie Logic)
                    // -------------------------------
                    var groupedItems = await database.pharmacy_sales
                        .Where(i => i.sale_date.Month == prevMonth.Month
                                 && i.sale_date.Year == prevMonth.Year)
                        .GroupBy(i => i.med_name)
                        .Select(g => new
                        {
                            med_name = g.Key,
                            total_quantity_sold = g.Sum(x => x.quantity_sold),
                            total_sales = g.Sum(x => x.total_price)
                        })
                        .ToListAsync();

                    string finalTopSellingItem = "None";

                    if (groupedItems != null && groupedItems.Count > 0)
                    {
                        // Highest quantity sold
                        var maxQuantity = groupedItems.Max(i => i.total_quantity_sold);

                        // Tie check
                        var tiedItems = groupedItems
                            .Where(i => i.total_quantity_sold == maxQuantity)
                            .ToList();

                        if (tiedItems.Count == 1)
                        {
                            // One clear winner
                            finalTopSellingItem = tiedItems.First().med_name;
                        }
                        else
                        {
                            // Tied → return NONE
                            finalTopSellingItem = "None";
                        }
                    }

                    // -------------------------------
                    // INSERT REPORT
                    // -------------------------------
                    var monthSalesReport = new month_pharmacy_sales
                    {
                        month = prevMonth.Month,
                        year = prevMonth.Year,
                        topSellingItem = finalTopSellingItem,
                        totalTransactions = monthTotalTransactions,
                        totalSales = totalSales
                    };

                    if (monthSalesReport == null)
                    {
                        _logger.LogWarning($"Expecting data but none was found for {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await database.month_pharmacy_sales.AddAsync(monthSalesReport);
                    await database.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Attempt {attempts}: {ex.Message}");

                    if (attempts == maxAttempts)
                    {
                        _logger.LogError("Maximum attempts has been reached.");
                        throw;
                    }

                    return;
                }
            }
        }
    }
}

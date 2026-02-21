using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.service_helpers
{
    public class dailyServiceUpdateYearBillingSummary : BackgroundService
    {
        private readonly ILogger<dailyServiceUpdateYearBillingSummary> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public dailyServiceUpdateYearBillingSummary(
            ILogger<dailyServiceUpdateYearBillingSummary> logger,
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
                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

                    await ProcessBillingReport(db);

                    await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken); // every 2 hours
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Billing Service Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }
        }


        // MAIN PROCESSOR
        private async Task ProcessBillingReport(ReportDbContext db)
        {
            int att = 0;
            int max = 5;

            while (att < max)
            {
                try
                {
                    DateTime now = DateTime.UtcNow;
                    int month = now.Month;
                    int year = now.Year;

                    // ============================================================
                    // Load Existing Monthly Report
                    // ============================================================
                    var monthReport = await db.month_billing_report
                        .FirstOrDefaultAsync(i => i.month == month && i.year == year);

                    int lastProcessedMonthlyId = monthReport?.lastBillingRecordId ?? 0;


                    // ============================================================
                    // Load Existing Yearly Report
                    // ============================================================
                    var yearReport = await db.yearly_billing_report
                        .FirstOrDefaultAsync(i => i.year == year);

                    int lastProcessedYearlyId = yearReport?.lastBillingRecordId ?? 0;


                    // ============================================================
                    // Fetch ONLY new billing records (NO DUPLICATES)
                    // ============================================================
                    int lastProcessedId = Math.Max(lastProcessedMonthlyId, lastProcessedYearlyId);

                    var newBills = await db.billing_records
                        .Where(b => b.billing_id > lastProcessedId)
                        .OrderBy(b => b.billing_id)
                        .ToListAsync();

                    if (newBills.Count == 0)
                    {
                        _logger.LogInformation("No new billing records to process.");
                        return;
                    }

                    int maxBillingRecordId = newBills.Max(b => b.billing_id);

                    // Summaries
                    decimal? totalBilled = newBills.Sum(b => b.total_amount);
                    decimal? totalPaid = newBills.Sum(b => b.paid_amount);

                    int pendingTransactions = newBills.Count(b => b.payment_status != "Paid");
                    decimal? pendingAmount = newBills.Where(b => b.payment_status != "Paid")
                                                    .Sum(b => b.balance);

                    decimal? oopCollected = newBills.Sum(b => b.out_of_pocket);
                    decimal? insuranceCovered = newBills.Sum(b => b.insurance_covered);


                    // ============================================================
                    // UPDATE / INSERT MONTHLY REPORT
                    // ============================================================
                    if (monthReport != null)
                    {
                        monthReport.total_billed += totalBilled;
                        monthReport.total_paid += totalPaid;
                        monthReport.total_pending_transaction += pendingTransactions;
                        monthReport.total_pending_amount += pendingAmount;
                        monthReport.total_oop_collected += oopCollected;
                        monthReport.total_insurance_covered += insuranceCovered;
                        monthReport.lastBillingRecordId = maxBillingRecordId;

                        db.month_billing_report.Update(monthReport);
                    }
                    else
                    {
                        monthReport = new month_billing_report
                        {
                            month = month,
                            year = year,
                            total_billed = totalBilled,
                            total_paid = totalPaid,
                            total_pending_transaction = pendingTransactions,
                            total_pending_amount = pendingAmount,
                            total_oop_collected = oopCollected,
                            total_insurance_covered = insuranceCovered,
                            lastBillingRecordId = maxBillingRecordId
                        };

                        await db.month_billing_report.AddAsync(monthReport);
                    }

                    await db.SaveChangesAsync();


                    // ============================================================
                    // UPDATE / INSERT YEARLY REPORT
                    // ============================================================
                    if (yearReport != null)
                    {
                        yearReport.total_billed += totalBilled;
                        yearReport.total_paid += totalPaid;
                        yearReport.total_pending_transaction += pendingTransactions;
                        yearReport.total_pending_amount += pendingAmount;
                        yearReport.total_oop_collected += oopCollected;
                        yearReport.total_insurance_covered += insuranceCovered;
                        yearReport.lastBillingRecordId = maxBillingRecordId;

                        db.yearly_billing_report.Update(yearReport);
                    }
                    else
                    {
                        yearReport = new yearly_billing_report
                        {
                            year = year,
                            total_billed = totalBilled,
                            total_paid = totalPaid,
                            total_pending_transaction = pendingTransactions,
                            total_pending_amount = pendingAmount,
                            total_oop_collected = oopCollected,
                            total_insurance_covered = insuranceCovered,
                            lastBillingRecordId = maxBillingRecordId
                        };

                        await db.yearly_billing_report.AddAsync(yearReport);
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempts{att}: {ex.Message}");

                    if(att == max)
                    {
                        _logger.LogError("Maximum limit reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
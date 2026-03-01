using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.service_helpers
{
    public class InsuranceClaimBackgroundService : BackgroundService
    {
        private readonly ILogger<InsuranceClaimBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public InsuranceClaimBackgroundService(
            ILogger<InsuranceClaimBackgroundService> logger,
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

                    await ProcessInsuranceClaims(db);

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Insurance Claim Service Error: {ex}");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }
        }

        private async Task ProcessInsuranceClaims(ReportDbContext db)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            // ============================================================
            // 1. FIND TODAY’S DAILY REPORT
            // ============================================================
            var dailyReport = await db.daily_insurance_submitted_report
                .FirstOrDefaultAsync(d => d.report_date >= today && d.report_date < today.AddDays(1));

            // ============================================================
            // 2. CREATE daily report if NONE exists for today
            // ============================================================
            if (dailyReport == null)
            {
                // Get last claim in database ONCE for NEW DAYS ONLY
                int lastClaimInDb = await db.insurance_claims
                    .OrderByDescending(c => c.insurance_claims_id)
                    .Select(c => c.insurance_claims_id)
                    .FirstOrDefaultAsync();

                dailyReport = new daily_insurance_submitted_report
                {
                    report_date = today,
                    claim_amount_submitted = 0,
                    claims_amount_denied = 0,
                    claims_approved = 0,
                    claims_pending = 0,
                    claims_denied = 0,
                    number_of_claims_submitted = 0,

                    // IMPORTANT:
                    // Start at latest claim when new day begins
                    lastProcessClaimid = lastClaimInDb
                };

                await db.daily_insurance_submitted_report.AddAsync(dailyReport);
                await db.SaveChangesAsync();
            }

            // ============================================================
            // 3. GET NEW CLAIMS ONLY AFTER LAST PROCESSED CLAIM ID
            // ============================================================
            var newClaims = await db.insurance_claims
                .Where(c => c.insurance_claims_id > dailyReport.lastProcessClaimid)
                .OrderBy(c => c.insurance_claims_id)
                .ToListAsync();

            if (newClaims.Count == 0)
                return;

            // ============================================================
            // 4. UPDATE DAILY REPORT TOTALS
            // ============================================================
            foreach (var claim in newClaims)
            {
                dailyReport.number_of_claims_submitted++;
                dailyReport.claim_amount_submitted += claim.claim_amount;

                if (claim.status == "approved")
                {
                    dailyReport.claims_approved++;
                }
                else if (claim.status == "denied")
                {
                    dailyReport.claims_denied++;
                    dailyReport.claims_amount_denied += claim.claim_amount;
                }
                else
                {
                    dailyReport.claims_pending++;
                }

                // Update to last processed claim
                dailyReport.lastProcessClaimid = claim.insurance_claims_id;
            }

            await db.SaveChangesAsync();

            // ============================================================
            // 5. UPDATE MONTHLY REPORT
            // ============================================================
            int month = today.Month;
            int year = today.Year;

            var monthReport = await db.monthly_claim_report
                .FirstOrDefaultAsync(m => m.month == month && m.year == year);

            if (monthReport == null)
            {
                monthReport = new monthly_claim_report
                {
                    month = month,
                    year = year,
                    total_claims = 0,
                    total_approved_claims = 0,
                    total_denied_claims = 0,
                    total_amount_paid = 0,
                    total_amount_denied = 0,
                    lastInsuranceClaimIdProcessed = 0
                };

                await db.monthly_claim_report.AddAsync(monthReport);
            }

            monthReport.total_claims += newClaims.Count;
            monthReport.total_approved_claims += newClaims.Count(c => c.status == "approved");
            monthReport.total_denied_claims += newClaims.Count(c => c.status == "denied");
            monthReport.total_amount_paid += newClaims
                .Where(c => c.status == "approved")
                .Sum(c => c.claim_amount);
            monthReport.total_amount_denied += newClaims
                .Where(c => c.status == "denied")
                .Sum(c => c.claim_amount);

            monthReport.lastInsuranceClaimIdProcessed = dailyReport.lastProcessClaimid;

            await db.SaveChangesAsync();

            // ============================================================
            // 6. UPDATE YEARLY REPORT
            // ============================================================
            var yearlyReport = await db.yearly_claim_report
                .FirstOrDefaultAsync(y => y.year == year);

            if (yearlyReport == null)
            {
                yearlyReport = new yearly_claim_report
                {
                    year = year,
                    total_claims = 0,
                    total_approved_claims = 0,
                    total_denied_claims = 0,
                    total_amount_paid = 0,
                    total_amount_denied = 0,
                    lastInsuranceClaimIdProcessed = 0
                };

                await db.yearly_claim_report.AddAsync(yearlyReport);
            }

            yearlyReport.total_claims += newClaims.Count;
            yearlyReport.total_approved_claims += newClaims.Count(c => c.status == "approved");
            yearlyReport.total_denied_claims += newClaims.Count(c => c.status == "denied");
            yearlyReport.total_amount_paid += newClaims
                .Where(c => c.status == "approved")
                .Sum(c => c.claim_amount);
            yearlyReport.total_amount_denied += newClaims
                .Where(c => c.status == "denied")
                .Sum(c => c.claim_amount);

            yearlyReport.lastInsuranceClaimIdProcessed = dailyReport.lastProcessClaimid;

            await db.SaveChangesAsync();
        }
    }
}
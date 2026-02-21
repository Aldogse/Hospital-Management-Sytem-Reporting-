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

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Every 2 hours
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Insurance Claim Service Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
                }
            }
        }

        private async Task ProcessInsuranceClaims(ReportDbContext db)
        {
            DateTime now = DateTime.UtcNow;

            int month = now.Month;
            int year = now.Year;

            // ============================================================
            // 1. Load existing MONTHLY claim report
            // ============================================================
            var monthReport = await db.monthly_claim_report
                .FirstOrDefaultAsync(i => i.month == month && i.year == year);

            int lastMonthlyClaimId = monthReport?.lastInsuranceClaimIdProcessed ?? 0;


            // ============================================================
            // 2. Load existing YEARLY claim report
            // ============================================================
            var yearReport = await db.yearly_claim_report
                .FirstOrDefaultAsync(i => i.year == year);

            int lastYearlyClaimId = yearReport?.lastInsuranceClaimIdProcessed ?? 0;


            // Get the last processed ID for either (max)
            int lastProcessedId = Math.Max(lastMonthlyClaimId, lastYearlyClaimId);


            // ============================================================
            // 3. Fetch ONLY NEW CLAIMS
            // ============================================================
            var newClaims = await db.insurance_claims
                .Where(c => c.insurance_claims_id > lastProcessedId)
                .OrderBy(c => c.insurance_claims_id)
                .ToListAsync();

            if (newClaims.Count == 0)
            {
                _logger.LogInformation("No new insurance claims to process.");
                return;
            }

            // CLAIM CALCULATIONS
            int totalClaims = newClaims.Count;
            int approved = newClaims.Count(c => c.status.ToLower() == "approved");
            int denied = newClaims.Count(c => c.status.ToLower() == "denied");

            // The highest ID processed this round
            int maxClaimId = newClaims.Max(c => c.insurance_claims_id);


            // ============================================================
            // 4. UPDATE / INSERT MONTHLY REPORT
            // ============================================================
            if (monthReport != null)
            {
                monthReport.total_claims += totalClaims;
                monthReport.total_approved_claims += approved;
                monthReport.total_denied_claims += denied;
                monthReport.lastInsuranceClaimIdProcessed = maxClaimId;

                db.monthly_claim_report.Update(monthReport);
            }
            else
            {
                monthReport = new monthly_claim_report
                {
                    month = month,
                    year = year,
                    total_claims = totalClaims,
                    total_approved_claims = approved,
                    total_denied_claims = denied,
                    lastInsuranceClaimIdProcessed = maxClaimId
                };

                await db.monthly_claim_report.AddAsync(monthReport);
            }

            await db.SaveChangesAsync();


            // ============================================================
            // 5. UPDATE / INSERT YEARLY REPORT
            // ============================================================
            if (yearReport != null)
            {
                yearReport.total_claims += totalClaims;
                yearReport.total_approved_claims += approved;
                yearReport.total_denied_claims += denied;
                yearReport.lastInsuranceClaimIdProcessed = maxClaimId;

                db.yearly_claim_report.Update(yearReport);
            }
            else
            {
                yearReport = new yearly_claim_report
                {
                    year = year,
                    total_claims = totalClaims,
                    total_approved_claims = approved,
                    total_denied_claims = denied,
                    lastInsuranceClaimIdProcessed = maxClaimId
                };

                await db.yearly_claim_report.AddAsync(yearReport);
            }

            await db.SaveChangesAsync();
        }
    }
}
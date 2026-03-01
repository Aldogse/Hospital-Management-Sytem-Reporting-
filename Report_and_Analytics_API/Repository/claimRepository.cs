using APIResponses.claim_response;
using APIResponses.Historical_report.Models;
using APIResponses.Historical_report.training_models_prediction;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.Enums;
using Report_and_Analytics_Library.Insurance;

namespace Report_and_Analytics_API.Repository
{
    public class claimRepository : IinsuranceClaimRepository
    {
        private readonly ReportDbContext _reportDbContext;

        public claimRepository(ReportDbContext reportDbContext)
        {
            _reportDbContext = reportDbContext;
        }

        public async Task<monthly_claim_report> getMonthClaimReports(int month, int year)
        {
            var monthReports = await _reportDbContext.monthly_claim_report
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();
          
            return monthReports;
        }


        //QUERY FOR BACKGROUND SERVICE
        public async Task<daily_insurance_submitted_report> getDailyTransactionsSummary(DateOnly date)
        {
            var today = DateOnly.FromDateTime(new DateTime(DateTime.Now.Month,DateTime.Now.Year,1));
            var transactions = await (
                from claims in _reportDbContext.insurance_claims
                where claims.submmited_date == date
                group claims by 1 into x
                select new daily_insurance_submitted_report
                {       
                    report_date = today,
                    claim_amount_submitted = x.Sum(i => i.claim_amount_submitted),
                    number_of_claims_submitted = x.Where(i => i.submmited_date == date).Count(),
                    claims_approved = x.Where(i => i.status == "Approved" && i.resolved_date == date).Count()
                }).FirstOrDefaultAsync();

            return transactions;
        }



        //QUERY FOR PREDICTION TRAINING MODEL
        //QUERIES FOR ML MODEL
        public async Task<List<month_insurance_claims_status_training_data>> getProvidersClaimsHistoryStatus(int month, int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            var last2MonthsStartDate = startDate.AddMonths(-2);
            var last2MonthsEndDate = last2MonthsStartDate.AddMonths(1);

            var claimsInfo = await (
                from provider in _reportDbContext.insurance_provider
                join claims in _reportDbContext.insurance_claims
                on provider.insurance_provider_id equals claims.insurance_provider_id
                where claims.resolved_date >= startDate && claims.resolved_date < endDate
                group new {provider,claims} by provider.insurance_provider_id into x
                select new month_insurance_claims_status_training_data
                {
                    insurance_provider_id = x.Key,
                    month = month,
                    year = year,

                    total_claims = x.Count(),
                    total_claim_approved = x.Count(i => i.claims.status == "approved"),
                    total_claim_denied = x.Count(i => i.claims.status == "denied"),

                    //PREVIOUS month data
                    last_month_approved_claims = x
                    .Where(i => i.claims.resolved_date >= last2MonthsStartDate && i.claims.resolved_date < last2MonthsEndDate
                    && i.claims.status == "approved").Count(),

                    last_month_denied_claims = x
                    .Where(i => i.claims.resolved_date >= last2MonthsStartDate && i.claims.resolved_date < last2MonthsEndDate
                    && i.claims.status == "denied").Count(),

                }).ToListAsync();

            return claimsInfo;
        }

        public async Task<List<month_insurance_claim_amount_training_data>> getProvidersClaimHistoryAmount(int month, int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            var last2monthsStartDate = startDate.AddMonths(-2);
            var last2monthsEndDate = last2monthsStartDate.AddMonths(1);

            var claimAmount = await (
                from provider in _reportDbContext.insurance_provider
                join claims in _reportDbContext.insurance_claims
                on provider.insurance_provider_id equals claims.insurance_provider_id
                where claims.resolved_date >= startDate && claims.resolved_date < endDate
                group new { provider, claims } by provider.insurance_provider_id into x
                select new month_insurance_claim_amount_training_data
                {
                   insurance_provider_id = x.Key,
                   month = month,
                   year = year,

                   total_claim_amount_submitted = x.Sum(i => i.claims.claim_amount_submitted),

                   total_claim_approved_amount = x
                   .Where(i => i.claims.status == "approved").Sum(i => i.claims.claim_amount_submitted),

                   total_claim_declined_amount = x
                   .Where(i => i.claims.status == "denied").Sum(i => i.claims.claim_amount_submitted),

                   last_month_total_claim_approved_amount = x
                   .Where(i => i.claims.resolved_date >= last2monthsStartDate && i.claims.resolved_date < last2monthsEndDate)
                   .Sum(i => i.claims.claim_amount_submitted),

                    last_month_total_claim_declined_amount = x
                   .Where(i => i.claims.resolved_date >= last2monthsStartDate && i.claims.resolved_date < last2monthsEndDate)
                   .Sum(i => i.claims.claim_amount_submitted)

                }).ToListAsync();

            return claimAmount;
        }

        public async Task<List<month_insurance_claims_status_forecast_result>> getMonthProviderClaimStatusForecast(int month,int year)
        {
            var report = await _reportDbContext.month_insurance_claims_status_forecast_result
                .Where(i => i.month == month &&i.year == year)
                .ToListAsync();

            return report;
        }

        public async Task<List<month_insurance_claim_amount_forecast_result>> getMonthProviderClaimsAmountForecast(int month,int year)
        {
            var report = await _reportDbContext.month_insurance_claim_amount_forecast_result
               .Where(i => i.month == month && i.year == year)
               .ToListAsync();

            return report;
        }

        public async Task<List<monthProviderClaimReport>> getProvidersMonthPerformance(int month,int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            var report = await (
                from provider in _reportDbContext.insurance_provider
                join claims in _reportDbContext.insurance_claims
                on provider.insurance_provider_id equals claims.insurance_provider_id
                where claims.resolved_date >= startDate && claims.resolved_date < endDate
                group new {provider,claims} by provider.insurance_provider_id into x
                select new monthProviderClaimReport
                {
                    month = month,
                    year = year,
                    approvedAmount = x.Where(i => i.claims.status == "approved").Select(i => i.claims.claim_amount_submitted).Sum(),
                    declinedAmount = x.Where(i => i.claims.status == "denied").Select(i => i.claims.claim_amount_submitted).Sum(),
                    provider_id = x.Key
                }
                ).ToListAsync();

            return report;
        }

        public async Task<monthClaimsHistory> getMonthsClaimHistory(int month, int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            //SUMMARY OF MONTHLY CLAIMS
            var monthClaimsInformation = await _reportDbContext.monthly_claim_report.Where(i => i.month == month
            && i.year == year).FirstOrDefaultAsync();


            var claimsReport = await (
                from claims in _reportDbContext.insurance_claims
                where claims.submmited_date >= startDate && claims.submmited_date < endDate
                group claims by 1 into x
                select new monthClaimsHistory
                {
                    month = month,
                    year = year,
                    totalClaims = monthClaimsInformation.total_claims,
                    totalApprovedClaims = monthClaimsInformation.total_approved_claims,
                    totalDeniedClaims = monthClaimsInformation.total_denied_claims,
                    totalApprovedAmount = monthClaimsInformation.total_amount_paid,
                    totalDeniedAmount = monthClaimsInformation.total_amount_denied
                })            
                .FirstAsync();
        
            return claimsReport;
        }
        //GET ALL CLAIMS FOR THE MONTH
        public async Task<List<insurance_claims>> monthClaims(int month,int year,int page,int size)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            var claims = await _reportDbContext.insurance_claims
                .Where(i => i.resolved_date >= startDate
                 && i.resolved_date < endDate)
                .Skip((page - 1) * size)
                .Take(size)
                .OrderBy(i => i.resolved_date)
                .ToListAsync();

            return claims;
        }


        public async Task<monthly_claim_report> getMonthClaimReport(int month, int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year,month,1));
            var endDate = startDate.AddMonths(1);

            var monthReport = await _reportDbContext.insurance_claims
                .Where(i => i.resolved_date >= startDate && i.resolved_date < endDate).ToListAsync();

            var response = monthReport.Select(i => new monthly_claim_report
            {
                month = month,
                year = year,
                total_claims = monthReport.Count(),
                total_approved_claims = monthReport.Count(i => i.status == "approved"),
                total_denied_claims = monthReport.Count(i => i.status == "denied"),
                total_amount_denied = monthReport.Where(i => i.status == "denied")
                .Select(i => i.claim_amount_submitted).Sum(),
                total_amount_paid = monthReport.Where(i => i.status == "approved")
                .Select(i => i.claim_amount_submitted).Sum(),
            }).FirstOrDefault();

            return response;
                
        }

        public async Task<yearly_claim_report> getYearClaimReport(int year)
        {
            var yearReport = await _reportDbContext.monthly_claim_report.Where(i => i.year == year).ToListAsync();

            return new yearly_claim_report
            {
                total_approved_claims = yearReport.Sum(i => i.total_approved_claims),
                total_claims = yearReport.Sum(i => i.total_claims),
                total_denied_claims = yearReport.Sum(i => i.total_denied_claims),
                year = year
            };
        }

        public async Task<yearlyClaimReportResponse> yearClaimsSummary(int year)
        {
            var yearReport = await _reportDbContext.yearly_claim_report.Where(i => i.year == year).FirstOrDefaultAsync();
            var monthSummary = await _reportDbContext.monthly_claim_report.Where(i => i.year == year).ToListAsync();

            return new yearlyClaimReportResponse
            {
                total_approved_claims = yearReport?.total_approved_claims ?? 0,
                total_claims = yearReport?.total_claims ?? 0,
                total_denied_claims = yearReport?.total_denied_claims ?? 0,
                year = year,
                monthsClaim = monthSummary
            };
        }

        public async Task<monthsComparisonClaimResponse> monthClaimsComparison(int month, int year, int parterMont, int partnerYear)
        {
            var baseMonth = await _reportDbContext.monthly_claim_report.Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();
            var partnerMonth = await _reportDbContext.monthly_claim_report.Where(i => i.month == parterMont && i.year == partnerYear)
                .FirstOrDefaultAsync();

            return new monthsComparisonClaimResponse
            {
                basemonth = month,
                baseyear = year,
                base_total_approved_claims = baseMonth?.total_approved_claims ?? 0,
                base_total_claims = baseMonth?.total_claims ?? 0,
                base_total_denied_claims = baseMonth?.total_denied_claims ?? 0,

                partnermonth = parterMont,
                partneryear = partnerYear,
                partner_total_approved_claims = partnerMonth?.total_approved_claims ?? 0,
                partner_total_claims = partnerMonth?.total_claims ?? 0,
                partner_total_denied_claims = partnerMonth?.total_denied_claims ?? 0
            };
        }

        public async Task<yearClaimSummaryDetails> yearInsuranceSummaryReport(int year)
        {
            var insuranceList = await _reportDbContext.insurance_claims.Where(i => i.submmited_date.Year == year)
                .ToListAsync();

            var report = new yearClaimSummaryDetails
            {
                year = year,
                totalApprovePayoutAmount = insuranceList.Where(i => i.status == "approved")
                .Select(i => i.claim_amount_submitted).Sum(),
                totalHospitalLoss = insuranceList.Where(i => i.status == "denied")
                .Select(i => i.claim_amount_submitted).Sum(),
                totalClaimApproved = insuranceList.Where(i => i.status == "approved").Count(),
                totalClaimDenied = insuranceList.Where(i => i.status == "denied").Count(),               
            };

            return report;
        }

        //NEW SERVICE QUERIES
        public async Task<monthInsuranceClaimRangeQuery> monthInsuranceRangeQuery(DateOnly start, DateOnly end)
        {
            // Convert DateOnly → DateTime for EF filtering
            DateTime startDt = start.ToDateTime(TimeOnly.MinValue);
            DateTime endDt = end.ToDateTime(TimeOnly.MaxValue);

            // ================================================================================
            // 1. GET CLAIMS WITHIN RANGE (using resolved_date)
            // ================================================================================
            var claims = await _reportDbContext.insurance_claims
                .Where(c => c.resolved_date >= start && c.resolved_date <= end)
                .ToListAsync();

            // ================================================================================
            // 2. TOTAL SUMMARY CALCULATION
            // ================================================================================
            var summary = new monthInsuranceClaimRangeQuery
            {
                total_claims = claims.Count,
                total_approved_claims = claims.Count(c => c.status == "approved"),
                total_denied_claims = claims.Count(c => c.status == "denied"),

                total_amount_approved = claims
                    .Where(c => c.status == "approved")
                    .Sum(c => c.claim_amount_submitted),

                total_amount_denied = claims
                    .Where(c => c.status == "denied")
                    .Sum(c => c.claim_amount_submitted),

                months = new List<monthly_claim_report>(),
                providers = new List<provider_claim_report>()
            };

            // ================================================================================
            // 3. MONTHLY GROUPING SUMMARY USING ONLY DateOnly PROPERTIES
            // ================================================================================
            summary.months = claims
                .GroupBy(c => new
                {
                    Year = c.submmited_date.Year,
                    Month = c.submmited_date.Month
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new monthly_claim_report
                {
                    month = g.Key.Month,

                    total_claims = g.Count(),
                    total_approved_claims = g.Count(c => c.status == "approved"),
                    total_denied_claims = g.Count(c => c.status == "denied"),

                    total_amount_paid = g.Where(c => c.status == "approved")
                                         .Sum(c => c.claim_amount_submitted),

                    total_amount_denied = g.Where(c => c.status == "denied")
                                           .Sum(c => c.claim_amount_submitted)
                })
                .ToList();

            // ================================================================================
            // 4. PROVIDER SUMMARY (DateOnly-safe filtering)
            // ================================================================================
            summary.providers = await (
                from p in _reportDbContext.insurance_provider
                join c in _reportDbContext.insurance_claims
                    on p.insurance_provider_id equals c.insurance_provider_id into claimGroup
                from cg in claimGroup.DefaultIfEmpty()

                where cg == null ||
                      (cg.submmited_date >= start && cg.submmited_date <= end)

                group cg by new
                {
                    p.insurance_provider_id,
                    p.name
                }
                into g

                select new provider_claim_report
                {
                    provider_id = g.Key.insurance_provider_id,
                    provider_name = g.Key.name,

                    total_claims = g.Count(x => x != null),
                    approved_claims = g.Count(x => x != null && x.status == "approved"),
                    denied_claims = g.Count(x => x != null && x.status == "denied"),

                    approved_amount = g.Where(x => x != null && x.status == "approved")
                                       .Sum(x => x.claim_amount_submitted),

                    denied_amount = g.Where(x => x != null && x.status == "denied")
                                     .Sum(x => x.claim_amount_submitted)
                }
            ).ToListAsync();

            return summary;
        }

        public async Task<dateRangeSummaryClaimQueryResponse> dateRangeSummaryResponseQuery(DateOnly start, DateOnly end)
        {
            var data = await _reportDbContext.daily_insurance_submitted_report
               .Where(i => i.report_date >= start && i.report_date <= end)
               .ToListAsync();

            return new dateRangeSummaryClaimQueryResponse
            {
                claims_amount_denied = data.Sum(i => i.claims_amount_denied),
                claims_approved = data.Sum(i => i.claims_approved),
                claim_amount_submitted = data.Sum(i => i.claim_amount_submitted),
                claims_denied = data.Sum(i => i.claims_denied),
                claims_pending = data.Sum(i => i.claims_pending),
                number_of_claims_submitted = data.Sum(i => i.number_of_claims_submitted),
                days = data
            };
        }
    }
} 

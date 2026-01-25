using APIResponses.claim_response;
using APIResponses.Historical_report.Models;
using APIResponses.Historical_report.training_models_prediction;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.Enums;

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
            var transactions = await (
                from claims in _reportDbContext.insurance_claims
                where claims.submmited_date == date
                group claims by 1 into x
                select new daily_insurance_submitted_report
                {       
                    report_date = DateTime.Now,
                    claim_amount = x.Sum(i => i.claim_amount),
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

                   total_claim_amount_submitted = x.Sum(i => i.claims.claim_amount),

                   total_claim_approved_amount = x
                   .Where(i => i.claims.status == "approved").Sum(i => i.claims.claim_amount),

                   total_claim_declined_amount = x
                   .Where(i => i.claims.status == "denied").Sum(i => i.claims.claim_amount),

                   last_month_total_claim_approved_amount = x
                   .Where(i => i.claims.resolved_date >= last2monthsStartDate && i.claims.resolved_date < last2monthsEndDate)
                   .Sum(i => i.claims.claim_amount),

                    last_month_total_claim_declined_amount = x
                   .Where(i => i.claims.resolved_date >= last2monthsStartDate && i.claims.resolved_date < last2monthsEndDate)
                   .Sum(i => i.claims.claim_amount)

                }).ToListAsync();

            return claimAmount;
        }

        public async Task<List<month_insurance_claims_status_forecast_result>> getMonthProviderClaimStatusForecast(int year)
        {
            var report = await _reportDbContext.month_insurance_claims_status_forecast_result
                .Where(i => i.year == year).ToListAsync();

            return report;
        }

        public async Task<List<month_insurance_claim_amount_forecast_result>> getMonthProviderClaimsAmountForecast(int year)
        {
            var report = await _reportDbContext.month_insurance_claim_amount_forecast_result
               .Where(i => i.year == year).ToListAsync();

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
                    approvedAmount = x.Where(i => i.claims.status == "approved").Select(i => i.claims.claim_amount).Sum(),
                    declinedAmount = x.Where(i => i.claims.status == "denied").Select(i => i.claims.claim_amount).Sum(),
                    provider_id = x.Key
                }
                ).ToListAsync();

            return report;
        }

    }
}

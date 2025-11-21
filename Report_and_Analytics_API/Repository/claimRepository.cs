using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

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
    }
}

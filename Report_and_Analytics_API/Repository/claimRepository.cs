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
    }
}

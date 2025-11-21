using APIResponses.Historical_report.Models;

namespace Report_and_Analytics_API.Interface
{
    public interface IinsuranceClaimRepository
    {
       Task<monthly_claim_report> getMonthClaimReports(int month,int year);
       Task<daily_insurance_submitted_report> getDailyTransactionsSummary(DateOnly date);

    }
}

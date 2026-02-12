using System.ComponentModel;
using APIResponses.claim_response;
using APIResponses.Historical_report.Models;
using APIResponses.Historical_report.training_models_prediction;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Report_and_Analytics_Library.Insurance;

namespace Report_and_Analytics_API.Interface
{
    public interface IinsuranceClaimRepository
    {
       Task<monthly_claim_report> getMonthClaimReports(int month,int year);
       Task<daily_insurance_submitted_report> getDailyTransactionsSummary(DateOnly date);
       Task<List<month_insurance_claims_status_forecast_result>> getMonthProviderClaimStatusForecast(int year);
       Task<List<month_insurance_claim_amount_forecast_result>> getMonthProviderClaimsAmountForecast(int year);
       Task<List<monthProviderClaimReport>> getProvidersMonthPerformance(int month,int year);
       Task<monthClaimsHistory> getMonthsClaimHistory(int month, int year,int page,int size);
       Task<List<insurance_claims>> monthClaims(int month,int year,int page,int size);

        //QUERIES FOR BACKGROUND SERVICES
        Task<List<month_insurance_claims_status_training_data>> getProvidersClaimsHistoryStatus(int month,int year);
        Task<List<month_insurance_claim_amount_training_data>> getProvidersClaimHistoryAmount(int month,int year);
        Task<monthly_claim_report> getMonthClaimReport(int month,int year);
    }
}

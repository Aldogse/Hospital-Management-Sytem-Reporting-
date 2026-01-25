using APIResponses.forecast_results;
using APIResponses.Historical_report.Models;

namespace Report_and_Analytics_API.Interface
{
    public interface IpatientAdmissionRepository
    {
        Task<int> getMonthTotalAdmissions(int month,int year);
        Task<int> getPreviousMonthTotalAdmissions(int month,int year);
        Task<int> getLastThreeMonthsTotalAdmissions(DateTime startDate,DateTime endDate);
        Task<int> getLastSixMonthsTotalAdmissions(DateTime startDate,DateTime endDate);
        Task<month_patient_admission_forecast_result> getMonthPatientForecast(int month,int year);
        Task<List<object>> getPreviousMonthsPatientAdmission(int year);
    }
}

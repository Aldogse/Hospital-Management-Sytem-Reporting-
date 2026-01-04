using APIResponses.Historical_report.Models;

namespace Report_and_Analytics_API.Interface
{
    public interface IpatientAdmissionRepository
    {
        Task<int> getMonthTotalAdmissions(int month,int year);
        Task<int> getPreviousMonthTotalAdmissions(int month,int year);
        Task<int> getLastThreeMonthsTotalAdmissions(DateTime startDate,DateTime endDate);
        Task<int> getLastSixMonthsTotalAdmissions(DateTime startDate,DateTime endDate);
    }
}

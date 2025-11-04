using APIResponses.Historical_report.Models;
using APIResponses.PropertyAndManagementResponse;

namespace Report_and_Analytics_API.Interface
{
    public interface IpropertyRepository
    {
        Task<month_admission_and_discharge_report> getMonthAdmissionAndDischargeReport(int year,int month);
        Task<List<monthAdmissionReportResponse>> getAdmissionReport(int year,int month);
        Task<List<monthDischargeReportResponse>> getDischargeReport(int year,int month);
    }
}

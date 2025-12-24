using APIResponses.Historical_report.Models;
using APIResponses.PropertyAndManagementResponse;
using APIResponses.Training_Models;

namespace Report_and_Analytics_API.Interface
{
    public interface IpropertyRepository
    {
        //BACKGROUND SERVICE QUERY
        Task<month_admission_and_discharge_report> getMonthAdmissionAndDischargeReport(int month,int year);
        Task<List<monthAdmissionReportResponse>> getAdmissionReport(int year,int month);
        Task<List<monthDischargeReportResponse>> getDischargeReport(int month,int year);
        Task<month_admission_and_discharge_report> getPreviousMonthAdmissionReport(int month,int year);
        Task<daily_beds_utilization_report> getDailyBedsUtilizationReport(DateTime date);
        Task<month_admission_and_discharge_report> getYearAdmissionsAndDischargeReport(int year);
        Task<List<month_admission_and_discharge_report>> getMonthsAdmissionData(int year);

        //ENDPOINT QUERIES
        Task<yearly_admission_and_discharge_report> yearlyAdmissionAndDischargeReport(int year);
        Task<List<month_admission_and_discharge_report>> monthBedsDistribution(int year);
    }
}

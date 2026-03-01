using APIResponses.forecast_results;
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
        Task<List<yearTotalOccupiedBedsResponse>> getYearAdmissionDataReport(int year);
        Task<int> numberOfBeds();

        //ENDPOINT QUERIES
        Task<yearly_admission_and_discharge_report> yearlyAdmissionAndDischargeReport(int year);
        Task<List<month_admission_and_discharge_report>> monthBedsDistribution(int year);

        //FORECASTED QUERY RESULTS
        Task<month_bed_occupancy_forecast_result> monthForecastedBedOccupancyRate(int month,int year);

        //NEW SERVICE QUERIES
        Task<monthBedsAndDischargesQueryResponse> monthBedsAndDishcargeRangeQuery(int startmonth,int startyear,int endmonth,int endyear);
    }
}

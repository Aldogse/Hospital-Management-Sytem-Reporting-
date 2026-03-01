using APIResponses.Historical_report.Models;
using APIResponses.PayrollResponse;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<monthPayrollSummaryResponse> monthPayrollSummaryResponse(int month, int year);
        Task<month_payroll_summary> hospitalMonthPayrollReport(int month, int year);
        Task<monthPayrollComparisonResponse> monthPayrollComparisonResult(int month,int year,int comparedMonth,int comparedYear);
        Task<year_hospital_payroll_report> getYearHospitalPayrollReport(int year);
        Task<yearSummaryPayrollResponse> yearHospitalPayrollSummary(int year);
        Task<monthPayrollQueryRangeResponse> monthPayrollRangeQueryAsync(int startmonth,int startyear,int endmonth,int endyear);
    }
}

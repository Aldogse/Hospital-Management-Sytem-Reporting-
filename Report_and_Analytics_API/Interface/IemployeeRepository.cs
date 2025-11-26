using APIResponses.Employee_Responses;
using APIResponses.Historical_report.Models;

namespace Report_and_Analytics_API.Interface
{
    public interface IemployeeRepository
    {
        Task<dailyAttendanceReportResponse> getDayAttendanceReport(DateTime date);
        Task<month_attendance_report> getMonthAttendanceReport(int month,int year);
        Task<month_attendance_report> getMonthAttendanceReportSummary(int month, int year);
        Task<year_attendance_report> getYearAttendanceReport(int year);
        Task<year_attendance_report> getYearAttendanceReportSummary(int year);
        Task<month_employees_performance_and_evaluation_report> getMonthEmployeePerformanceReport(int month,int year);
        Task<month_employees_performance_and_evaluation_report> monthEmployeesPerformanceReport(int month,int year);
        Task<List<monthPerformanceSummaryListResponse>> getMonthEmployeePerformanceSummarryList(int month,int year,int page,int size);
    }
}

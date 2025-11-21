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
    }
}

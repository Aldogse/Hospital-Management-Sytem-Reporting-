using APIResponses.Employee_Responses;
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Report_and_Analytics_API.Repository
{
    public class employeeRepository : IemployeeRepository
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<dailyAttendanceReportResponse> _logger;

        public employeeRepository(ReportDbContext reportDbContext,ILogger<dailyAttendanceReportResponse>logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }
        public async Task<dailyAttendanceReportResponse> getDayAttendanceReport(DateTime date)
        {
            var employees = await _reportDbContext.hr_employees.CountAsync();
            var report = await _reportDbContext.daily_attendance_report
                .Where(i => i.reportDate == date)
                .Select(i => new dailyAttendanceReportResponse
                {
                    reportDate = i.reportDate.ToShortDateString(),
                    absent = i.absent,
                    late = i.late,
                    leave = i.leave,
                    present = i.present,
                    underTime = i.underTime,
                    totalEmployees = employees
                }).FirstOrDefaultAsync();

            if(report == null)
            {
                _logger.LogInformation($"No extracted data for {date}");
                return null;
            }

            return report;
        }

        public async Task<month_attendance_report> getMonthAttendanceReportSummary(int month, int year)
        {
            var summary = await _reportDbContext.month_attendance_report
                .Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();

            if (summary == null)
            {
                _logger.LogInformation($"No extracted data");
                return null;
            }

            return summary;
        }

        public async Task<year_attendance_report> getYearAttendanceReportSummary(int year)
        {
            var summary = await _reportDbContext.year_attendance_report
                .Where(i => i.year == year).FirstOrDefaultAsync();

            return summary;
        }
            
        //BACKGROUND SERVICE QUERIES
        public async Task<month_attendance_report> getMonthAttendanceReport(int month, int year)
        {
            var monthAttendanceReport = await (
                from attendance in _reportDbContext.daily_attendance_report
                where attendance.reportDate.Month == month && attendance.reportDate.Year == year
                group attendance by 1 into x 
                select new month_attendance_report
                {
                    absent = x.Sum(i => i.absent),
                    late = x.Sum(i => i.late),
                    present = x.Sum(i => i.present),
                    leave_count = x.Sum(i => i.leave),
                    underTime = x.Sum(i => i.underTime),
                    last_modified_date = DateTime.Now,
                    report_date = DateTime.Now,
                    month = month,
                    year = year
                })
                .OrderBy(i => i.last_modified_date)
                .FirstOrDefaultAsync();

            if(monthAttendanceReport == null)
            {
                _logger.LogInformation($"No date extracted in {month}/{year}");
                return null;
            }

            return monthAttendanceReport;
            
        }

        public async Task<year_attendance_report> getYearAttendanceReport(int year)
        {
            var yearSummary = await (
                from report in _reportDbContext.month_attendance_report
                where report.year == year
                group report by 1 into x
                select new year_attendance_report
                {
                    present = x.Sum(i => i.present),
                    absent = x.Sum(i => i.absent), 
                    late = x.Sum(i => i.late),
                    leave_count = x.Sum(i => i.leave_count),
                    underTime = x.Sum(i => i.underTime),
                    year = year,
                    attendanceRate = (x.Sum(i => i.present) / 365) * 100,
                }).FirstOrDefaultAsync();

            if (yearSummary == null)
            {
                _logger.LogInformation($"No date extracted for {year}");
                return null;
            }

            yearSummary.average_present = yearSummary.present / 12;
            yearSummary.average_absent = yearSummary.absent / 12;
            yearSummary.average_late = yearSummary.late / 12;
            yearSummary.average_leave = yearSummary.leave_count / 12;
            yearSummary.average_undertime = yearSummary.underTime / 12;

            return yearSummary;
        }

        //EMPLOYEE PERFORMANCE BACKGROUND SERVICE QUERY
        public async Task<month_employees_performance_and_evaluation_report> getMonthEmployeePerformanceReport(int month, int year)
        {

                var report = await _reportDbContext.evaluation_records
                    .Where(i => i.evaluation_date.Month == month && i.evaluation_date.Year == year)
                    .ToListAsync();

                var summary = new month_employees_performance_and_evaluation_report()
                {
                    month = month,
                    year = year,
                    average_score = report.Sum(i => i.score) / report.Count,
                    total_evaluations = report.Count,
                    poor_performer_count = report.Where(i => i.score <= 3).Count(),
                };
                return summary;           
        }

        public async Task<month_employees_performance_and_evaluation_report> monthEmployeesPerformanceReport(int month, int year)
        {
            var monthReport = await _reportDbContext.month_employees_performance_and_evaluation_report
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

            return monthReport;
        }

        public async Task<List<monthPerformanceSummaryListResponse>> getMonthEmployeePerformanceSummarryList(int month, int year,int page,int size)
        {
            var monthPerformanceList = await (
                from eval in _reportDbContext.evaluation_records
                join emp in _reportDbContext.hr_employees
                on eval.employee_id equals emp.employee_id
                where eval.evaluation_date.Month == month && eval.evaluation_date.Year == year
                group new {eval,emp} by emp.employee_id into x
                select new monthPerformanceSummaryListResponse
                {
                    fullName = $"{x.Select(i => i.emp.first_name).FirstOrDefault()} {x.Select(i => i.emp.middle_name).FirstOrDefault()} {x.Select(i => i.emp.last_name).FirstOrDefault()}",
                    comments = x.Select(i => i.eval.comments).FirstOrDefault() ?? "",
                    evaluationDate = x.Select(i => i.eval.evaluation_date).FirstOrDefault(),
                    rating = x.Select(i => i.eval.rating).FirstOrDefault() ?? "",
                    score = x.Select(i => i.eval.score).FirstOrDefault()
                })
                .Skip((page - 1 ) * size)
                .Take(size)
                .OrderByDescending(i => i.score)
                .ToListAsync();

           return monthPerformanceList;
        }
    }
}

using APIResponses.Employee_Responses;
using APIResponses.forecast_results;
using APIResponses.Historical_report.Models;
using APIResponses.Training_Models;
using APIResponses.Training_Models_forecast;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;
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

        //RESPONSE QUERIES
        public async Task<yearSummaryAttendanceReportResponse> yearAttendanceSummary(int year)
        {
            var monthsReport = await _reportDbContext.month_attendance_report.Where(i => i.year == year).ToListAsync();

            var yearReport = await (
                from report in _reportDbContext.year_attendance_report
                where report.year == year
                group report by 1 into x
                select new yearSummaryAttendanceReportResponse
                {
                    attendanceRate = x.Select(i => i.attendanceRate).FirstOrDefault(),
                    late = x.Select(i => i.late).FirstOrDefault(),
                    present = x.Select(i => i.present).FirstOrDefault(),
                    underTime = x.Select(i => i.underTime).FirstOrDefault(),
                    year = x.Select(i => i.year).FirstOrDefault(),
                    monthsReport = monthsReport,
                }).FirstOrDefaultAsync();

            return yearReport;
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

        //FORECAST QUERIES
        public async Task<List<month_staffing_needs_forecast_training_data>> getMonthStaffingForecastNeeds(int month, int year)
        {
            var startDate = DateOnly.FromDateTime(new DateTime(year, month, 1));
            var endDate = startDate.AddMonths(1);
            var numberOfDaysInAMonth = DateTime.DaysInMonth(year,month);

            var dailyStats = await (
                from emp in _reportDbContext.hr_employees
                join attendance in _reportDbContext.hr_daily_attendance
                on emp.employee_id equals attendance.employee_id
                where attendance.attendance_date >= startDate && attendance.attendance_date < endDate
                group new {emp,attendance}by new {emp.department, attendance.attendance_date} into x
                select new
                {
                    department = x.Key.department,
                    avg_overtime_hours = x.Average(i => (decimal)i.attendance.overtime_minutes) / 60m,
                    avg_working_hours = x.Average(i => i.attendance.working_hours),
                    staffPresent = x.Select(i => i.emp.employee_id).Distinct().Count()
                }).ToListAsync();

            var report = dailyStats.GroupBy(i => i.department)
                .Select(i => new month_staffing_needs_forecast_training_data
                {
                    department = i.Key,
                    month = month,
                    year = year,

                    avg_working_hours = i.Average(i => i.avg_working_hours) ?? 0,
                    avg_overtime_hours = i.Average(i => i.avg_overtime_hours),
                    avg_staff_present = i.Average(i => (decimal)i.staffPresent)
                }).ToList();

            foreach(var item in report)
            {
                item.total_working_hours_needed = item.avg_staff_present * item.avg_working_hours * numberOfDaysInAMonth;
                item.total_staff_needed = item.total_working_hours_needed / (item.avg_working_hours * numberOfDaysInAMonth);
            }

            return report;
        }

        //FORECAST QUERIES
        public async Task<List<month_staffing_needs_forecast_result>> getMontStaffingNeedsForecast(int month, int year)
        {
            var report = await _reportDbContext.month_staffing_needs_forecast_result
                .Where(i => i.month == month && i.year == year).ToListAsync();

            return report;
        }

        //DOCTOR INFORMATION QUERIES
        public async Task<List<doctorDetailsAndEvaluationSummaryResponse>> getDoctorsInformation(int page,int size)
        {
            var doctor = await (
                from emp in _reportDbContext.hr_employees
                join eval in _reportDbContext.evaluation_records
                on emp.employee_id equals eval.employee_id into doctorDetails
                from eval in doctorDetails.DefaultIfEmpty()
                where emp.profession == "Doctor" && emp.status == "Active"
                group eval by emp into x
                select new doctorDetailsAndEvaluationSummaryResponse
                {
                    degreeType = x.Key.degree_type ?? "",
                    department = x.Key.department ?? "",
                    educationalStatus = x.Key.educational_status ?? "",
                    employmentType = x.Key.employment_type ?? "",
                    graduationYear = x.Key.graduation_year,
                    licenseExpiry = x.Key.license_expiry,
                    licenseIssued = x.Key.license_issued,
                    licenseNumber = x.Key.degree_type ?? "",
                    licenseType = x.Key.license_type ?? "",
                    medicalSchool = x.Key.medical_school ?? "",
                    name = $"{x.Key.first_name} {x.Key.middle_name} {x.Key.last_name}",
                    role = x.Key.role ?? "",
                    specialization = x.Key.specialization ?? "",

                    evaluation_records = x.Where(e => e != null)
                    .Select(i => new doctorSummaryEvaluationResponse
                    {
                        created_at = i.created_at.ToShortDateString(),
                        comments = i.comments,
                        rating = i.rating,
                        score = i.score
                    }).ToList()
                })
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return doctor;
        }

    }
}

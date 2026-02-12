
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class dailyAttendanceReportGeneratorService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ILogger<dailyAttendanceReportGeneratorService> _logger;

        public dailyAttendanceReportGeneratorService(IServiceScopeFactory serviceScope,ILogger<dailyAttendanceReportGeneratorService>logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {          
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    DateTime now = DateTime.Now;
                    DateTime nextMidnight = DateTime.Now.AddDays(1);
                    TimeSpan delay = now - nextMidnight;
                    await Task.Delay(delay,stoppingToken);

                    await dailyAttendanceReport(dbContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(20),stoppingToken);
                }
            }
        }

        //REPORT FUNCTION 
        //RUNS EVERY END OF DAY
        private async Task dailyAttendanceReport(ReportDbContext reportDb)
        {
            var prevDay = DateTime.Now.AddDays(-1);
            try
            {
                var attendance_report = await (
                    from attendance in reportDb.hr_daily_attendance
                    where attendance.attendance_date.Day == prevDay.Day
                    && attendance.attendance_date.Month == prevDay.Month
                    && attendance.attendance_date.Year == prevDay.Year
                    group new { attendance } by 1 into x 
                    select new
                    {
                        present = x.Where(i => i.attendance.status == "Present").Count(),
                        absent = x.Where(i => i.attendance.status == "Absent").Count(),
                        underTime = x.Where(i => i.attendance.status == "Undertime").Count(),
                        late = x.Where(i => i.attendance.status == "Late").Count(),
                        onLeave = x.Where(i => i.attendance.status == "On Leave").Count()
                    }).ToListAsync();

                var dailyReport = attendance_report.Select(i => new daily_attendance_report
                {
                    reportDate = prevDay,
                    absent = i.absent,
                    present = i.present,
                    underTime = i.underTime,
                    late = i.late,
                    leave = i.onLeave,
                    lastModifiedDate = DateTime.Now,
                }).ToList();

                await reportDb.daily_attendance_report.AddRangeAsync(dailyReport);
                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

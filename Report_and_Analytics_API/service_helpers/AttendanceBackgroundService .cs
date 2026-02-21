//using Microsoft.EntityFrameworkCore;
//using Report_and_Analytics_API.Data;

//namespace Report_and_Analytics_API.service_helpers
//{
//    public class AttendanceBackgroundService : BackgroundService
//    {
//        private readonly ILogger<AttendanceBackgroundService> _logger;
//        private readonly IServiceScopeFactory _serviceScopeFactory;

//        public AttendanceBackgroundService(
//            ILogger<AttendanceBackgroundService> logger,
//            IServiceScopeFactory factory)
//        {
//            _logger = logger;
//            _serviceScopeFactory = factory;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using var scope = _serviceScopeFactory.CreateScope();
//                    var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

//                    await ProcessDailyAttendance(db);

//                    // RUNS EVERY 2 HOURS
//                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError($"Attendance Service Error: {ex.Message}");
//                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//                }
//            }
//        }

//        private async Task ProcessDailyAttendance(ReportDbContext db)
//        {
//            _logger.LogInformation("Running Attendance Summary Update...");

//            // 1. GET LAST PROCESSED ID
//            int lastProcessedId = await db.daily_attendance_report
//                .Select(x => (int?)x.lastAttendanceIdProcessed)
//                .MaxAsync() ?? 0;

//            // 2. FETCH ONLY NEW ATTENDANCE ROWS
//            var newRows = await db.hr_daily_attendance
//                .Where(a => a.attendance_id > lastProcessedId)
//                .OrderBy(a => a.attendance_id)
//                .ToListAsync();

//            if (newRows.Count == 0)
//            {
//                _logger.LogInformation("No new attendance records found.");
//                return;
//            }

//            // 3. GROUP BY YYYY-MM-DD (DATE ONLY)
//            var groupedDaily = newRows
//                .GroupBy(a => a.attendance_date)   // ALREADY DATE ONLY
//                .Select(g => new
//                {
//                    Date = g.Key,
//                    Present = g.Count(x => x.status == "Present"
//                                        || x.status == "Late"
//                                        || x.status == "Overtime"),
//                    Leave = g.Count(x => x.status == "Off Duty"),
//                    Absent = g.Count(x => x.status == "Absent"),
//                    Late = g.Count(x => x.late_minutes > 0),
//                    UnderTime = g.Count(x => x.undertime_minutes > 0),
//                    MaxId = g.Max(x => x.attendance_id)
//                })
//                .ToList();

//            // 4. UPSERT DAILY SUMMARY
//            foreach (var d in groupedDaily)
//            {
//                var existing = await db.daily_attendance_report
//                        .FirstOrDefaultAsync(x => x.reportDate.Date == d.Date);

//                if (existing != null)
//                {
//                    existing.present = d.Present;
//                    existing.leave = d.Leave;
//                    existing.absent = d.Absent;
//                    existing.late = d.Late;
//                    existing.underTime = d.UnderTime;
//                    existing.lastAttendanceIdProcessed = d.MaxId;
//                    existing.lastModifiedDate = DateTime.UtcNow;

//                    db.daily_attendance_report.Update(existing);
//                }
//                else
//                {
//                    await db.daily_attendance_report.AddAsync(
//                        new APIResponses.Historical_report.Models.daily_attendance_report
//                        {
//                            reportDate = d.Date,
//                            present = d.Present,
//                            leave = d.Leave,
//                            absent = d.Absent,
//                            late = d.Late,
//                            underTime = d.UnderTime,
//                            lastAttendanceIdProcessed = d.MaxId,
//                            lastModifiedDate = DateTime.UtcNow
//                        });
//                }

//                await db.SaveChangesAsync();

//                // 5. UPDATE MONTH SUMMARY
//                await UpdateMonthAttendance(db, d);

//                // 6. UPDATE YEAR SUMMARY
//                await UpdateYearAttendance(db, d);
//            }

//            _logger.LogInformation("Attendance Summary Update Completed.");
//        }

//        // ----------------- MONTHLY --------------------
//        private async Task UpdateMonthAttendance(ReportDbContext db, dynamic d)
//        {
//            int month = d.Date.Month;
//            int year = d.Date.Year;

//            var monthRow = await db.month_attendance_report
//                .FirstOrDefaultAsync(x => x.month == month && x.year == year);

//            if (monthRow == null)
//            {
//                monthRow = new APIResponses.Historical_report.Models.month_attendance_report
//                {
//                    month = month,
//                    year = year,
//                    present = d.Present,
//                    leave_count = d.Leave,
//                    absent = d.Absent,
//                    late = d.Late,
//                    underTime = d.UnderTime,
//                    lastAttendanceIdProcessed = d.MaxId,
//                    last_modified_date = DateTime.UtcNow
//                };

//                await db.month_attendance_report.AddAsync(monthRow);
//            }
//            else
//            {
//                monthRow.present += d.Present;
//                monthRow.leave_count += d.Leave;
//                monthRow.absent += d.Absent;
//                monthRow.late += d.Late;
//                monthRow.underTime += d.UnderTime;
//                monthRow.lastAttendanceIdProcessed = d.MaxId;
//                monthRow.last_modified_date = DateTime.UtcNow;

//                db.month_attendance_report.Update(monthRow);
//            }

//            await db.SaveChangesAsync();
//        }

//        // ----------------- YEARLY --------------------
//        private async Task UpdateYearAttendance(ReportDbContext db, dynamic d)
//        {
//            int year = d.Date.Year;

//            var yearRow = await db.year_attendance_report
//                .FirstOrDefaultAsync(x => x.year == year);

//            if (yearRow == null)
//            {
//                yearRow = new APIResponses.Historical_report.Models.year_attendance_report
//                {
//                    year = year,
//                    present = d.Present,
//                    leave_count = d.Leave,
//                    absent = d.Absent,
//                    late = d.Late,
//                    underTime = d.UnderTime,
//                    lastAttendanceIdProcessed = d.MaxId,
//                };

//                await db.year_attendance_report.AddAsync(yearRow);
//            }
//            else
//            {
//                yearRow.present += d.Present;
//                yearRow.leave_count += d.Leave;
//                yearRow.absent += d.Absent;
//                yearRow.late += d.Late;
//                yearRow.underTime += d.UnderTime;
//                yearRow.lastAttendanceIdProcessed = d.MaxId;

//                db.year_attendance_report.Update(yearRow);
//            }

//            await db.SaveChangesAsync();
//        }
//    }
//}
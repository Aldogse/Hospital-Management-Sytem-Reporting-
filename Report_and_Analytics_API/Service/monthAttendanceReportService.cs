
using Microsoft.AspNetCore.Mvc;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class monthAttendanceReportService : BackgroundService
    {
        private readonly ILogger<monthAttendanceReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthAttendanceReportService(ILogger<monthAttendanceReportService>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var repo = scope.ServiceProvider.GetRequiredService<IemployeeRepository>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    if(DateTime.Now.Day > 1)
                    {
                        await MonthAttendanceReportGenerator(db,repo);
                        await Task.Delay(TimeSpan.FromDays(1));
                    }
                    await Task.Delay(TimeSpan.FromDays(1));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
        }

        //RUNS EVERY 5TH OF THE FOLLOWING MONTH
        private async Task MonthAttendanceReportGenerator(ReportDbContext reportDbContext,IemployeeRepository empRepo)
        {
            DateTime date = DateTime.Now.AddMonths(-1);

            try
            {
                var monthReport = await empRepo.getMonthAttendanceReport(date.Month,date.Year);

                if(monthReport == null)
                {
                    _logger.LogInformation($"No date extracted {date.Month}/{date.Year}");
                    return;
                }

                await reportDbContext.month_attendance_report.AddAsync(monthReport);
                await reportDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
        }
    }
}

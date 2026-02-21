
using Microsoft.AspNetCore.Mvc;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

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
                var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                DateTime date = DateTime.Now;

                while (!stoppingToken.IsCancellationRequested)
                {
                    if(DateTime.Now.Day >= 5)
                    {
                        if(!await jobRepo.hasRunThisMonth("MonthAttendanceReportGenerator",date.Month,date.Year))
                        {
                            await MonthAttendanceReportGenerator(db,repo);
                            await jobRepo.markAsRunThisMonth("MonthAttendanceReportGenerator",date.Month,date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month.");
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Job failed");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        
        //RUNS EVERY 5TH OF THE FOLLOWING MONTH
        private async Task MonthAttendanceReportGenerator(ReportDbContext reportDbContext,IemployeeRepository empRepo)
        {
            DateTime date = DateTime.Now.AddMonths(-1);
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;
                    var monthReport = await empRepo.getMonthAttendanceReport(date.Month, date.Year);

                    if (monthReport == null)
                    {
                        _logger.LogInformation(message: $"No date extracted {date.Month}/{date.Year}");
                        return;
                    }

                    await reportDbContext.month_attendance_report.AddAsync(monthReport);
                    await reportDbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Attempt {attempts} failed: {ex.Message}");

                    if (attempts >= maxAttempts)
                    {
                        _logger.LogInformation(message: "Maximum attempts has been reached");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));

                }
            }
        }
    }
}

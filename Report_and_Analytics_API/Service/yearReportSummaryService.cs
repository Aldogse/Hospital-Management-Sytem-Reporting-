
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearReportSummaryService: BackgroundService
    {
        private readonly ILogger<yearReportSummaryService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public yearReportSummaryService(ILogger<yearReportSummaryService>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceScope.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var repo  = scope.ServiceProvider.GetRequiredService<IemployeeRepository>();
                var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                int year = DateTime.Now.Year;

                //this should be AND NOT OR 
                if(DateTime.Now.Month == 1 && DateTime.Now.Day >= 5)
                {
                    if(!await jobRepo.hasRunThisYear("YearReportSummaryGenerator",year))
                    {
                        await YearReportSummaryGenerator(database,repo);
                        await jobRepo.markAsRunThisYear("YearReportSummaryGenerator",year);
                    }
                    else
                    {
                        _logger.LogInformation(message: "Service already run for the month");
                        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(message:$"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //RUNS EVERY 5TH OF JANUARY
        private async Task YearReportSummaryGenerator(ReportDbContext reportDb,IemployeeRepository empRepo)
        {
            int year = DateTime.Now.Year - 1;

            try
            {
                var summaryReport = await empRepo.getYearAttendanceReport(year);

                if (summaryReport == null)
                {
                    _logger.LogInformation($"No data extracted for {year}");
                    return;
                }

                await reportDb.year_attendance_report.AddAsync(summaryReport);
                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error: {ex.Message}");
                return;
            }
        }
    }
}

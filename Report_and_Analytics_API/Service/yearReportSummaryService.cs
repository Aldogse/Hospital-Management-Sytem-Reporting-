
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

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

                if (DateTime.Now.Day > 1)
                {
                    await YearReportSummaryGenerator(database,repo);
                    await Task.Delay(TimeSpan.FromDays(1));
                }
                await Task.Delay(TimeSpan.FromDays(1));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
        }

        //RUNS EVERY 5TH OF JANUARY
        private async Task YearReportSummaryGenerator(ReportDbContext reportDb,IemployeeRepository empRepo)
        {
            DateTime prevYear = DateTime.Now;
            try
            {
                var summaryReport = await empRepo.getYearAttendanceReport(prevYear.Year);

                if (summaryReport == null)
                {
                    _logger.LogInformation($"No data extracted for {prevYear.Year}");
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


using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class monthPerformanceReportService : BackgroundService
    {
        private readonly ILogger<monthPerformanceReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthPerformanceReportService(ILogger<monthPerformanceReportService>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IemployeeRepository>();

                    if(DateTime.Now.Day > 5)
                    {
                        await MonthPerformanceReportExtraction(database,repository);
                        await Task.Delay(TimeSpan.FromDays(1));
                    }
                    await Task.Delay(TimeSpan.FromDays(1));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        //RUNS EVERY TIME THE MONTH ENDS
        private async Task MonthPerformanceReportExtraction(ReportDbContext reportDb,IemployeeRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.Now.AddMonths(-1);
                var monthPerformanceReport = await repository.getMonthEmployeePerformanceReport(prevMonth.Month,prevMonth.Year);

                if(monthPerformanceReport == null)
                {
                    _logger.LogCritical($"Expecting data but nothing was reported for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                await reportDb.month_employees_performance_and_evaluation_report.AddAsync(monthPerformanceReport);
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

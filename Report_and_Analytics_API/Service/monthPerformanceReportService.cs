
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IemployeeRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 1)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthPayrollSummaryReport", date.Month, date.Year))
                        {
                            await MonthPerformanceReportExtraction(database, repository);
                            await jobRepo.markAsRunThisMonth("MonthPerformanceReportExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month");
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error starting the service: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }     
        }


        //RUNS EVERY TIME THE MONTH ENDS
        private async Task MonthPerformanceReportExtraction(ReportDbContext reportDb, IemployeeRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;
            while (attempts < maxAttempts)
            { 
                try
                {
                    DateTime prevMonth = DateTime.Now.AddMonths(-1);
                    var monthPerformanceReport = await repository.getMonthEmployeePerformanceReport(prevMonth.Month, prevMonth.Year);

                    if (monthPerformanceReport == null)
                    {
                        _logger.LogCritical($"Expecting data but nothing was reported for {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await reportDb.month_employees_performance_and_evaluation_report.AddAsync(monthPerformanceReport);
                    await reportDb.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempts {attempts}: {ex.Message}");

                    if(attempts == maxAttempts)
                    {
                        _logger.LogError("Maximum attempts has been reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }
    }
}

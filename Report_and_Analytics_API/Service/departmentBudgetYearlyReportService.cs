
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;
using Serilog;

namespace Report_and_Analytics_API.Service
{
    public class departmentBudgetYearlyReportService : BackgroundService
    {
        private readonly ILogger<departmentBudgetYearlyReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public departmentBudgetYearlyReportService(ILogger<departmentBudgetYearlyReportService>logger,IServiceScopeFactory serviceScope)
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
                    var repo = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    int year  = DateTime.Now.Year;

                    if(/*DateTime.Now.Month == 1 ||*/ DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisYear("DepartmentBudgetYearReportService",year))
                        {
                            await DepartmentBudgetYearReportService(database,repo);
                            await jobRepo.markAsRunThisYear("DepartmentBudgetYearReportService",year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month.");
                            await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Job failed: {ex.Message}");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }

        }
        //YEARLY EXTRACTION OF TOTAL BUDGET ACCUMULATED FOR THE PAST YEAR
        //WILL RUN EVERY 5TH OF JANUARY
        public async Task DepartmentBudgetYearReportService(ReportDbContext reportDb,IjournalRepository repository)
        {
            int prevYear = DateTime.Now.Year - 1;
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    var records = await repository.getYearBudgetReport(prevYear);

                    if (records == null)
                    {
                        _logger.LogWarning($"No budget extracted for {prevYear}");
                        return;
                    }

                    bool exist = await reportDb.department_budget_year_report.AnyAsync(i => i.year == prevYear);

                    if (!exist)
                    {
                        await reportDb.department_budget_year_report.AddAsync(records);
                        await reportDb.SaveChangesAsync();
                        _logger.LogInformation("Department budget records has been saved.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Attempt {attempts}: {ex.Message}");

                    if (attempts >= maxAttempts)
                    {
                        _logger.LogInformation(message: $"Maximum of {maxAttempts} has been reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
            
        }
    }
}

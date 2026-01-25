
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
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    int year  = DateTime.Now.Year;

                    if(DateTime.Now.Month == 1 && DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisYear("DepartmentBudgetYearReportService",year))
                        {
                            await DepartmentBudgetYearReportService(database);
                            await jobRepo.markAsRunThisYear("DepartmentBudgetYearReportService",year);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Job failed: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }

        }
        //YEARLY EXTRACTION OF TOTAL BUDGET ACCUMULATED FOR THE PAST YEAR
        //WILL RUN EVERY 5TH OF JANUARY
        public async Task DepartmentBudgetYearReportService(ReportDbContext reportDb)
        {
            int prevYear = DateTime.Now.Year - 1;
            try
            {
                var records = await (
                    from budget in reportDb.department_budgets
                    where budget.request_date.Year == 2025
                    group budget by budget.budget_id into x
                    select new department_budget_year_report
                    {
                        total_allocated = x.Sum(x => x.allocated_budget),
                        total_approved = x.Sum(x => x.approved_amount),
                        total_requested = x.Sum(x => x.requested_amount),
                        year = prevYear,
                        last_update_date = DateTime.Now,
                    }).FirstOrDefaultAsync();

                if(records == null)
                {
                    _logger.LogWarning($"No budget extracted for {prevYear}");
                    return;
                }

                await reportDb.department_budget_year_report.AddAsync(records);
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


using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthPayrollSummaryReportService : BackgroundService
    {
        private readonly ILogger<monthPayrollSummaryReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthPayrollSummaryReportService(ILogger<monthPayrollSummaryReportService>logger, IServiceScopeFactory serviceScope)
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
                var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                DateTime date = DateTime.Now;

                //this is should be equal to one to know the month already changes
                if (DateTime.UtcNow.Day >= 5)
                {
                    if (!await jobRepo.hasRunThisMonth("MonthPayrollSummaryReport", date.Month, date.Year))
                    {
                        await MonthPayrollSummaryReport(database);
                        await jobRepo.markAsRunThisMonth("MonthPayrollSummaryReport", date.Month, date.Year);
                    }
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error starting up the service,Details: {ex.Message}");
            }
        }

        //EXTRACT DATA EVERY 5TH OF THE FOLLOWING MONTH
        public async Task MonthPayrollSummaryReport(ReportDbContext reportDb)
        {
            var prevMonth = DateTime.Now.AddMonths(-1);
            try
            {
                var payrollReport = await reportDb.hr_payroll
                    .Where(i => i.pay_period_start.Month == prevMonth.Month)
                    .ToListAsync();

                if (payrollReport != null)
                {
                    var payrollSummary = new month_payroll_summary()
                    {
                        month = prevMonth.Month,
                        year = prevMonth.Year,
                        total_deductions = payrollReport.Sum(i => i.total_deductions),
                        total_gross_pay = payrollReport.Sum(i => i.gross_pay),
                        total_net_pay = payrollReport.Sum(i => i.net_pay),
                        total_employees = payrollReport.Count
                    };

                    await reportDb.month_payroll_summary.AddAsync(payrollSummary);
                    await reportDb.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"No report extracted for {prevMonth.Month}-{prevMonth.Year}");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
            }
        }
    }
}


using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{

    public class monthLeaveServiceReport : BackgroundService
    {
        private readonly ILogger<monthLeaveServiceReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthLeaveServiceReport(ILogger<monthLeaveServiceReport> logger, IServiceScopeFactory serviceScope)
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
                    using var scope =  _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 1)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthLeaveServiceReport",date.Month, date.Year))
                        {
                            
                                await MonthLeaveServiceReport(database);
                                await jobRepo.markAsRunThisMonth("MonthLeaveServiceReport", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:$"Job already run for the month");
                            return;
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task MonthLeaveServiceReport(ReportDbContext reportDb)
        {
            DateTime prevMonth = DateTime.Now.AddMonths(-2);
            try
            {
                var totalLeaveRequest = await reportDb.hr_leave
                    .Where(i => i.submit_at.Month == prevMonth.Month && i.submit_at.Year == prevMonth.Year)
                    .CountAsync();

                var pastMonthLeaveReport = await (
                    from leaves in reportDb.hr_leave
                    where leaves.submit_at.Month == prevMonth.Month
                    && leaves.submit_at.Year == prevMonth.Year
                    group new { leaves } by 1 into x 
                    select new month_leave_report
                    {                    
                        year = prevMonth.Year,
                        month = prevMonth.Month,
                        total_leave_request = totalLeaveRequest, 
                        month_approved_leaves = x.Where(i => i.leaves.leave_status == "Approved").Count(),
                        month_pending_leaves = x.Where(i => i.leaves.leave_status == "Pending").Count(),
                        month_rejected_leaves = x.Where(i => i.leaves.leave_status == "Rejected").Count()
                    }).FirstOrDefaultAsync();

                if(pastMonthLeaveReport == null)
                {
                    _logger.LogInformation($"No data extracted for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                await reportDb.month_leave_report.AddAsync(pastMonthLeaveReport);
                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error: {ex.Message}");
                return;
            }
        }
    }
}

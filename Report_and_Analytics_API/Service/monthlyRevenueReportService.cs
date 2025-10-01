
using System.Collections.Immutable;
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class monthlyRevenueReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ILogger<monthlyRevenueReportService> _logger;

        public monthlyRevenueReportService(IServiceScopeFactory serviceScope,ILogger<monthlyRevenueReportService>logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var reportDB = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

                    if(DateTime.Now.Day > 1)
                    {
                        _logger.LogInformation("Extraction starting.....");
                        await monthlyRevenueReportExtraction(reportDB,stoppingToken);
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task monthlyRevenueReportExtraction(ReportDbContext reportDb,CancellationToken stoppingToken)
        {
            var prevMonth = DateTime.Now;
            var transaction = await reportDb.Database.BeginTransactionAsync(stoppingToken);

            try
            {
                var firstDayOftheMonth = new DateTime(prevMonth.Year, prevMonth.Month, 1);
                var firstDayOfNextMonth = firstDayOftheMonth.AddMonths(1);

                //get all the descrptions
                var existingData = await reportDb.month_revenue_breakdownreport
                    .Where(i => i.year == prevMonth.Year && i.month == prevMonth.Month && i.isStored == true)
                    .Select(i => i.description)
                    .ToListAsync();

                //revenue data
                var revenueData = await (
                    from entry in reportDb.journal_entry
                    join line in reportDb.journal_entry_line
                    on entry.journal_entry_id equals line.journal_entry_id
                    join account in reportDb.journal_account
                    on line.account_id equals account.account_id
                    where entry.entry_date >= firstDayOftheMonth &&
                    entry.entry_date < firstDayOfNextMonth &&
                    account.account_type == "Revenue"
                    group new { line, account } by entry.description into x
                    select new
                    {
                        year = prevMonth.Year,
                        month = prevMonth.Month,
                        description = x.Key,
                        revenue = x.Sum(g => g.line.credit - g.line.debit)
                    }).ToListAsync(stoppingToken);

                var newRevenueData = revenueData.Where(i => !existingData.Contains(i.description))
                    .Select(i => new month_revenue_breakdownreport
                    {
                        year = i.year,
                        month = i.month,
                        description = i.description,
                        amount = i.revenue,
                        isStored = true
                    }).ToList();

                if (newRevenueData.Any())
                {
                    await reportDb.month_revenue_breakdownreport.AddRangeAsync(newRevenueData);
                    await reportDb.SaveChangesAsync();
                }       


                //total month revenue
                var totalMonthRevenues = await reportDb.month_revenue_breakdownreport
                    .Where(i => i.month == prevMonth.Month && i.year == prevMonth.Year)
                    .SumAsync(y => y.amount);

                //Insert data to month revenue report and check which quarter to insert 
                var monthRevenue = new monthly_revenue_report()
                {
                    month = prevMonth.Month,
                    quarter = (prevMonth.Month - 1) / 3 + 1,
                    year = prevMonth.Year,
                    month_revenue = totalMonthRevenues
                };

                await reportDb.monthly_revenue_report.AddAsync(monthRevenue);
                await reportDb.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
                
            }        
            catch(Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                throw new Exception(ex.Message);
            }

        }
    }
}

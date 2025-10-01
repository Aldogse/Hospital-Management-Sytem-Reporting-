
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class quarterlyRevenueReportService : BackgroundService
    {
        private readonly ILogger<quarterlyRevenueReportService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public quarterlyRevenueReportService(ILogger<quarterlyRevenueReportService>logger,IServiceScopeFactory serviceScope)
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
                    var reportDB = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

                    if(DateTime.Now.Day > 1)
                    {
                        _logger.LogInformation("Quarter data extraction starts......");
                        await quarterlyExtraction(reportDB, stoppingToken);
                        _logger.LogInformation("Quarter data extraction ended......");
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.LogInformation("Error extracting......");
                throw new Exception(ex.Message);
            }
        }

        //REVENUE EXTRACTION HAPPEN EVERY MONTH
        public async Task quarterlyExtraction(ReportDbContext reportDb,CancellationToken stoppingToken)
        {
            DateTime prevMonth = DateTime.Now;
            var month_quarter = (prevMonth.Month - 1) / 3 + 1;
            try
            {
                var monthToExtract = await reportDb.monthly_revenue_report
                    .Where(i => i.month == prevMonth.Month && i.year == prevMonth.Year)
                    .ToListAsync(stoppingToken);

                var exist = await reportDb.quarter_revenue.Where(i => i.quarter == month_quarter && i.year == prevMonth.Year)
                    .FirstOrDefaultAsync();

                if (exist == null)
                {
                    var quarterRevenue = new quarter_revenue()
                    {
                        year = prevMonth.Year,
                        totalRevenue = monthToExtract.Sum(i => i.month_revenue),
                        quarter = month_quarter
                    };
                    await reportDb.quarter_revenue.AddAsync(quarterRevenue);
                    await reportDb.SaveChangesAsync();
                }
                else
                {
                    exist.totalRevenue += monthToExtract.Sum(i => i.month_revenue);
                    reportDb.quarter_revenue.Update(exist);
                }

                await reportDb.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}

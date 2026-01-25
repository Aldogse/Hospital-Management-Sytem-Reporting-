
using APIResponses.Historical_report.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class monthInsuranceClaimReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<monthInsuranceClaimReportService> _logger;

        public monthInsuranceClaimReportService(IServiceScopeFactory serviceScopeFactory,ILogger<monthInsuranceClaimReportService>logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();

                await monthInsuranceCoverageReport(database);
                await Task.Delay(TimeSpan.FromDays(1));
            }
        }


        //INSURANCE CLAIM REPORT SERVICE
        // WILL RUN EVERY DAY THAT CHECK INFORMATION ON PREVIOUS MONTH TRANSACTIONS
        private async Task monthInsuranceCoverageReport(ReportDbContext reportDbContext)
        {
            DateTime prevMonth = DateTime.Now.AddMonths(-1);
            try
            {
                var existingReport = await reportDbContext.monthly_claim_report
                    .FirstOrDefaultAsync(i => i.year == prevMonth.Year && i.month == prevMonth.Month);

                var status = await reportDbContext.insurance_logs.Select(i => i.status).ToListAsync();

                if (existingReport == null)
                {
                    var newReport = new monthly_claim_report()
                    {
                        year = prevMonth.Year,
                        month = prevMonth.Month,
                        approveClaims = status.Where(i => i == "Approved").Count(),
                        declinedClaims = status.Where(i => i == "Declined").Count(),
                        pendingClaims = status.Where(i => i == "Pending").Count(),                    
                    };
                    await reportDbContext.monthly_claim_report.AddAsync(newReport);
                }
                else
                {
                    existingReport.approveClaims = status.Where(i => i == "Approved").Count();
                    existingReport.pendingClaims = status.Where(i => i == "Pending").Count();
                    existingReport.declinedClaims = status.Where(i => i == "Declined").Count();

                    reportDbContext.monthly_claim_report.Update(existingReport);
                }
                await reportDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }
    }
}

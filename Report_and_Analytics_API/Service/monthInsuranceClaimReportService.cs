
using APIResponses.Historical_report.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

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
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IinsuranceClaimRepository>();
                    var joblogsRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    var date = DateTime.UtcNow;

                    if (date.Day >= 5)
                    {
                        if (!await joblogsRepo.hasRunThisMonth("monthInsuranceCoverageReport", date.Month, date.Year))
                        {
                            await monthInsuranceCoverageReport(database, repository);
                            await joblogsRepo.markAsRunThisMonth("monthInsuranceCoverageReport", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation("Job already run for the month");
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                    }
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                   

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error:{ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
                 
            }
        }


        //INSURANCE CLAIM REPORT SERVICE
        // RUNS EVERY 5TH OF THE MONTH
        private async Task monthInsuranceCoverageReport(ReportDbContext reportDbContext,IinsuranceClaimRepository repository)
        {
            DateTime prevMonth = DateTime.Now.AddMonths(-1);
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    var report = await repository.getMonthClaimReport(prevMonth.Month, prevMonth.Year);

                    if (report == null)
                    {
                        _logger.LogInformation("Expecting data but nothing was extracted");
                        return;
                    }

                    await reportDbContext.monthly_claim_report.AddAsync(report);
                    await reportDbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");

                    if(attempts >= maxAttempts)
                    {
                        _logger.LogError(message:$"Maximum limit has been reached");
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}

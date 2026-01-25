
using APIResponses.Historical_report.Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;
using Serilog.Core;

namespace Report_and_Analytics_API.Service
{
    public class monthProviderClaimStatusDataExtraction : BackgroundService
    {
        private readonly ILogger<monthProviderClaimStatusDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthProviderClaimStatusDataExtraction(ILogger<monthProviderClaimStatusDataExtraction>logger,IServiceScopeFactory serviceScope)
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
                    var repo = scope.ServiceProvider.GetRequiredService<IinsuranceClaimRepository>();
                    var joblogsRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    var date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if(!await joblogsRepo.hasRunThisMonth("MonthProviderClaimStatusDataExtraction",date.Month,date.Year))
                        {
                            await MonthProviderClaimStatusDataExtraction(database,repo);
                            await joblogsRepo.markAsRunThisMonth("MonthProviderClaimStatusDataExtraction",date.Month,date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:$"job already run for the month.");
                            await Task.Delay(TimeSpan.FromMinutes(60),stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH OF THE MONTH
        private async Task MonthProviderClaimStatusDataExtraction(ReportDbContext reportDbContext,IinsuranceClaimRepository repository)
        {
            try
            {
                var prevMonth = DateTime.UtcNow.AddMonths(-1);
                var report = await repository.getProvidersClaimsHistoryStatus(prevMonth.Month,prevMonth.Year);

                if(report == null)
                {
                    _logger.LogInformation(message:$"Expecting data but nothing was extracted");
                    return;
                }

                await reportDbContext.month_insurance_claims_status_training_data.AddRangeAsync(report);
                await reportDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

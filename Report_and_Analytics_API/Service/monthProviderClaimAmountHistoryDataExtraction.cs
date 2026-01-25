
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthProviderClaimAmountHistoryDataExtraction : BackgroundService
    {
        private readonly ILogger<monthProviderClaimAmountHistoryDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthProviderClaimAmountHistoryDataExtraction(ILogger<monthProviderClaimAmountHistoryDataExtraction>logger,IServiceScopeFactory serviceScope)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IinsuranceClaimRepository>();
                    var joblogsRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if (!await joblogsRepository.hasRunThisMonth("MonthProviderClaimAmountHistoryDataExtraction",date.Month,date.Year))
                        {
                            await MonthProviderClaimAmountHistoryDataExtraction(database,repository);
                            await joblogsRepository.markAsRunThisMonth("MonthProviderClaimAmountHistoryDataExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:"Job already run for the month");
                            await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                            continue;
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(10),stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH OF THE MONTH
        private async Task MonthProviderClaimAmountHistoryDataExtraction(ReportDbContext reportDbContext,IinsuranceClaimRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                var report = await repository.getProvidersClaimHistoryAmount(prevMonth.Month,prevMonth.Year);

                if(report == null)
                {
                    _logger.LogInformation(message:$"Expecting data but nothing was extracted for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                await reportDbContext.month_insurance_claim_amount_training_data.AddRangeAsync(report);
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


using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Service
{
    public class dailyInsuranceSummaryReport : BackgroundService
    {
        private readonly ILogger<dailyInsuranceSummaryReport> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public dailyInsuranceSummaryReport(ILogger<dailyInsuranceSummaryReport>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {          
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IinsuranceClaimRepository>();

                    await DailyInsuranceTransaction(database,repo);
                    await Task.Delay(TimeSpan.FromDays(1));
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error: {ex.Message}");
            }
        }

        //RUNS EVERY TIME THE DAY ENDS
        private async Task DailyInsuranceTransaction(ReportDbContext reportDb,IinsuranceClaimRepository repository)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);
            try
            {
                var claimsSummary = await repository.getDailyTransactionsSummary(date);

                if(claimsSummary == null)
                {
                    _logger.LogError($"Expecting date but none was retrieved for {date}");
                    return;
                }

                await reportDb.daily_insurance_submitted_reports.AddAsync(claimsSummary);
                await reportDb.SaveChangesAsync();
                _logger.LogInformation($"Data successfully added for {date}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

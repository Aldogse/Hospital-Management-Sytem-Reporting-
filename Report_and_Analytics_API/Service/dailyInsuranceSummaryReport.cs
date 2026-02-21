
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


                    DateTime now = DateTime.Now;
                    DateTime nextMidnight = now.Date.AddDays(1);
                    TimeSpan delay = now - nextMidnight;                    

                    await DailyInsuranceTransaction(database,repo);
                    await Task.Delay(delay,stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        //RUNS EVERY TIME THE DAY ENDS
        private async Task DailyInsuranceTransaction(ReportDbContext reportDb,IinsuranceClaimRepository repository)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;
                    var claimsSummary = await repository.getDailyTransactionsSummary(date);

                    if (claimsSummary == null)
                    {
                        _logger.LogError($"Expecting date but none was retrieved for {date}");
                        return;
                    }

                    await reportDb.daily_insurance_submitted_reports.AddAsync(claimsSummary);
                    await reportDb.SaveChangesAsync();
                    _logger.LogInformation($"Data successfully added for {date}");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Attempt {attempts}:{ex.Message}");

                    if (attempts >= maxAttempts)
                    {
                        _logger.LogInformation(message: "Maximum attempts has been reached");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));

                }
            }
           
        }
    }
}

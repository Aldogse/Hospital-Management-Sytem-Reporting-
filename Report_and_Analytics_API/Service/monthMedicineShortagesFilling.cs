
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthMedicineShortagesFilling : BackgroundService
    {
        private readonly ILogger<monthMedicineShortagesFilling> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthMedicineShortagesFilling(ILogger<monthMedicineShortagesFilling>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }
        //THIS IS FOR REVIEW IF IT IS A NECESSSARY JOB
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repository = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var joblogsRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if (!await joblogsRepository.hasRunThisMonth("MonthMedicineShortagesFilling",date.Month,date.Year))
                        {
                            await MonthMedicineShortagesFilling(database,repository);
                            await joblogsRepository.markAsRunThisMonth("MonthMedicineShortagesFilling", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:$"Job already from for the month");
                            await Task.Delay(TimeSpan.FromHours(24),stoppingToken);
                        }
                    }
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        //RUNS EVERY 5TH OF THE MONTH
        private async Task MonthMedicineShortagesFilling(ReportDbContext reportDbContext,IjournalRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {

                try
                {
                    attempts++;

                    DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);
                    var experienceShortage = await repository.populateCorrectDataforTheSupplyTraining(prevMonth.Month, prevMonth.Year);

                    if (experienceShortage == null)
                    {
                        _logger.LogInformation(message: $"No medicine experience a shortage");
                        return;
                    }

                    reportDbContext.month_medicine_shortage_training_data.UpdateRange(experienceShortage);
                    await reportDbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(message: $"Error: {ex.Message}");                   

                    if(attempts >= maxAttempts)
                    {
                        _logger.LogError("Maximum attempts has been reached.");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}

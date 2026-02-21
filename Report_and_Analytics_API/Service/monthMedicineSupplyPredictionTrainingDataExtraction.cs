
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthMedicineSupplyPredictionTrainingDataExtraction : BackgroundService
    {
        private readonly ILogger<monthMedicineSupplyPredictionTrainingDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthMedicineSupplyPredictionTrainingDataExtraction(ILogger<monthMedicineSupplyPredictionTrainingDataExtraction>logger,
            IServiceScopeFactory serviceScope)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IjournalRepository>();
                    var joblogRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    if (date.Day >= 5)
                    {
                        if (!await joblogRepo.hasRunThisMonth("MonthMedicineSupplyPredictionTrainingDataExtraction", date.Month, date.Year))
                        {
                            await MonthMedicineSupplyPredictionTrainingDataExtraction(database, repository);
                            await joblogRepo.markAsRunThisMonth("MonthMedicineSupplyPredictionTrainingDataExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message: $"Job already run for the month");
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH of the following month
        private async Task MonthMedicineSupplyPredictionTrainingDataExtraction(ReportDbContext database,IjournalRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts < maxAttempts)
            {
                try
                {
                    attempts++;

                    //this should be 1 for prev month
                    DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                    var medicineReport = await repository.getMonthMedicineSupplyTrainingData(prevMonth.Month, prevMonth.Year);

                    if (medicineReport == null)
                    {
                        _logger.LogInformation(message: $"Expecting data but nothing was extracted for {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await database.month_medicine_shortage_training_data.AddRangeAsync(medicineReport);
                    await database.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");                  

                    if(attempts >= maxAttempts)
                    {
                        _logger.LogError("Maximum attempt has been reached");
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}

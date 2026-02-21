
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthStaffingNeedsDataExtraction: BackgroundService
    {
        private readonly ILogger<monthStaffingNeedsDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthStaffingNeedsDataExtraction(ILogger<monthStaffingNeedsDataExtraction>logger,IServiceScopeFactory serviceScope)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IemployeeRepository>();
                    var joblogsRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if (!await joblogsRepository.hasRunThisMonth("MonthStaffingNeedsDataExtraction",date.Month,date.Year))
                        {
                            await MonthStaffingNeedsDataExtraction(database,repository);
                            await joblogsRepository.markAsRunThisMonth("MonthStaffingNeedsDataExtraction",date.Month,date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:$"Job already run for the month");
                            await Task.Delay(TimeSpan.FromMinutes(10),stoppingToken);
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1),stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH OF THE MONTH
        private async Task MonthStaffingNeedsDataExtraction(ReportDbContext database,IemployeeRepository repository)
        {
            int att = 0;
            int maxAtt = 5;

            while (att < maxAtt)
            {
                try
                {
                    att++;

                    DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                    var report = await repository.getMonthStaffingForecastNeeds(prevMonth.Month, prevMonth.Year);

                    if (report == null)
                    {
                        _logger.LogInformation(message: $"Expecting data but nothing was extracted for {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await database.month_staffing_needs_forecast_training_data.AddRangeAsync(report);
                    await database.SaveChangesAsync();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");

                    if (att == maxAtt)
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

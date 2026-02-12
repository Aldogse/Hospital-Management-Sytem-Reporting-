
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthOutcomeReportDataExtractionService : BackgroundService
    {
        private readonly ILogger<monthOutcomeReportDataExtractionService> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthOutcomeReportDataExtractionService(ILogger<monthOutcomeReportDataExtractionService>logger,
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
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.UtcNow;

                    if(date.Day >= 5)
                    {
                        if(!await jobRepository.hasRunThisMonth("MonthOutcomeReportDataExtractionService",date.Month,date.Year))
                        {
                            await MonthOutcomeReportDataExtractionService(database,repository);
                            await jobRepository.markAsRunThisMonth("MonthOutcomeReportDataExtractionService",date.Month,date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:"Job already run to the month");
                            await Task.Delay(TimeSpan.FromMinutes(10),stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Error: {ex.Message},trying again after 10 mins");
                    await Task.Delay(TimeSpan.FromMinutes(10),stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH OF THE MONTH
        private async Task MonthOutcomeReportDataExtractionService(ReportDbContext dbContext,IjournalRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                var report = await repository.getMonthTreatmentOutcomeReport(10,2025);

                if(report == null)
                {
                    _logger.LogInformation(message:$"Expecting data but nothing was extracted");
                    return;
                }

                await dbContext.month_treatment_outcome_report.AddAsync(report);
                await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }
    }
}

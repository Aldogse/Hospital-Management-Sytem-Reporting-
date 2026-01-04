
using APIResponses.Historical_report.Models;
using APIResponses.Training_Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthCostManagementTrainingDataExtraction : BackgroundService
    {
        private readonly ILogger<monthCostManagementTrainingDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthCostManagementTrainingDataExtraction(ILogger<monthCostManagementTrainingDataExtraction>logger,IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

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

                    if(DateTime.UtcNow.Day >= 2)
                    {
                        if (!await joblogsRepository.hasRunThisMonth("MonthCostManagementTrainingDataExtraction", date.Month, date.Year)
                            && !await joblogsRepository.hasRunThisMonth("MonthPayrollSummaryReport", date.Month, date.Year))
                        {
                            await MonthCostManagementTrainingDataExtraction(database, repository);
                            await joblogsRepository.markAsRunThisMonth("MonthCostManagementTrainingDataExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogError(message: $"Either primary service did not run yet or service already run for the month");
                            return;
                        }                  
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }

        //RUNS AFTER MONTHPAYROLLSUMMARY SERVICE THE MONTHS ENDS
        private async Task MonthCostManagementTrainingDataExtraction(ReportDbContext reportDb,IjournalRepository repository)
        {
            try
            {
                //THIS IS to check the previous month report
                DateTime prevMonth = DateTime.UtcNow.AddMonths(-1);

                //this is to check the report 2 months ago
                DateTime prevMonthOfTargetDate = DateTime.UtcNow.AddMonths(-2);

                //this is to check the report 3 months ago
                DateTime lastThreeMonths = DateTime.UtcNow.AddMonths(-4);

                //this is to check the report 6 months ago
                DateTime lastSixMonths = DateTime.UtcNow.AddMonths(-7);

                var monthCostReport = new month_cost_management_and_training_data()
                {
                    month = prevMonth.Month,
                    year = prevMonthOfTargetDate.Year,
                    created_at = DateTime.UtcNow,
                    last_six_months_cost = await repository.getLastSixMonthsOperationalCost(lastSixMonths, prevMonthOfTargetDate),
                    last_three_months_cost = await repository.getLastThreeMonthsOperationalCost(lastThreeMonths,prevMonthOfTargetDate),
                    previous_month_operational_cost = await repository.getPreviousMonthOperationalCost(prevMonthOfTargetDate.Month, prevMonthOfTargetDate.Year),
                    total_month_operational_cost = await repository.getMonthOperationalCost(prevMonth.Month,prevMonth.Year),               
                };

                await reportDb.month_cost_management_training_data.AddAsync(monthCostReport);
                await reportDb.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }
    }
}

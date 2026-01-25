
using APIResponses.Historical_report.Models;
using APIResponses.Training_Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthRevenueTrainingDataExtraction : BackgroundService
    {
        private readonly ILogger<month_revenue_forecasting_training_data> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthRevenueTrainingDataExtraction(ILogger<month_revenue_forecasting_training_data>logger,IServiceScopeFactory serviceScope)
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
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    if (date.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisMonth("MonthRevenueTrainingDataExtraction", date.Month, date.Year))
                        {
                            await MonthRevenueTrainingDataExtraction(database, repository);
                            await jobRepo.markAsRunThisMonth("MonthRevenueTrainingDataExtraction", date.Month, date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:"Service already run for the month");
                            return;
                        }
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        //RUNS every 5th of the following month
        private async Task MonthRevenueTrainingDataExtraction(ReportDbContext database,IjournalRepository repository)
        {
            try
            {
                DateTime prevMonth = DateTime.Now.AddMonths(-1);

                var pharmacyData = await repository.getTrainingDataRevenueForecastPharmacy(prevMonth.Month,prevMonth.Year);
                var billRecsData = await repository.getTrainingDataBillRecordsForecast(prevMonth.Month,prevMonth.Year);

                if(pharmacyData == null && billRecsData == null)
                {
                    _logger.LogError(message:$"Expecting data but nothing extracted for {prevMonth.Month}/{prevMonth.Year}");
                    return;
                }

                var trainingData = new month_revenue_forecasting_training_data()
                {
                    month = prevMonth.Month,
                    year = prevMonth.Year,
                    total_revenue = pharmacyData?.totalSales + billRecsData.Sum(i => i.grand_total),
                    pharmacy_total_transactions = pharmacyData.totalTransactions,
                    average_bill_amount = billRecsData.Average(i => i.grand_total),
                    average_pharmacy_sale_per_transaction = (decimal)pharmacyData?.totalSales / pharmacyData.totalTransactions,
                    total_patient = billRecsData.Count,
                };

                await database.month_revenue_forecasting_training_data.AddAsync(trainingData);
                await database.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error: {ex.Message}");
                return;
            }
        }
    }
}

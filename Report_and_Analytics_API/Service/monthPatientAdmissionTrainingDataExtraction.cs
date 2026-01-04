
using APIResponses.Historical_report.Models;
using APIResponses.Training_Models;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class monthPatientAdmissionTrainingDataExtraction : BackgroundService
    {
        private readonly ILogger<monthPatientAdmissionTrainingDataExtraction> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public monthPatientAdmissionTrainingDataExtraction(ILogger<monthPatientAdmissionTrainingDataExtraction>logger,IServiceScopeFactory serviceScope)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IpatientAdmissionRepository>();
                    var jobLogsRepository = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    if(DateTime.UtcNow.Day >= 5) {
                        if (!await jobLogsRepository.hasRunThisMonth("MonthPatientAdmissionTrainingDataExtraction", date.Month, date.Year))
                        {
                            if (await jobLogsRepository.hasRunThisMonth("DischargeSummaryAndAdmissionReportService", date.Month, date.Year))
                            {
                                await MonthPatientAdmissionTrainingDataExtraction(database, repository);
                                await jobLogsRepository.markAsRunThisMonth("MonthPatientAdmissionTrainingDataExtraction", date.Month, date.Year);
                            }
                            else
                            {
                                _logger.LogInformation("Primary service did not initiate yet will check again afther 2 hours..");
                                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                            }
                            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        }
                    }
                    else
                    {
                        _logger.LogCritical(message:$"service already run this month {date.Month}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error:{ex.Message}");
                return;
            }
        }

        //RUNS EVERY TIME THE MONTH ENDS
        private async Task MonthPatientAdmissionTrainingDataExtraction(ReportDbContext reportDb,IpatientAdmissionRepository repository)
        {
            try
            {
                //the dates are related to target month that is why we put extract -1 to get the right values
                DateTime lastThreeMonths = DateTime.Now.AddMonths(-4);
                DateTime lastSixMonths = DateTime.Now.AddMonths(-7);

                //prev month of the prev date so for example if we extract for november this is for october
                DateTime prevMonthOfTheTargetDate = DateTime.Now.AddMonths(-2);


                DateTime preMonthOfTheExistingDate = DateTime.Now.AddMonths(-1);

                var lastThreeMonthsAdmissionCount = await repository.getLastThreeMonthsTotalAdmissions(lastThreeMonths, prevMonthOfTheTargetDate);
                var lastSixMonthsAdmissionCount = await repository.getLastSixMonthsTotalAdmissions(lastSixMonths, prevMonthOfTheTargetDate);
                var prevMonthTargetDateAdmissionCount = await repository.getPreviousMonthTotalAdmissions(prevMonthOfTheTargetDate.Month, preMonthOfTheExistingDate.Year);
                var totalAdmissionCount = await repository.getMonthTotalAdmissions(preMonthOfTheExistingDate.Month,preMonthOfTheExistingDate.Year);

                var trainingData = new month_patient_admission_forecasting_training_data()
                {
                    month = preMonthOfTheExistingDate.Month,
                    year = preMonthOfTheExistingDate.Year,
                    total_admission = totalAdmissionCount,
                    last_sixth_month_admission = lastSixMonthsAdmissionCount,
                    last_three_month_admission = lastThreeMonthsAdmissionCount,
                    prev_month_admission = prevMonthTargetDateAdmissionCount,
                };

                if (trainingData == null)
                {
                    _logger.LogCritical(message:$"Expecting data but nothing was extracted for {preMonthOfTheExistingDate.Month}/{preMonthOfTheExistingDate.Year}");
                    return;
                }

                await reportDb.month_patient_admission_forecasting_training_data.AddAsync(trainingData);
                await reportDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

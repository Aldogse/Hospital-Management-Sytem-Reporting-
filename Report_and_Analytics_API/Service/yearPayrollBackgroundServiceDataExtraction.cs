
using MimeKit.Cryptography;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class yearPayrollBackgroundServiceDataExtraction : BackgroundService
    {
        private readonly ILogger<yearPayrollBackgroundServiceDataExtraction> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public yearPayrollBackgroundServiceDataExtraction(ILogger<yearPayrollBackgroundServiceDataExtraction>logger,IServiceScopeFactory scope)
        {
            _logger = logger;
            _scopeFactory = scope;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var payrollRepo = scope.ServiceProvider.GetRequiredService<IhrPayrollRepository>();
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    var date = DateTime.Now;

                    if(date.Month == 1 && date.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisYear("YearPayrollBackgroundServiceDataExtraction",date.Year))
                        {
                            await YearPayrollBackgroundServiceDataExtraction(database, payrollRepo);
                            await jobRepo.markAsRunThisYear("YearPayrollBackgroundServiceDataExtraction",date.Year);
                        }
                        else
                        {
                            _logger.LogInformation(message:"Job already run for the year");
                            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                        }
                    }
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(10),stoppingToken);
                }
            }
        }

        //RUNS EVERY 5TH OF JANUARY
        private async Task YearPayrollBackgroundServiceDataExtraction(ReportDbContext reportDbContext, IhrPayrollRepository repository)
        {
            int attempts = 0;
            int maxAttempts = 5;

            while (attempts <= maxAttempts) 
            {
                try
                {
                    attempts++;

                    DateTime prevYear = DateTime.Now.AddYears(-1);

                    var yearReport = await repository.getYearHospitalPayrollReport(prevYear.Year);

                    if(yearReport == null)
                    {
                        _logger.LogInformation(message:$"Expecting data but nothing was extracted for {prevYear.Year}");
                        return;
                    }
                    await reportDbContext.year_hospital_payroll_report.AddAsync(yearReport);
                    await reportDbContext.SaveChangesAsync();
                    return;

                }
                catch (Exception ex)
                {
                    _logger.LogError(message:$"Attempt{attempts}" +
                        $" Error:{ex.Message}");

                    //retry after 2 seconds
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            _logger.LogInformation(message: "Maximum attempt has been met.");
            return;
        }
    }
}

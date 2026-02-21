
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_API.job_logs;

namespace Report_and_Analytics_API.Service
{
    public class patientAdmissionAndDischargeSummaryReportService : BackgroundService
    {
        private readonly IServiceScopeFactory _scope;
        private readonly ILogger<patientAdmissionAndDischargeSummaryReportService> _logger;

        public patientAdmissionAndDischargeSummaryReportService(IServiceScopeFactory scope,ILogger<patientAdmissionAndDischargeSummaryReportService>logger)
        {
            _scope = scope;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scope.CreateScope();
                    var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var repo = scope.ServiceProvider.GetRequiredService<IpropertyRepository>();

                    var jobRepo = scope.ServiceProvider.GetRequiredService<IjoblogsRepository>();
                    DateTime date = DateTime.Now;

                    //this is should be equal to one to know the month already changes
                    if (DateTime.Now.Day >= 5)
                    {
                        if (!await jobRepo.hasRunThisMonth("DischargeSummaryAndAdmissionReportService", date.Month, date.Year))
                        {
                            await DischargeSummaryAndAdmissionReportService(database, repo);
                            await jobRepo.markAsRunThisMonth("DischargeSummaryAndAdmissionReportService", date.Month, date.Year);
                        }
                    }
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(message: $"Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }

        //EXTRACT EVERY 5TH OF THE OF THE FOLLOWING MONTH
        private async Task DischargeSummaryAndAdmissionReportService(ReportDbContext reportDbContext, IpropertyRepository propertyRepo)
        {
            int att = 0;
            int maxAtt = 5;
            DateTime prevMonth = DateTime.Now.AddMonths(-1);

            while (att < maxAtt) 
            {
                try
                {
                    att++;
                    var report = await propertyRepo.getPreviousMonthAdmissionReport(prevMonth.Month, prevMonth.Year);

                    if (report == null)
                    {
                        _logger.LogError($"No Values extracted {prevMonth.Month}/{prevMonth.Year}");
                        return;
                    }

                    await reportDbContext.month_admission_and_discharge_report.AddAsync(report);
                    await reportDbContext.SaveChangesAsync();
                    return;
                }
                catch (InvalidOperationException ex)
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

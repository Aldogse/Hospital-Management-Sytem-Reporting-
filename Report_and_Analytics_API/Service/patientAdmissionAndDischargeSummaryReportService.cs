
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

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
            try
            {
                using var scope = _scope.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                var repo = scope.ServiceProvider.GetRequiredService<IpropertyRepository>();

                if(DateTime.Now.Day > 5)
                {
                    await DischargeSummaryAndAdmissionReportService(database,repo);
                    await Task.Delay(TimeSpan.FromDays(1));
                }
                await Task.Delay(TimeSpan.FromDays(1));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        //EXTRACT EVERY 5TH OF THE OF THE FOLLOWING MONTH
        private async Task DischargeSummaryAndAdmissionReportService(ReportDbContext reportDbContext, IpropertyRepository propertyRepo)
        {
            DateTime prevMonth = DateTime.Now.AddMonths(-1);
            try
            {
                var report = await propertyRepo.getPreviousMonthAdmissionReport(prevMonth.Month,prevMonth.Year);

                if (report == null)
                {
                    return;
                }

                await reportDbContext.month_admission_and_discharge_report.AddAsync(report);
                await reportDbContext.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return;
            }
        }
    }
}

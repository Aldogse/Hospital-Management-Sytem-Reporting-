
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.Service
{
    public class annualSalaryReport : BackgroundService
    {
        private readonly ILogger<annualSalaryReport> _logger;
        private readonly IServiceScope _serviceScope;
        public annualSalaryReport(ILogger<annualSalaryReport> logger, IServiceScope serviceScope)
        {
            _logger = logger;
            _serviceScope = serviceScope;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }


    }
}

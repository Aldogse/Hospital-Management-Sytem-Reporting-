
using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_API.Service
{
    public class quarterEmployeeAndPerformanceReportService : BackgroundService
    {
        private readonly ILogger<quarterEmployeeAndPerformanceReportService> _logger;

        public quarterEmployeeAndPerformanceReportService(ILogger<quarterEmployeeAndPerformanceReportService>logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        //THIS WILL RUN EVERY TIME THE CALENDAR YEAR ENDS 
        // WILL EXTRACT MONTH HOSPITAL EVALUATION 
        private async Task quarterEmployeeAndPerformanceService(ReportDbContext reportDbContext)
        {
            var transaction = reportDbContext.Database.BeginTransaction();
            var prevMonth = DateTime.Now.AddMonths(-1);
            string month = prevMonth.Month.ToString();
            try
            {
                //get all reports for the month of september 
                var monthEvaluation = await reportDbContext.evaluation_summary_reports
                    .Where(i => i.evaluation_period == month).ToListAsync();

                var report = new quarter_employees_performance_and_evaluation_report()
                {
                    totalEmployeesEvaluated = monthEvaluation.Count(),
                    month = month,
                    year = prevMonth.Year,
                    averagePerformanceScore = (monthEvaluation.Select(i => i.average_score).Sum()) / monthEvaluation.Count(),
                    lowPerformers = monthEvaluation.Where(i => i.average_score <= 3).Count(),                   
                };

                if (report == null)
                {
                    string message = $"No data extracted for {prevMonth.Month}/{prevMonth.Year}";
                    _logger.LogInformation(message);
                    return;
                }

                await reportDbContext.quarter_employees_performance_and_evaluation_report.AddAsync(report);
                await reportDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogInformation($"Error: {ex.Message} ");
            }
        }
    }
}

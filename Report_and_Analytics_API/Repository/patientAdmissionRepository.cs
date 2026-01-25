using APIResponses.forecast_results;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Repository
{
    public class patientAdmissionRepository : IpatientAdmissionRepository
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<patientAdmissionRepository> _logger;

        public patientAdmissionRepository(ReportDbContext reportDbContext,ILogger<patientAdmissionRepository>logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }


        //BACKGROUND SERVICE QUERIES
        public async Task<int> getLastSixMonthsTotalAdmissions(DateTime startDate, DateTime endDate)
        {
            var startKey = DateTime.Now.Year * 12 + startDate.Month;
            var endKey = DateTime.Now.Year * 12 + endDate.Month;

            var records = await _reportDbContext.month_admission_and_discharge_report
                .Where(i =>
                (i.year * 12 + i.month) >= startKey && (i.year * 12 + i.month) <= endKey)
                .SumAsync(i => i.occupied_beds);

            return records;
        }

        public async Task<int> getLastThreeMonthsTotalAdmissions(DateTime startDate, DateTime endDate)
        {
            var startKey = DateTime.Now.Year * 12 + startDate.Month;
            var endKey = DateTime.Now.Year * 12 + endDate.Month;


            var records = await _reportDbContext.month_admission_and_discharge_report
                .Where(i =>
                (i.year * 12 + i.month) >= startKey && (i.year * 12 + i.month) <= endKey)
                .SumAsync(i => i.occupied_beds);

            return records;
        }

        public async Task<int> getMonthTotalAdmissions(int month, int year)
        {
            return await _reportDbContext.month_admission_and_discharge_report.Where(i => i.month == month && i.year == year)
                .SumAsync(i => i.occupied_beds);
        }

        public async Task<int> getPreviousMonthTotalAdmissions(int month, int year)
        {
            return await _reportDbContext.month_admission_and_discharge_report.Where(i => i.month == month && i.year == year)
                .SumAsync(i => i.occupied_beds);
        }

        //FORECAST QUERY
        public async Task<month_patient_admission_forecast_result> getMonthPatientForecast(int month, int year)
        {
            var report = await _reportDbContext.month_patient_admission_forecast_result
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

            return report;
        }

        public async Task<List<object>> getPreviousMonthsPatientAdmission(int year)
        {
            var monthsReport = await _reportDbContext.month_patient_admission_forecasting_training_data.Where(i => i.year == year)
                .Select(i => new
                {
                    month = i.month,
                    year = i.year,
                    total_admission = i.total_admission,
                }).ToListAsync();

            return monthsReport.Cast<object>().ToList();
        }
    }
}

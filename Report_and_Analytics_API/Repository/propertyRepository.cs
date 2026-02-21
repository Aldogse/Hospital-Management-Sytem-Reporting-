using APIResponses.forecast_results;
using APIResponses.Historical_report.Models;
using APIResponses.PropertyAndManagementResponse;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_API.Repository
{
    public class propertyRepository : IpropertyRepository
    {
        private readonly ReportDbContext _reportDbContext;
        private readonly ILogger<propertyRepository> _logger;

        public propertyRepository(ReportDbContext reportDbContext, ILogger<propertyRepository>logger)
        {
            _reportDbContext = reportDbContext;
            _logger = logger;
        }

        //DB CALLS FOR MONTH AND PATIENT ADMISSION AND DISCHARGE REPORT SUMMARY
        public async Task<List<monthAdmissionReportResponse>> getAdmissionReport(int month, int year)
        {
                var monthAdmissions = await (
                    from beds in _reportDbContext.p_beds
                    join bedAssignments in _reportDbContext.p_bed_assignments
                    on beds.bed_id equals bedAssignments.bed_id
                    where bedAssignments.assigned_date.Month == month && bedAssignments.assigned_date.Year == year
                    group new { beds, bedAssignments } by beds.bed_id into x
                    select new monthAdmissionReportResponse
                    {
                        bedId = x.Select(x => x.beds.bed_id).FirstOrDefault(),
                        ward = x.Select(x => x.beds.ward).FirstOrDefault(),
                        bedType = x.Select(x => x.beds.bed_type).FirstOrDefault(),
                        roomNumber = x.Select(x => x.beds.room_number).FirstOrDefault(),
                        assignedDate = x.Select(x => x.bedAssignments.assigned_date).FirstOrDefault(),
                    }).ToListAsync();

                return monthAdmissions;
        }

        public async Task<daily_beds_utilization_report> getDailyBedsUtilizationReport(DateTime date)
        {
            var utilizationReport = await (
                from assignBeds in _reportDbContext.p_bed_assignments
                join beds in _reportDbContext.p_beds
                on assignBeds.bed_id equals beds.bed_id
                where assignBeds.assigned_date == date
                group new { assignBeds , beds } by 1 into x select new daily_beds_utilization_report
                {
                    bed_assigned = x.Where(i => i.assignBeds.assigned_date == date).Count(),
                    report_date = date,
                    bed_released = x.Where(i => i.assignBeds.released_date == date).Count(),
                    available_beds = x.Where(i => i.beds.status == "Available").Count(),
                    occupied_beds = x.Where(i => i.beds.status == "Occupied").Count()
                }).FirstOrDefaultAsync();

            return utilizationReport;
        }

        public async Task<List<monthDischargeReportResponse>>  getDischargeReport(int month, int year)
        {
                var startDate = new DateTime(year,month,1);
                var endDate = startDate.AddMonths(1);


                var monthDischarges = await(
                    from beds in _reportDbContext.p_beds
                    join bedAssignments in _reportDbContext.p_bed_assignments
                    on beds.bed_id equals bedAssignments.bed_id
                    where bedAssignments.released_date >= startDate && bedAssignments.released_date < endDate
                    group new { beds, bedAssignments } by beds.bed_id into x
                    select new monthDischargeReportResponse
                    {
                        bedId = x.Select(x => x.beds.bed_id).FirstOrDefault(),
                        ward = x.Select(x => x.beds.ward).FirstOrDefault(),
                        bedType = x.Select(x => x.beds.bed_type).FirstOrDefault(),
                        roomNumber = x.Select(x => x.beds.room_number).FirstOrDefault(),
                        releasedDate = x.Select(x => x.bedAssignments.released_date).FirstOrDefault(),
                    }).ToListAsync();

                return monthDischarges;
        }

        public async Task<month_admission_and_discharge_report> getMonthAdmissionAndDischargeReport(int month, int year)
        {
                var monthAdmissionReportSummary = await _reportDbContext.month_admission_and_discharge_report
                    .Where(i => i.year == year && i.month == month).FirstOrDefaultAsync();

                if(monthAdmissionReportSummary == null)
                {
                    _logger.LogWarning($"No data found for {month}/{year}");
                    return null;
                }
                return monthAdmissionReportSummary;      
        }


        //DATABASE CALLS FOR BACKGROUND SERVICE
        public async Task<month_admission_and_discharge_report> getPreviousMonthAdmissionReport(int month,int year)
        {
            var startDate = new DateTime(year,month,1);
            var endDate = startDate.AddMonths(1);

            var report = new month_admission_and_discharge_report();

            var monthDischargedReport = await _reportDbContext.p_bed_assignments
                .Where(i => i.released_date >= startDate && i.released_date < endDate).CountAsync();

            var totalBeds = await _reportDbContext.p_beds.CountAsync();

            var availableStatus = await _reportDbContext.p_beds
                .Where(i => i.status == "Available").CountAsync();

            var occupiedStatus = await _reportDbContext.p_beds
                .Where(i => i.status == "Occupied").CountAsync();

            var brokenBeds = await _reportDbContext.p_beds
                .Where(i => i.status == null || i.status == "")
                .CountAsync();

            report.recently_discharged = monthDischargedReport;
            report.total_beds = totalBeds;
            report.available_beds = availableStatus;
            report.occupied_beds = occupiedStatus;
            report.month = month;
            report.year = year;
            report.broken_beds = brokenBeds;

            return report;
        }

        public async Task<month_admission_and_discharge_report> getYearAdmissionsAndDischargeReport(int year)
        {
            var yearData = await (
                from rec in _reportDbContext.month_admission_and_discharge_report
                where  rec.year == year
                group new {rec} by 1 into x 
                select new month_admission_and_discharge_report
                {
                    year = year,
                    available_beds = x.Select(i => i.rec.available_beds).FirstOrDefault(),
                    broken_beds = x.Select(i => i.rec.broken_beds).FirstOrDefault(),
                    occupied_beds = x.Select(i => i.rec.occupied_beds).FirstOrDefault(),
                    total_beds = x.Select(i => i.rec.total_beds).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return yearData;
        }

        public async Task<List<month_admission_and_discharge_report>> monthBedsDistribution(int year)
        {
            var monthData = await _reportDbContext.month_admission_and_discharge_report.Where(i => i.year == year)
                .ToListAsync();

            return monthData;
        }

        public async Task<yearly_admission_and_discharge_report> yearlyAdmissionAndDischargeReport(int year)
        {
            var yearData = await _reportDbContext.yearly_admission_and_discharge_report.Where(i => i.year == year)
                .FirstOrDefaultAsync();

            return yearData;
        }

        public async Task<List<month_admission_and_discharge_report>> getMonthsAdmissionData(int year)
        {
            var monthData = await _reportDbContext.month_admission_and_discharge_report.Where(i => i.year == year)
                .ToListAsync();

            return monthData;
        }

        public async Task<month_bed_occupancy_forecast_result> monthForecastedBedOccupancyRate(int month, int year)
        {
            var monthForecast = await _reportDbContext.month_bed_occupancy_forecast_result
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

            return monthForecast;
        }

        public async Task<List<yearTotalOccupiedBedsResponse>> getYearAdmissionDataReport(int year)
        {

            var report = await _reportDbContext.month_admission_and_discharge_report.Where(i => i.year == year)
                .Select(i => new yearTotalOccupiedBedsResponse
                {
                    month = i.month,
                    year = i.year,
                    total_occupied_beds = i.occupied_beds
                })
                .ToListAsync();

            return report;
        }

        public async Task<int> numberOfBeds()
        {
            var num = await _reportDbContext.p_beds.ToListAsync();

            return num.Count;
        }
    }
}
  
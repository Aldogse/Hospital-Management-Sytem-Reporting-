using APIResponses.Historical_report.Models;
using APIResponses.PropertyAndManagementResponse;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

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

        public async Task<List<monthDischargeReportResponse>> getDischargeReport(int year, int month)
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

        public async Task<month_admission_and_discharge_report> getMonthAdmissionAndDischargeReport(int year, int month)
        {
                var monthAdmissionReportSummary = await _reportDbContext.month_admission_and_discharge_reports
                    .Where(i => i.year == year && i.month == month).FirstOrDefaultAsync();

                if(monthAdmissionReportSummary == null)
                {
                    _logger.LogWarning($"No data found for {month}/{year}");
                    return null;
                }
                return monthAdmissionReportSummary;      
        }


    }
}

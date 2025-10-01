using APIResponses.Historical_report.Models;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;

namespace Report_and_Analytics_API.Repository
{
    public class journalRepository : IjournalRepository
    {
        private readonly ReportDbContext _reportDb;

        public journalRepository(ReportDbContext reportDb)
        {
            _reportDb = reportDb;
        }


        //BREAKDOWN REPORTS DATA
        public async Task<List<month_revenue_breakdownreport>> getQuarterOneBreakdown(int year)
        {
            return await _reportDb.month_revenue_breakdownreport
                .Where(i => i.year == year && i.month == 1 && i.month <= 3)
                .ToListAsync();
        }

        public async Task<List<month_revenue_breakdownreport>> getQuarterTwoBreakdown(int year)
        {
            return await _reportDb.month_revenue_breakdownreport
                .Where(i => i.year == year && i.month >= 4 && i.month <= 6)
                .ToListAsync();
        }

        public async Task<List<month_revenue_breakdownreport>> getQuarterThreeBreakdown(int year)
        {
            return await _reportDb.month_revenue_breakdownreport
                .Where(i => i.year == year && i.month >= 7 && i.month <= 9)
                .ToListAsync();
        }

        public async Task<List<month_revenue_breakdownreport>> getQuarterFourBreakdown(int year)
        {
            return await _reportDb.month_revenue_breakdownreport
                .Where(i => i.year == year && i.month >= 10 && i.month <= 12)
                .ToListAsync();
        }



        //HOSPITAL REVENUE REPORT SUMMARY LAND PAGE DATA
        public async Task<decimal?> getQuarterRevenue(int year, int quarter)
        {
            var quarterlyRevenue = await _reportDb.quarter_revenue
                .Where(i => i.year == year && i.quarter == quarter)
                .ToListAsync();

            return quarterlyRevenue.Sum(i => i.totalRevenue);
        }

        public async Task<decimal?> getYearRevenue(int year)
        {
            var yearRecord = await _reportDb.quarter_revenue.Where(i => i.year == year)
                 .ToListAsync();

            return yearRecord.Sum(x => x.totalRevenue);
        }
    }
}

using APIResponses;
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

        //PHARMACY SALES REPORT QUERY
        public async Task<daily_pharmacy_sales> getDailyPharmacySalesReport(int month,int day,int year)
        {
            var start = new DateTime(year,month,day,0,0,0);
            var end = start.AddDays(1);

            var salesReport = await (
                from sales in _reportDb.pharmacy_sales
                where sales.sale_date >= start && sales.sale_date < end
                group sales by 1 into x
                select new daily_pharmacy_sales
                {
                     quantity_sold = x.Sum(i => i.quantity_sold),
                     total_amount = x.Sum(i => i.total_price),
                     sale_date = start

                }).FirstOrDefaultAsync();

            return salesReport;
        }

        public async Task<rangePharmacySalesReport> getRangePharmacySalesReport(DateTime start, DateTime end)
        {
            var startDate = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0);
            var endDate = new DateTime(end.Year, end.Month, end.Day, 0, 0, 0).AddDays(1);

            var salesReport = await (
                from sales in _reportDb.daily_pharmacy_sales
                where sales.sale_date >= start && sales.sale_date <= end
                group sales by 1 into x
                select new rangePharmacySalesReport
                {
                    total_amount = x.Sum(i => i.total_amount),
                    quantity_sold = x.Sum(i => i.quantity_sold)
                }).FirstOrDefaultAsync();

            return salesReport;
        }
    }
}

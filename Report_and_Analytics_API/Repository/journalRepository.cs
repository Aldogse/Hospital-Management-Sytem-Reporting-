using APIResponses;
using APIResponses.Historical_report.Models;
using APIResponses.journal_responses;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.Billing;

namespace Report_and_Analytics_API.Repository
{
    public class journalRepository : IjournalRepository
    {
        private readonly ReportDbContext _reportDb;
        private readonly ILogger<journalRepository> _logger;

        public journalRepository(ReportDbContext reportDb,ILogger<journalRepository>logger)
        {
            _reportDb = reportDb;
            _logger = logger;
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

        //BILLING SUMMARY QUERIES

        //BACKGROUND SERVICE QUERY
        public async Task<month_billing_report> getMonthBillingReport(int month, int year)
        {
            //GET how many is still pending for payments
            int? pending =  await _reportDb.billing_records
                .Where(i => i.status == "Pending" && i.billing_date.Month == month && i.billing_date.Year == year)
                .CountAsync();

            var pendingTransaction = await _reportDb.billing_records
             .Where(i => i.status == "Pending" && i.billing_date.Month == month && i.billing_date.Year == year)
             .ToListAsync();

            decimal? pendingAmount = 0;

            foreach (var item in pendingTransaction)
            {
                pendingAmount += item.grand_total;
            }


            decimal? total_billed = await _reportDb.billing_records
                .Where(i => i.billing_date.Month == month && i.billing_date.Year == year)
                .SumAsync(i => i.total_amount);
                
            //GET all records for paid bills
            var paidData = await (
                from records in _reportDb.billing_records
                where records.billing_date.Month == month && records.billing_date.Year == year
                && records.status == "Paid"
                group new {records} by 1 into x
                select new month_billing_report
                {
                    total_billed = total_billed,
                    total_insurance_covered = x.Sum(i => i.records.insurance_covered),
                    total_oop_collected = x.Sum(i => i.records.out_of_pocket),
                    total_paid = x.Sum(i => i.records.grand_total), 
                    month = month,
                    year = year,
                    total_pending_transaction  = pending,
                    total_pending_amount = pendingAmount
                }).FirstOrDefaultAsync();

            return paidData;
        }

        public async Task<daily_billing_report> getDailyBillingReport(DateOnly date)
        {
            DateTime dateChecker = date.ToDateTime(new TimeOnly(0,0));

                int? pending = _reportDb.billing_records
                    .Where(i => i.status == "Pending" && i.billing_date == dateChecker)
                    .Count();


            var pendingTransaction = await _reportDb.billing_records.Where(i => i.status == "Pending" && i.billing_date == dateChecker)
               .ToListAsync();

            decimal? pendingAmount = 0;

            foreach(var item in pendingTransaction)
            {
                pendingAmount += item.grand_total;
            }

            decimal? total_billed = await _reportDb.billing_records
            .Where(i => i.billing_date == dateChecker)
            .SumAsync(i => i.total_amount);

            var todayTransaction = await (
                    from records in _reportDb.billing_records
                    where records.billing_date == dateChecker && records.status == "Paid"
                    group new { records } by 1 into x
                    select new daily_billing_report
                    {
                        total_billed = total_billed,
                        total_insurance_covered = x.Sum(i => i.records.insurance_covered),
                        total_oop_collected = x.Sum(i => i.records.out_of_pocket),
                        total_paid = x.Sum(i => i.records.grand_total),
                        total_pending_transactions = pending,
                        report_date = date,
                        total_pending_amount = pendingAmount
                    }).FirstOrDefaultAsync();

             return todayTransaction;

        }

        //ENDPOINT QUERIES
        public async Task<month_billing_report> monthBillingReport(int month, int year)
        {
            var monthBillingReport = await _reportDb.month_billing_report
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

            return monthBillingReport;
        }

        public async Task<daily_billing_report> dailyBillingReport(DateOnly date)
        {
            var dailyReport = await _reportDb.daily_billing_report
                .Where(i => i.report_date == date).FirstOrDefaultAsync();

            return dailyReport;
        }

        public async Task<List<daily_billing_report>> monthBillingTransactionSummary(int month, int year,int page,int size)
        {
            var listOfTransactions = await _reportDb.daily_billing_report
                .Where(i => i.report_date.Month == month && i.report_date.Year == year)
                .Skip((page - 1) * size)
                .Take(size)
                .OrderBy(i => i.total_billed)
                .ToListAsync();

            return listOfTransactions;
        }
    }
}

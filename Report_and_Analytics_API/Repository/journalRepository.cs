using APIResponses;
using APIResponses.forecast_results;
using APIResponses.Historical_report.Models;
using APIResponses.journal_responses;
using APIResponses.prediction_results;
using APIResponses.Training_Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
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

        //REVENUE SUMMARY QUERIES

        public async Task<decimal?> getMonthBillRevenueReport(int month, int year)
        {
            var monthBillRevenueReport = await _reportDb.billing_records
                .Where(i => i.billing_date.Month == month && i.billing_date.Year == year)
                .SumAsync(i => i.grand_total);

            return monthBillRevenueReport;
        }

        public async Task<decimal?> getMonthPharmacyTotalSales(int month, int year)
        {
            var monthPharmacyRevenueReport = await _reportDb.month_pharmacy_sales
                .Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();

            return monthPharmacyRevenueReport?.totalSales;
        }

        public async Task<List<yearRevenueResponse>> getMonthsRevenueReport(int year)
        {

            var report = await _reportDb.month_revenue_report.Where(i => i.year == year).ToListAsync();    

            var response = report.Select(i => new yearRevenueResponse
            {
                year = i.year,
                month = i.month,
                total_revenue = i.total_revenue
            }).ToList();
                
            return response;
        }


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

        public async Task<yearly_pharmacy_sales_report> getYearPharmacySales(int year)
        {

            var topSellingItem = await _reportDb.month_pharmacy_sales.Where(i => i.year == year)
                 .GroupBy(i => i.topSellingItem)
                 .OrderByDescending(i => i.Count())
                 .Select(i => i.Key)
                 .FirstOrDefaultAsync();

            var yearSales = await (
                from sales in _reportDb.month_pharmacy_sales
                where sales.year == year
                group new { sales } by 1 into x
                select new yearly_pharmacy_sales_report
                {
                    totalSales = x.Sum(i => i.sales.totalSales),
                    totalTransactions = x.Sum(i => i.sales.totalTransactions),
                    year = year,
                    topSellingItem = topSellingItem
                }).FirstOrDefaultAsync();

            return yearSales;
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

        public async Task<yearly_billing_report> getYearBillingReport(int year)
        {
            var yearSummary = await (
                from rec in _reportDb.month_billing_report
                where rec.year == year
                group new {rec} by 1 into x
                select new yearly_billing_report
                {
                    year = year,
                    total_pending_amount = x.Sum(i => i.rec.total_pending_amount),
                    total_billed = x.Sum(i => i.rec.total_billed),
                    total_insurance_covered = x.Sum(i => i.rec.total_insurance_covered),
                    total_oop_collected = x.Sum(i => i.rec.total_oop_collected),
                    total_paid = x.Sum(i => i.rec.total_paid),
                    total_pending_transaction = x.Sum(i => i.rec.total_pending_transaction)
                }).FirstOrDefaultAsync();

            return yearSummary;
        }

        public async Task<yearly_billing_report> yearBillingReport(int year)
        {
            var yearReport = await _reportDb.yearly_billing_report.Where(i => i.year == year)
                .FirstOrDefaultAsync();

            return yearReport;
        }

        public async Task<List<month_billing_report>> monthsBillingReport(int year)
        {
            var monthReports = await _reportDb.month_billing_report.Where(i => i.year == year)
                .ToListAsync();

            return monthReports;
        }

        public async Task<yearly_pharmacy_sales_report> yearPharmacySales(int year)
        {
            var yearSaleSummary = await _reportDb.yearly_pharmacy_sales_report.Where(i => i.year == year)
                .FirstOrDefaultAsync();

            return yearSaleSummary;
        }

        public async Task<List<month_pharmacy_sales>> monthsPharmacySales(int year)
        {
            var monthsSales = await _reportDb.month_pharmacy_sales.Where(i => i.year == year).ToListAsync();
            return monthsSales;
        }

        public async Task<month_pharmacy_sales> monthPharmacySales(int month, int year)
        {
            var monthSales = await _reportDb.month_pharmacy_sales.Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();

            return monthSales;
        }

        public async Task<month_pharmacy_sales> getTrainingDataRevenueForecastPharmacy(int month, int year)
        {
            var pharmacData = await _reportDb.month_pharmacy_sales.Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();

            return pharmacData;
        }

        public async Task<List<billing_records>> getTrainingDataBillRecordsForecast(int month, int year)
        {
            var billRecs = await _reportDb.billing_records
                .Where(i => i.billing_date.Month == month && i.billing_date.Year == year && i.status == "Paid")
                .ToListAsync();

            return billRecs;
        }


        //COST MANAGEMENT QUERIES
        public async Task<float> getLastThreeMonthsOperationalCost(DateTime startDate, DateTime endDate)
        {
            int startKey =  12 + startDate.Month;
            int endKey =  12 + endDate.Month;

            var lastThreeMonths = (float)await _reportDb.month_operational_records_report.Where(i =>
            ( 12 + i.month) >= startKey && (12 + i.month) <= endKey)
                .SumAsync(i => i.total_operational_cost);


            return lastThreeMonths;
        }

        public async Task<float> getLastSixMonthsOperationalCost(DateTime startDate, DateTime endDate)
        {
            var startKey = 12 + startDate.Month;
            var endKey = 12 + endDate.Month;

            var lastSixMonthsReport = (float)await _reportDb.month_operational_records_report
                .Where(i =>
                (12 + i.month) >= startKey && (12 + i.month) <= endKey)
                .SumAsync(i => i.total_operational_cost);

            return lastSixMonthsReport;
        }

        public async Task<float> getPreviousMonthOperationalCost(int month, int year)
        {
            var totalCost = (float)await _reportDb.month_operational_records_report.Where(i => i.month == month && i.year == year)
               .Select(i => i.total_operational_cost).FirstOrDefaultAsync();

            return totalCost;
        }

        public async Task<float> getMonthOperationalCost(int month, int year)
        {
            var totalCost = (float)await _reportDb.month_operational_records_report.Where(i => i.month == month && i.year == year)
                .Select(i => i.total_operational_cost).FirstOrDefaultAsync();

            return totalCost;
        }

        public async Task<decimal?> getMonthTotalGrossPaid(int month, int year)
        {
            var monthReport = await _reportDb.month_payroll_summary.Where(i => i.month == month && i.year == year)
                .Select(i => i.total_gross_pay).FirstOrDefaultAsync();

            return monthReport;

        }

        public async Task<decimal> getMonthTotalReceiptRecorded(int month, int year)
        {
            var monthReport = await _reportDb.receipts.Where(i => i.created_at.Month == month && i.created_at.Year == year)
                .Select(i => i.total).SumAsync();

            return monthReport;
        }

        public async Task<decimal> getMonthTotalMedicineDisposedCost(int month, int year)
        {
            var monthReport = await _reportDb.disposed_medicines.Where(i => i.disposed_at.Month == month && i.disposed_at.Year == year)
                .Select(i => i.price).SumAsync();

            return monthReport;
        }

        //MEDICINE SHORTAGE QUERY 
        public async Task<List<month_medicine_shortage_training_data>> getMonthMedicineSupplyTrainingData(int month, int year)
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly next30Days = date.AddDays(30);
            int daysInMonth = DateTime.DaysInMonth(year,month);

            DateTime lastMonthStart = new DateTime(year,month,1);
            DateTime lastMonthEnd = lastMonthStart.AddMonths(1);

            var baseData = 
                from pharmacy_items in _reportDb.pharmacy_prescription_items
                join meds in _reportDb.pharmacy_inventory
                on pharmacy_items.med_id equals meds.med_id
                where pharmacy_items.dispensed_date >= lastMonthStart && pharmacy_items.dispensed_date < lastMonthEnd
                group new { pharmacy_items, meds } by meds.med_id into x
                select new
                {
                    med_id = x.Key,
                    total_dispensed_month = x.Sum(i => i.pharmacy_items.quantity_dispensed),
                    avg_daily_use = x.Sum(i => i.pharmacy_items.quantity_dispensed) / (decimal)daysInMonth,
                };

            var shortageReport = await (
                from stockBatches in _reportDb.pharmacy_stock_batches
                join item in _reportDb.pharmacy_inventory
                on stockBatches.med_id equals item.med_id
                where stockBatches.date_added >= lastMonthStart && stockBatches.date_added < lastMonthEnd
                && item.status == "Out of Stock"
                select item.med_id).Distinct().ToListAsync();

            var medicineReports = await(
                from baseItem in baseData
                join batches in _reportDb.pharmacy_stock_batches
                on baseItem.med_id equals batches.med_id into x
                select new month_medicine_shortage_training_data
                {
                    med_id = baseItem.med_id,
                    month = month,
                    year = year,

                    total_dispensed_month = baseItem.total_dispensed_month,
                    avg_daily_use = baseItem.avg_daily_use,

                    current_stock = x.Sum(i => i.stock_quantity),
                    expiring_within_30_days = x.Any
                    (i => i.expiry_date >= date && i.expiry_date <= next30Days),

                    shortage_occured = shortageReport.Contains(baseItem.med_id),
                }).ToListAsync();

           return medicineReports;
        }

        public async Task<List<month_medicine_shortage_training_data>> populateCorrectDataforTheSupplyTraining(int month, int year)
        {
            var shortMedicinesSupply = await (
                from stockBatches in _reportDb.pharmacy_stock_batches
                join item in _reportDb.pharmacy_inventory
                on stockBatches.med_id equals item.med_id
                where stockBatches.date_added.Month == month && stockBatches.date_added.Year == year
                && item.status == "Out of Stock"
                group new {stockBatches,item} by item.med_id into x
                select new month_medicine_shortage_training_data
                {
                    shortage_occured = true,
                }).ToListAsync();

            return shortMedicinesSupply;
        }

        //FORECAST RESULTS
        public async Task<month_cost_management_forecast_result> getMonthCostForecast(int month, int year)
        {
            var report = await _reportDb.month_cost_management_forecast_result
                .Where(i => i.month == month && i.year == year).FirstOrDefaultAsync();

            return report;
        }

        public async Task<month_revenue_forecast_result> getMonthRevenueForecast(int month, int year)
        {
            var forecast = await _reportDb.month_revenue_forecast_result.Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();

            return forecast;
        }

        public async Task<List<month_medicine_supply_forecast_result>> getMonthMedicineShortageForecast(int month, int year)
        {
            var forecast = await _reportDb.month_medicine_supply_forecast_result
                .Where(i => i.month == month && i.year == year && i.shortage_occured == true).ToListAsync();

            return forecast;
        }

        public async Task<List<object>> getMedicineMonthDispensed(int month,int year)
        {
            var forecast = await (
                from result in _reportDb.month_medicine_supply_forecast_result
                join training in _reportDb.month_medicine_shortage_training_data
                on result.med_id equals training.med_id
                where result.shortage_occured == true && 
                training.month == month && training.year == year
                group new {result ,training} by result.med_id into x
                select new 
                {
                    med_id = x.Key,
                    dispensed = x.Sum(i => i.training.total_dispensed_month)
                }).ToListAsync();

            return forecast.Cast<object>().ToList();
        }

        public async Task<List<monthsOperationalCostResponse>> getPreviousMonthOperationalCostReport(int year)
        {
            var prevMonthsData = await _reportDb.month_cost_management_training_data
                .Where(i => i.year == year)
                .Select(i => new monthsOperationalCostResponse
                {
                    month = i.month,
                    year = i.year,
                    total_month_operational_cost = i.total_month_operational_cost
                }).ToListAsync();

            return prevMonthsData;
        }

        public async Task<month_cost_management_forecast_result> getMonthForecastResult(int month, int year)
        {
            var forecast = await _reportDb.month_cost_management_forecast_result.Where(i => i.month == month &&
            i.year == year).FirstOrDefaultAsync();

            return forecast;
        }
    }
}
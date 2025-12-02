using APIResponses;
using APIResponses.Historical_report.Models;
using APIResponses.journal_responses;
using Report_and_Analytics_Library.Billing;

namespace Report_and_Analytics_API.Interface
{
    public interface IjournalRepository
    {
        public Task<decimal?> getYearRevenue(int year);
        public Task<decimal?> getQuarterRevenue(int year,int quarter);
        public Task<List<month_revenue_breakdownreport>> getQuarterOneBreakdown(int year);
        public Task<List<month_revenue_breakdownreport>> getQuarterTwoBreakdown(int year);
        public Task<List<month_revenue_breakdownreport>> getQuarterThreeBreakdown(int year);
        public Task<List<month_revenue_breakdownreport>> getQuarterFourBreakdown(int year);


        //PHARMACY SALES QUERY
        public Task <daily_pharmacy_sales> getDailyPharmacySalesReport(int month, int day, int year);
        public Task<rangePharmacySalesReport> getRangePharmacySalesReport(DateTime start,DateTime end);

        //BILLING SUMMARY QUERY
        public Task<month_billing_report> getMonthBillingReport(int month,int year);
        public Task<daily_billing_report> getDailyBillingReport(DateOnly date);

        public Task<month_billing_report> monthBillingReport(int month, int year);
        public Task<daily_billing_report> dailyBillingReport(DateOnly date);
        public Task<List<daily_billing_report>> monthBillingTransactionSummary(int month,int year,int page,int size);
    }
}
  
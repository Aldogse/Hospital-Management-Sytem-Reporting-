using APIResponses;
using APIResponses.Historical_report.Models;
using APIResponses.journal_responses;
using APIResponses.Training_Models;
using Report_and_Analytics_Library.Billing;

namespace Report_and_Analytics_API.Interface
{
    public interface IjournalRepository
    {

        //PHARMACY SALES QUERY
        public Task <daily_pharmacy_sales> getDailyPharmacySalesReport(int month, int day, int year);
        public Task<rangePharmacySalesReport> getRangePharmacySalesReport(DateTime start,DateTime end);
        public Task<yearly_pharmacy_sales_report> getYearPharmacySales(int year);
        public Task<yearly_pharmacy_sales_report> yearPharmacySales(int year);
        public Task<List<month_pharmacy_sales>> monthsPharmacySales(int year);
        public Task<month_pharmacy_sales> monthPharmacySales(int month,int year);

        //BILLING SUMMARY QUERY
        public Task<month_billing_report> getMonthBillingReport(int month,int year);
        public Task<daily_billing_report> getDailyBillingReport(DateOnly date);

        public Task<month_billing_report> monthBillingReport(int month, int year);
        public Task<daily_billing_report> dailyBillingReport(DateOnly date);
        public Task<List<daily_billing_report>> monthBillingTransactionSummary(int month,int year,int page,int size);
        public Task<yearly_billing_report> getYearBillingReport(int year);
        public Task<yearly_billing_report> yearBillingReport(int year);
        public Task<List<month_billing_report>> monthsBillingReport(int year);

        //REVENUE QUERIES
        public Task<decimal?> getMonthBillRevenueReport(int month,int year);
        public Task<decimal?> getMonthPharmacyTotalSales(int month,int year);
        public Task<month_pharmacy_sales> getTrainingDataRevenueForecastPharmacy(int month,int year);
        public Task<List<billing_records>> getTrainingDataBillRecordsForecast(int month,int year);

        //COST MANAGEMENT QEURIES

        //GET SPECIFIC MONTH OPERATIONAL COST
        public Task<decimal?> getLastThreeMonthsOperationalCost(DateTime startDate,DateTime endDate);
        public Task<decimal?> getLastSixMonthsOperationalCost(DateTime startDate, DateTime endDate);
        public Task<decimal?> getPreviousMonthOperationalCost(int month,int year);
        public Task<decimal?> getMonthOperationalCost(int month,int year);


        public Task<decimal?> getMonthTotalGrossPaid(int month, int year);
        public Task<decimal> getMonthTotalReceiptRecorded(int month,int year);
        public Task<decimal> getMonthTotalMedicineDisposedCost(int month,int year);


    }
}
  
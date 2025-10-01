using APIResponses.Historical_report.Models;

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
    }
}
  
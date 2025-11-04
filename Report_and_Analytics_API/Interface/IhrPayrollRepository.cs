using APIResponses.PayrollResponse;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<List<individualPayrollSummaryReport>> individualPayrollSummaryReports(int month, int year, int pageSize, int currentPage);
        Task<monthPayrollSummaryResponse> monthPayrollSummaryResponse(int month, int year);
        Task<List<yearSummaryPayrollResponse>> yearSummaryPayrollResponses(int year);
    }
}

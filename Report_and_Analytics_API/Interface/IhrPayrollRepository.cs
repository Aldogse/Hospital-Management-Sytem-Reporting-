using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<hr_payroll> getEmployeePayrollInformation(DateTime dateGenerated);
        Task<List<DateTime>> payrollDates();
    }
}

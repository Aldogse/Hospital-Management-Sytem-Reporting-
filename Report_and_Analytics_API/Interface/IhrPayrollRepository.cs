using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<hr_payroll> getEmployeePayrollInformation(DateTime dateGenerated);
        Task<List<DateTime>> payrollDates();
        Task<List<hr_payroll>> totalMonthSalaryPaid(int month);
        Task<List<hr_payroll>>totalDeductionToAbsence(int month);
    }
}

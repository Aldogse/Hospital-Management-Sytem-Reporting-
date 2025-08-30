using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<hr_payroll> getEmployeePayrollInformation(DateTime dateGenerated);
        Task<List<DateOnly>> payrollStatementDates(int employeeId);
        Task<List<decimal?>> totalMonthSalaryPaidAfterTaxes(int month);
        Task<List<decimal>>totalDeductionToAbsence(int month);

    }
}

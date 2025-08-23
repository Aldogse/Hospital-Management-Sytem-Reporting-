using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrPayrollRepository
    {
        Task<hr_payroll> getEmployeePayrollInformation(DateTime dateGenerated);
        Task<List<DateTime>> payrollStatementDates();
        Task<List<decimal?>> totalMonthSalaryPaidAfterTaxes(int month);
        Task<List<decimal>>totalDeductionToAbsence(int month);
        Task<decimal?> payCycleOvertimeHours(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleOvertimeHoursPaidAmount(int employeeId, DateOnly payStartDate);
    }
}

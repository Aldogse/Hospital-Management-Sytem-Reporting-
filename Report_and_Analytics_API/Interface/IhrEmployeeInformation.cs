using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrEmployeeInformation
    {
        //THIS SECTION IS FOR ANNUAL PAYROLL SUMMARY
        Task<hr_employees> getEmployeeInformation(int employeeId);
        Task<List<decimal?>> getMonthTotalHoursWorked(int employeeId,int month);
        Task<List<decimal>> getMonthOvertimeHours(int employeeId,int month);
        Task<List<decimal?>> getMonthTotalWage(int employeeId,int month);


        //THIS SECTION IS FOR PAYROLL STATEMENT
        //PAYCYCLE QUERIES
        Task<decimal?> payCycleOvertimeHours(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleOvertimeHoursPaidAmount(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleSSSDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCyclephilHealthDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCyclePagibigDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleLoanDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal> payCycleGrossPay(int employeeId,DateOnly payStartDate);
        Task<decimal?> payCycleTotalDeductions(int employeeId, DateOnly payStartDate);

        //YEAR TO DATE QUERIES
        Task<decimal?> yearToDateSSSDeductions(int employeeId, int year);
        Task<decimal?> yearToDatephilHealthDeductions(int employeeId, int year);
        Task<decimal?> yearToDatePagibigDeductions(int employeeId, int year);
        Task<decimal?> yearToDateLoanDeductions(int employeeId, int year);
        Task<decimal?> yearToDateGrossPay(int employeeId,int year);
        Task<decimal?> yearToDateTotalDeductions(int employeeId, int year);
    }
}

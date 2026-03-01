using APIResponses.Employee_Responses;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrEmployeeInformation
    {
        //THIS SECTION IS FOR ANNUAL PAYROLL SUMMARY

        //MONTH RECORDS
        Task<hr_employees> getEmployeeInformation(int employeeId);
        Task<decimal?> getMonthTotalHoursWorked(int employeeId,int month, int year);


        //YEAR RECORDS 

  //-----------------------------------------------------------------------------------------------//
        //THIS SECTION IS FOR PAYROLL STATEMENT
        //PAYCYCLE QUERIES
        Task<decimal?> payCycleOvertimeHours(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleOvertimeHoursPaidAmount(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleSSSDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCyclephilHealthDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCyclePagibigDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal> payCycleGrossPay(int employeeId,DateOnly payStartDate);
        Task<decimal?> payCycleTotalDeductions(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleNetPay(int employeeId, DateOnly payStartDate);
        Task<decimal?> payCycleAbsenceDeduction(int employeeId, DateOnly payStartDate);

        //YEAR TO DATE QUERIES
        Task<decimal?> yearToDateSSSDeductions(int employeeId, int year);
        Task<decimal?> yearToDatephilHealthDeductions(int employeeId, int year);
        Task<decimal?> yearToDatePagibigDeductions(int employeeId, int year);
        Task<decimal?> yearToDateGrossPay(int employeeId,int year);
        Task<decimal?> yearToDateTotalDeductions(int employeeId, int year);
        Task<decimal?> yearToDateNetPay(int employeeId, int year);
        Task<decimal?> yearToDateAbsenceDeduction(int employeeId, int year);
      
    }
}

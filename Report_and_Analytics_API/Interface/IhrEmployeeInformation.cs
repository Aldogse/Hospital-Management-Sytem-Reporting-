using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrEmployeeInformation
    {
        Task<hr_employees> getEmployeeInformation(int employeeId);
        Task<List<decimal?>> getMonthTotalHoursWorked(int employeeId,int month);
        Task<List<decimal>> getMonthOvertimeHours(int employeeId,int month);
        Task<List<decimal?>> getMonthTotalWage(int employeeId,int month);
    }
}

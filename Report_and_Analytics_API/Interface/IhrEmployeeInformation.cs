using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrEmployeeInformation
    {
        Task<hr_employees> getEmployeeInformation(int employeeId);
        Task<hr_employees> getTotalHoursWorked(int employeeId,int month);
    }
}

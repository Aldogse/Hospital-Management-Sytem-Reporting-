using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Repository
{
    public class hrEmployeeInformation : IhrEmployeeInformation
    {
        private readonly ReportDbContext _reportDbContext;
        public hrEmployeeInformation(ReportDbContext reportDbContext)
        {
            _reportDbContext = reportDbContext;
        }
        public async Task<hr_employees> getEmployeeInformation(int employeeId)
        {
            return await _reportDbContext.hr_employees.Where(i => i.employee_id == employeeId).FirstOrDefaultAsync();
        }
    }
}

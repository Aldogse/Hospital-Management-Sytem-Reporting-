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

        //THIS SECTION BELOW IS FOR ANNUAL PAYROLL SUMMARY REPORT QUERIES
        public async Task<List<decimal>> getMonthOvertimeHours(int employeeId, int month)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.employee_id == employeeId &&
                                                    i.pay_period_start.Month == month)
                                                    .Select(t => t.overtime_hours).ToListAsync();
        }

        public async Task<List<decimal?>> getMonthTotalHoursWorked(int employeeId, int month)
        {
            return await _reportDbContext.hr_daily_attendance.Include(i => i.hr_Employees).Where(i => i.employee_id == employeeId
                                                              && i.attendance_date.Month == month)
                                                             .Select(w => w.working_hours)
                                                             .ToListAsync();
        }

        public async Task<List<decimal?>> getMonthTotalWage(int employeeId, int month)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.employee_id == employeeId
                                                           && i.pay_period_start.Month == month).Select(i => i.net_pay)
                                                           .ToListAsync();
        }
    }
}

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


        //THIS SECTION BELOW IS QUERY FOR PAYROLL STATEMENT FORM       
        public async Task<decimal?> payCycleOvertimeHours(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.employee_id == employeeId
                                                                                 && i.pay_period_start == payStartDate)
                                                                                 .Select(i => i.overtime_hours)
                                                                                 .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleOvertimeHoursPaidAmount(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.employee_id == employeeId
                                                                                        && i.pay_period_start == payStartDate)
                                                                                 .Select(i => i.overtime_pay)
                                                                                 .FirstOrDefaultAsync();
        }

        //THIS SECTION IS FOR CURRENT PAYCYCLE PAYROLL STATEMENT FORM 
        public async Task<decimal?> payCycleSSSDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(t => t.sss_deduction)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCyclephilHealthDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(t => t.philhealth_deduction)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCyclePagibigDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(t => t.pagibig_deduction)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleLoanDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId&& i.pay_period_start == payStartDate)
                .Select(t => t.loan_deduction)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> payCycleGrossPay(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(i => i.gross_pay)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleTotalDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(i => i.total_deductions)
                .FirstOrDefaultAsync();
        }

        //THIS SECTION IS FOR YEAR TO DATE  FOR PAYROLL STATEMENT FORM
        public async Task<decimal?> yearToDateSSSDeductions(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.sss_deduction);
        }

        public async Task<decimal?> yearToDatephilHealthDeductions(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.philhealth_deduction);
        }

        public async Task<decimal?> yearToDatePagibigDeductions(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.pagibig_deduction);
        }

        public async Task<decimal?> yearToDateLoanDeductions(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.loan_deduction);
        }

        public async Task<decimal?> yearToDateGrossPay(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync (t => t.gross_pay);
        }

        public async Task<decimal?> yearToDateTotalDeductions(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.total_deductions);
        }
    }
}

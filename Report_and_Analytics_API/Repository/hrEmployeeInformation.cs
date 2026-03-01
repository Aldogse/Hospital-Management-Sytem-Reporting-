using APIResponses.Employee_Responses;
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

        public async Task<decimal?> getMonthTotalHoursWorked(int employeeId, int month, int year)
        {
            return await _reportDbContext.hr_daily_attendance.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId
                && i.attendance_date.Month == month && i.attendance_date.Year == year)
                .SumAsync(i => i.working_hours);
        }

        public async Task<decimal?> yearTotalHoursWorked(int employeeId, int year)
        {
            return await _reportDbContext.hr_daily_attendance.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.attendance_date.Year == year)
                .SumAsync(t => t.working_hours);
        }


        //THIS SECTION BELOW IS QUERY FOR PAYROLL STATEMENT FORM       
        public async Task<decimal?> payCycleOvertimeHours(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId
                && i.pay_period_start == payStartDate)
                .Select(i => i.overtime_hours)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleOvertimeHoursPaidAmount(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll
                .Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId
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


        public async Task<decimal?> payCycleTotalDeductions(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(i => i.total_deductions)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> payCycleGrossPay(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                .Select(i => i.gross_pay)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleNetPay(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                 .Where(i => i.employee_id == employeeId && i.pay_period_start == payStartDate)
                 .Select(i => i.net_pay)
                 .FirstOrDefaultAsync();
        }

        public async Task<decimal?> payCycleAbsenceDeduction(int employeeId, DateOnly payStartDate)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId & i.pay_period_start == payStartDate)
                .Select(t => t.absence_deduction)
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


        public async Task<decimal?> yearToDateNetPay(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                .Where(i => i.employee_id == employeeId && i.pay_period_start.Year == year)
                .SumAsync(t => t.net_pay);
        }

        public async Task<decimal?> yearToDateAbsenceDeduction(int employeeId, int year)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees)
                 .Where(i => i.employee_id == employeeId & i.pay_period_start.Year == year)
                 .SumAsync(t => t.absence_deduction);
        }

        public async Task<monthAttendanceReportRangeQueryResponse> monthAttendanceReportRangeQuery(int startmonth, int startyear, int endmonth, int endyear)
        {
            int startKey = startyear * 100 + startmonth;
            int endKey = endyear * 100 + endmonth;

            var data = await _reportDbContext.month_attendance_report.Where(i =>
            (i.year * 100 + i.month) >= startKey && (i.year * 100 + i.month) <= endKey)
                .OrderBy(i => i.year)
                .ToListAsync();

            return new monthAttendanceReportRangeQueryResponse
            {
                absent = data.Select(i => i.absent).Sum(),
                late = data.Select(i => i.late).Sum(),
                leave_count = data.Select(i => i.leave_count).Sum(),
                months = data,
                present = data.Select(i => i.present).Sum(),
                underTime = data.Select(i => i.underTime).Sum()
            };
        }
    }
}

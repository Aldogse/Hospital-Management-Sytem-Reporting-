using APIResponses.Historical_report.Models;
using APIResponses.PayrollResponse;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Repository
{
    public class hrPayrollRepository: IhrPayrollRepository
    {
        private readonly ReportDbContext _reportDbContext;
        public hrPayrollRepository(ReportDbContext reportDbContext)
        {
            _reportDbContext = reportDbContext; 
        }

        public async Task<month_payroll_summary> hospitalMonthPayrollReport(int month, int year)
        {
            var report = await _reportDbContext.month_payroll_summary.Where(i => i.month == month && i.year == year)
                 .FirstOrDefaultAsync();
            return report;
        }

        //GET ALL EMPLOYEE MONTH SALARY SUMMARY
        public async Task<List<individualPayrollSummaryReport>> individualPayrollSummaryReports(int month, int year, int pageSize, int currentPage)
        {
            var payrollSummaryList = await(
                        from payrollData in _reportDbContext.hr_payroll
                        join emp in _reportDbContext.hr_employees
                        on payrollData.employee_id equals emp.employee_id
                        where payrollData.pay_period_start.Month == month && payrollData.pay_period_start.Year == year
                        group new { payrollData, emp } by emp.employee_id into x
                        select new individualPayrollSummaryReport
                        {
                            employeeName = $"{x.Select(x => x.emp.first_name).FirstOrDefault()} {x.Select(x => x.emp.last_name).FirstOrDefault()}",
                            grossPay = x.Select(x => x.payrollData.gross_pay).FirstOrDefault(),
                            netPay = x.Select(x => x.payrollData.net_pay).FirstOrDefault(),
                            totalDeductions = x.Select(x => x.payrollData.total_deductions).FirstOrDefault(),
                        })
                        .Skip((currentPage - 1) * pageSize)
                        .Take(pageSize)
                        .OrderByDescending(i => i.netPay)
                        .ToListAsync();

            return payrollSummaryList;
        }

        public async Task<monthPayrollSummaryResponse> monthPayrollSummaryResponse(int month, int year)
        {
            var payrollReport = await (
                  from payrollData in _reportDbContext.month_payroll_summary
                  where payrollData.month == month && payrollData.year == year
                  group payrollData by 1 into x
                  select new monthPayrollSummaryResponse
                  {
                      totalDeductions = x.Select(i => i.total_deductions).FirstOrDefault(),
                      totalEmployees = x.Select(i => i.total_employees).FirstOrDefault(),
                      totalGrossPay = x.Select(i => i.total_gross_pay).FirstOrDefault(),
                      totalNetPay = x.Select(i => i.total_net_pay).FirstOrDefault(),
                  }).FirstOrDefaultAsync();

            return payrollReport;
        }

        public async Task<List<yearSummaryPayrollResponse>> yearSummaryPayrollResponses(int year)
        {
            var yearReportData = await _reportDbContext.month_payroll_summary
                .Where(i => i.year == year)
                .Select(i => new yearSummaryPayrollResponse
                {
                    month = i.month,
                    totalDeductions = i.total_deductions,
                    totalEmployees = i.total_employees,
                    totalNetPay = i.total_net_pay,
                }).ToListAsync();

            return yearReportData;
        }
    }
}

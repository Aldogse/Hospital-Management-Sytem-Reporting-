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

        public async Task<monthPayrollComparisonResponse> monthPayrollComparisonResult(int month, int year, int comparedMonth, int comparedYear)
        {
            var baseMonth = await _reportDbContext.month_payroll_summary.Where(i => i.month == month && i.year == year)
                .FirstOrDefaultAsync();
            var partnerMonth = await _reportDbContext.month_payroll_summary.Where(i => i.month == comparedMonth && i.year == comparedYear)
                .FirstOrDefaultAsync();

            return new monthPayrollComparisonResponse
            {
                baseMonth = month,
                baseYear = year,
                BaseTotalDeductions = baseMonth?.total_deductions,
                BaseTotalEmployees = baseMonth?.total_employees,
                BaseTotalGrossPay = baseMonth?.total_gross_pay,
                BaseTotalNetPay = baseMonth?.total_net_pay,

                comparedMonth = comparedMonth,
                comparedYear = comparedYear,
                comparedTotalDeductions = partnerMonth?.total_deductions,
                comparedTotalEmployees = partnerMonth?.total_employees,
                comparedTotalGrossPay = partnerMonth?.total_gross_pay,
                comparedTotalNetPay = partnerMonth?.total_net_pay
            };
        }

        public async Task<monthPayrollSummaryResponse> monthPayrollSummaryResponse(int month, int year)
        {
            var monthSummary = await _reportDbContext.month_payroll_summary.Where(i => i.month == month
            && i.year == year).FirstOrDefaultAsync();

            var yearRecords = await _reportDbContext.month_payroll_summary
                .Where(i => i.year == year)
                .OrderBy(i => i.month)
                .ToListAsync();

            return new monthPayrollSummaryResponse
            {
                totalNetPay = monthSummary?.total_net_pay,
                totalDeductions = monthSummary?.total_deductions,
                monthsRecords = yearRecords,
                totalEmployees = monthSummary?.total_employees,
                totalGrossPay = monthSummary?.total_gross_pay,
            };
        }


        public async Task<year_hospital_payroll_report> getYearHospitalPayrollReport(int year)
        {
            var monthsInfo = await _reportDbContext.month_payroll_summary.Where(i => i.year == year)
                .ToListAsync();

            return new year_hospital_payroll_report
            {
                year = year,
                total_employees = monthsInfo.Where(i => i.month == 12).Select(i => i.total_employees).FirstOrDefault(),
                year_total_deductions = monthsInfo.Sum(i => i.total_deductions),
                year_total_gross_pay = monthsInfo.Sum(i => i.total_gross_pay),
                year_total_net_pay = monthsInfo.Sum(i => i.total_net_pay)
            };
        }


        public async Task<yearSummaryPayrollResponse> yearHospitalPayrollSummary(int year)
        {
            var yearReport = await _reportDbContext.year_hospital_payroll_report.Where(i => i.year == year).FirstOrDefaultAsync();
            var monthsPayrollReport = await _reportDbContext.month_payroll_summary
                .Where(i => i.year == year)
                .OrderBy(i => i.month)
                .ToListAsync();

            return new yearSummaryPayrollResponse
            {
                total_employees = yearReport?.total_employees ?? 0,
                year = year,
                year_total_deductions = yearReport?.year_total_deductions ?? 0,
                year_total_gross_pay = yearReport?.year_total_gross_pay ?? 0,
                year_total_net_pay = yearReport?.year_total_net_pay ?? 0,
                monthsPayroll = monthsPayrollReport
            };
        }

        //NEW SERVICE QUERIES
        public async Task<monthPayrollQueryRangeResponse> monthPayrollRangeQueryAsync(int startmonth, int startyear, int endmonth, int endyear)
        {
            var startKey = startyear * 100 + startmonth;
            var endKey = endyear * 100 + endmonth;

            var data = await _reportDbContext.month_payroll_summary.Where(i =>
            (i.year * 100 + i.month) >= startKey
            && (i.year * 100 + i.month) <= endKey)
            .OrderBy(i => i.year)
            .ToListAsync();

            return new monthPayrollQueryRangeResponse
            {
                months = data,
                total_deductions = data.Select(i => i.total_deductions).Sum(),
                total_employees = data.Select(i => i.total_employees).Sum(),
                total_gross_pay = data.Select(i => i.total_gross_pay).Sum(),
                total_net_pay = data.Select(i => i.total_net_pay).Sum()
            };
        }
    }
}

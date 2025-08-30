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

        public async Task<hr_payroll> getEmployeePayrollInformation(DateTime dateGenerated)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.date_generated == dateGenerated).FirstOrDefaultAsync();
        }


        //THIS IS TO GET ALL THE AVAILABLE DATES ON PAYROLL THAT WILL BE USED TO GET CURRENT PAYROLL INFO
        //DURING THAT TIME
        public async Task<List<DateOnly>> payrollStatementDates(int employeeId)
        {
            return await _reportDbContext.hr_payroll.Where(i => i.employee_id == employeeId)
                .Select(t => t.pay_period_start)
                .Distinct()
                .OrderBy(d => d.Month)
                .ToListAsync();
        }


        //THIS QUERY IS TO GET THE LOSS OF EMPLOYEES DUE TO ABSENCES FOR THE MONTH
        public async Task<List<decimal>> totalDeductionToAbsence(int month)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.date_generated.Month == month
                                                           && i.absence_deduction > 0)
                                                           .Select(i => i.absence_deduction)
                                                           .ToListAsync();
        }

        //THIS IS TO CHECK HOW MUCH THE COMPANY PAID FOR SALARY AFTER TAXES ARE APPLIED
        public async Task<List<decimal?>> totalMonthSalaryPaidAfterTaxes(int month)
        {
            return await _reportDbContext.hr_payroll.Include(i => i.hr_Employees).Where(i => i.date_generated.Month == month
                                                           && i.net_pay > 0)
                                                           .Select(i => i.net_pay)
                                                           .ToListAsync();
        }


    }
}

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
        public async Task<List<DateTime>> payrollDates()
        {
            return await _reportDbContext.hr_payroll.Select(i => i.date_generated).ToListAsync();
        }

        public async Task<List<hr_payroll>> totalDeductionToAbsence(int month)
        {
            return await _reportDbContext.hr_payroll.Where(i => i.date_generated.Month == month
                                                           && i.absence_deduction > 0).ToListAsync();
        }

        public async Task<List<hr_payroll>> totalMonthSalaryPaid(int month)
        {
            return await _reportDbContext.hr_payroll.Where(i => i.date_generated.Month == month
                                                           && i.net_pay > 0).ToListAsync();
        }
    }
}

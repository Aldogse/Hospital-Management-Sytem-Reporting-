using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;
using Report_and_Analytics_API.Interface;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Repository
{
    public class hrLeaveRepository : IhrLeaveRepository
    {
        private readonly ReportDbContext _reportDbContext;
        public hrLeaveRepository(ReportDbContext reportDbContext)
        {
            _reportDbContext = reportDbContext;
        }
        public async Task<List<hr_leave>> getLeaves()
        {
            return await _reportDbContext.hr_leave.Include(i => i.hr_Employees).ToListAsync();
        }
    }
}

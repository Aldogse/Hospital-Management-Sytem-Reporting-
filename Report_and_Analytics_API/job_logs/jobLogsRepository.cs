
using System.Threading.Tasks;
using APIResponses.Historical_report.Run_Logs;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_API.Data;

namespace Report_and_Analytics_API.job_logs
{
    public class jobLogsRepository : IjoblogsRepository
    {
        private readonly ReportDbContext _reportDb;

        public jobLogsRepository(ReportDbContext reportDb)
        {
            _reportDb = reportDb;
        }

        //MONTH JOBS CHECKER
        public async Task<bool> hasRunThisMonth(string jobName, int month, int year)
        {
            var exist = await _reportDb.month_job_run_logs.AnyAsync(i => i.job_name == jobName
            && i.run_month == month && i.run_year == year);

            return exist;
        }


        public async Task markAsRunThisMonth(string jobName, int month, int runYear)
        {
            var job = new month_job_run_logs()
            {
                job_name = jobName,
                last_run = DateTime.Now,
                run_month = month,
                run_year = runYear
            };

            await _reportDb.month_job_run_logs.AddAsync(job);
            await _reportDb.SaveChangesAsync();
        }


        //YEAR JOBS CHECKER
        public async Task<bool> hasRunThisYear(string jobName, int runYear)
        {
            var exist = await _reportDb.job_run_logs.AnyAsync(i => i.job_name == jobName && i.run_year == runYear);

            return exist;
        }


        public async Task markAsRunThisYear(string jobName, int runYear)
        {
            var job = new job_run_logs()
            {
                job_name = jobName,
                last_run = DateTime.Now,
                run_year = runYear
            };

            await _reportDb.job_run_logs.AddAsync(job);
            await _reportDb.SaveChangesAsync();
        }


    }
}

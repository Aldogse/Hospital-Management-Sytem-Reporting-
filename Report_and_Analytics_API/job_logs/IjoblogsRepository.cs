using APIResponses.Historical_report.Run_Logs;

namespace Report_and_Analytics_API.job_logs
{
    public interface IjoblogsRepository
    {
        Task<bool> hasRunThisYear(string jobName,int runYear);
        Task markAsRunThisYear(string jobName,int year);
        Task<bool> hasRunThisMonth(string jobName,int month,int runYear);
        Task markAsRunThisMonth(string jobName, int month,int year);
    }
}

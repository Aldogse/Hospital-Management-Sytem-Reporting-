using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Interface
{
    public interface IhrLeaveRepository
    {
        Task<List<hr_leave>> getLeaves();
    }
}

using WASender.Models;

namespace WASender.Contracts.AdminSide
{
    public interface IAdminScheduleService
    {
        Task<(List<Schedulemessage> schedules, int total, int pending, int delivered)> GetSchedulesAsync(string search, string type);
        Task<bool> DeleteScheduleAsync(ulong id);
    }
}

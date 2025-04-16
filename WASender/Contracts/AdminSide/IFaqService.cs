using WASender.Models;

namespace WASender.Contracts.AdminSide
{
    public interface IFaqService
    {
        Task<List<Post>> GetAllFaqsAsync();
        Task<bool> AddFaqAsync(string question, string answer, string language, string position = "bottom");
        Task<(bool success, string message)> UpdateFaqAsync(ulong id, string question, string answer, string position, string language);
        Task<(bool success, string message)> DeleteFaqAsync(ulong id);
    }
}

using System.Threading.Tasks;

namespace WASender.Services
{
    public interface ILoginService
    {
        Task<bool> ValidateUserAsync(string email, string password);
        Task<string> GetUserRoleAsync(string email);
        Task<string> GenerateJwtTokenAsync(string email);
    }
}

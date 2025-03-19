using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public interface IRegisterService
    {
        Task<(string PlanTitle, ulong PlanId)> GetPlanDetails(ulong? id);
        Task<(bool Success, string ErrorMessage)> RegisterUserAsync(User model);
    }
}
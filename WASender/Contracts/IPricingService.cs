using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public interface IPricingService
    {
        Task<List<Plan>> GetActivePlansAsync();
        Task<List<object>> GetFaqsAsync();
        Task<List<object>> GetWhyChooseAsync();
    }
}
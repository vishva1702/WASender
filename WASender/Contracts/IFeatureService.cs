using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public interface IFeatureService
    {
        Task<List<object>> GetFeaturesAsync();
        Task<object> GetFeatureDetailsAsync(string slug);
        Task<List<object>> GetFaqsAsync();
        Task<List<object>> GetWhyChooseAsync();
    }
}

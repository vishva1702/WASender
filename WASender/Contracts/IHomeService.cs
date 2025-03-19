using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WASender.Models;

namespace WASender.Services
{
    public interface IHomeService
    {
        Task<JObject> GetHeroSectionAsync();
        Task<JObject> GetHeaderFooterAsync();
        Task<List<Category>> GetBrandsAsync();
        Task<List<Post>> GetTestimonialsAsync();
        Task<List<object>> GetFaqsAsync();
        Task<JObject> GetHomePageDataAsync();
        Task<List<object>> GetFeaturesAsync();
    }
}

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WASender.Models;

namespace WASender.Services
{
    public interface IGlobalDataService
    {
        Task LoadGlobalDataAsync(ViewDataDictionary viewData, ILogger logger);
        Task<List<Plan>> GetActivePlansAsync();
        Task<Dictionary<string, string>> WhyChooseAsync();
    }
}

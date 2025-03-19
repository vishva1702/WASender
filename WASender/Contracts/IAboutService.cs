using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WASender.Models;

namespace WASender.Services
{
    public interface IAboutService
    {
        Task LoadAboutDataAsync(ViewDataDictionary viewData);
        Task LoadCounterDataAsync(ViewDataDictionary viewData);
        Task LoadFaqDataAsync(ViewDataDictionary viewData);
        Task<List<Plan>> GetPlansAsync();
        Task<List<object>> GetFeaturesAsync();
        Task<List<object>> GetTeamMembersAsync();
        Task<object> GetTeamMemberDetailsAsync(string slug);
    }
}

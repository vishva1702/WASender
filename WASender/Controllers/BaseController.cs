using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IGlobalDataService _globalDataService;
        protected readonly ILogger<BaseController> _logger;

        public ILogger<PricingController> Logger { get; }

        public BaseController(IGlobalDataService globalDataService, ILogger<BaseController> logger)
        {
            _globalDataService = globalDataService;
            _logger = logger;
        }

        

        protected async Task LoadGlobalDataAsync()
        {
            await _globalDataService.LoadGlobalDataAsync(ViewData, _logger);
        }

        protected async Task<List<Plan>> GetActivePlansAsync()
        {
            return await _globalDataService.GetActivePlansAsync();
        }

        protected async Task<Dictionary<string, string>> WhyChooseAsync()
        {
            return await _globalDataService.WhyChooseAsync();
        }
    }
}

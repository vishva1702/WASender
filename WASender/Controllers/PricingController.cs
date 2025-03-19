using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Services;

namespace WASender.Controllers
{
    public class PricingController : BaseController
    {
        private readonly IPricingService _pricingService;

        public PricingController(IGlobalDataService globalDataService, ILogger<PricingController> logger, IPricingService pricingService)
            : base(globalDataService, logger)
        {
            _pricingService = pricingService;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            var faqs = await _pricingService.GetFaqsAsync();  // Fetch FAQs
            var plans = await _pricingService.GetActivePlansAsync(); // Fetch Plans
            var whyChoose = await _pricingService.GetWhyChooseAsync(); // Fetch WhyChoose

            ViewData["Faqs"] = faqs;
            ViewData["Plans"] = plans;
            ViewData["WhyChoose"] = whyChoose;

            return View(plans); // Pass plans as model
        }
    }
}

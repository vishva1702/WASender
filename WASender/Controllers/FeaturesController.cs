using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WASender.Services;

namespace WASender.Controllers
{
    public class FeaturesController : BaseController
    {
        private readonly IFeatureService _featureService;

        public FeaturesController(IGlobalDataService globalDataService, IFeatureService featureService, ILogger<FeaturesController> logger)
            : base(globalDataService, logger) 
        {
            _featureService = featureService;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            var whyChoose = await _featureService.GetWhyChooseAsync();
            ViewData["WhyChoose"] = whyChoose;

            var features = await _featureService.GetFeaturesAsync();
            ViewBag.Features = features;

            var faqs = await _featureService.GetFaqsAsync();
            ViewBag.Faqs = faqs;

            return View("List"); 
        }

        public async Task<IActionResult> Details(string slug)
        {
            await LoadGlobalDataAsync();

            var feature = await _featureService.GetFeatureDetailsAsync(slug);
            if (feature == null)
            {
                return NotFound();
            }

            ViewBag.Feature = feature;
            return View("Show"); 
        }

        public async Task<IActionResult> Faq()
        {
            await LoadGlobalDataAsync();

            var faqs = await _featureService.GetFaqsAsync();
            ViewBag.Faqs = faqs;

            return View();
        }
    }
}

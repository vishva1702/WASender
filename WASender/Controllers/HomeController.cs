using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WASender.Services;

namespace WASender.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHomeService _homeService;

        public HomeController(IGlobalDataService globalDataService, ILogger<HomeController> logger, IHomeService homeService)
            : base(globalDataService, logger)  // ✅ Fix: Pass `IGlobalDataService` instead of `ApplicationDbContext`
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                await LoadGlobalDataAsync();

                var heroData = await _homeService.GetHeroSectionAsync();
                var headerFooter = await _homeService.GetHeaderFooterAsync();
                var brands = await _homeService.GetBrandsAsync();
                var testimonials = await _homeService.GetTestimonialsAsync();
                var faqs = await _homeService.GetFaqsAsync();
                var homeData = await _homeService.GetHomePageDataAsync();
                var features = await _homeService.GetFeaturesAsync();

                // Pass data to ViewBag
                ViewBag.HeroTitle = heroData?["about"]?["title"]?.ToString();
                ViewBag.HeroDescription = heroData?["about"]?["description"]?.ToString();
                ViewBag.HeroImage = heroData?["hero_image"]?.ToString();
                ViewBag.AboutCover = heroData?["about_cover"]?.ToString();
                ViewBag.ActionAreaTitle = heroData?["action_area_title"]?.ToString();
                ViewBag.LeftButtonLink = headerFooter?["footer"]?["left_image_link"]?.ToString();
                ViewBag.RightButtonLink = headerFooter?["footer"]?["right_image_link"]?.ToString();
                ViewBag.LeftButtonImage = headerFooter?["footer_left_button_image"]?.ToString();
                ViewBag.RightButtonImage = headerFooter?["footer_button_image"]?.ToString();
                ViewBag.FeaturesArea = homeData?["features"]?["status"]?.ToString();
                ViewBag.BrandArea = homeData?["brand"]?["status"]?.ToString();
                ViewBag.AccountArea = homeData?["account_area"]?["status"]?.ToString();
                ViewBag.Heading = homeData?["heading"]?["title"]?.ToString()?.Replace("<strong>", "<span>").Replace("</strong>", "</span>");
                ViewBag.FeatureTitle = homeData?["features"]?["title"]?.ToString();

                string themePath = homeData?["theme_path"]?.ToString() ?? "Index";

                return View(themePath, new
                {
                    Brands = brands,
                    Testimonials = testimonials,
                    Faqs = faqs,
                    Home = homeData,
                    Features = features,
                    FeaturesArea = ViewBag.FeaturesArea,
                    BrandArea = ViewBag.BrandArea,
                    AccountArea = ViewBag.AccountArea,
                    Heading = ViewBag.Heading,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching data: {ex}");
                ViewData["ErrorMessage"] = "Error fetching data.";
                return View("Index");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WASender.Contracts.AdminSide;

namespace WASender.Controllers.AdminSide
{
    public class AdminSeoController : Controller
    {
        private readonly ISeoService _seoService;

        public AdminSeoController(ISeoService seoService)
        {
            _seoService = seoService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _seoService.GetSeoOptionsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Edit(ulong id)
        {
            var option = await _seoService.GetSeoOptionByIdAsync(id);
            if (option == null)
                return NotFound();

            var contents = await _seoService.GetSeoMetadataAsync(option);
            ViewBag.Id = id;
            return View(contents);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ulong id, IFormFile? image, [FromForm] Dictionary<string, string> metadata)
        {
            var resultMessage = await _seoService.UpdateSeoAsync(id, image, metadata);
            return Json(new { message = resultMessage });
        }
    }
}

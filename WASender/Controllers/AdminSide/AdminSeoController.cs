//using Microsoft.AspNetCore.Mvc;
//using WASender.Contracts.AdminSide;

//namespace WASender.Controllers.AdminSide
//{
//    public class AdminSeoController : Controller
//    {
//        private readonly ISeoService _seoService;

//        public AdminSeoController(ISeoService seoService)
//        {
//            _seoService = seoService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var posts = await _seoService.GetSeoOptionsAsync();
//            return View(posts);
//        }

//        public async Task<IActionResult> Edit(ulong id)
//        {
//            var option = await _seoService.GetSeoOptionByIdAsync(id);
//            if (option == null)
//                return NotFound();

//            var contents = await _seoService.GetSeoMetadataAsync(option);
//            ViewBag.Id = id;
//            return View(contents);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Update(ulong id, IFormFile? image, [FromForm] Dictionary<string, string> metadata)
//        {
//            var resultMessage = await _seoService.UpdateSeoAsync(id, image, metadata);
//            return Json(new { message = resultMessage });
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WASender.Contracts.AdminSide;
using WASender.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WASender.Controllers.AdminSide
{
    public class AdminSeoController : BaseController
    {
        private readonly ISeoService _seoService;

        public AdminSeoController(
            ISeoService seoService,
            IGlobalDataService globalDataService,
            ILogger<AdminSeoController> logger
        ) : base(globalDataService, logger)
        {
            _seoService = seoService;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            var posts = await _seoService.GetSeoOptionsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Edit(ulong id)
        {
            await LoadGlobalDataAsync();

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

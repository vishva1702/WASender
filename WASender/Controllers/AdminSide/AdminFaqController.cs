using Microsoft.AspNetCore.Mvc;
using WASender.Contracts.AdminSide;
using WASender.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WASender.Controllers.Admin
{
    [Route("AdminFaq")]
    public class FaqController : BaseController
    {
        private readonly IFaqService _faqService;

        public FaqController(
            IFaqService faqService,
            IGlobalDataService globalDataService,
            ILogger<FaqController> logger
        ) : base(globalDataService, logger)
        {
            _faqService = faqService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            var faqs = await _faqService.GetAllFaqsAsync();

            ViewBag.Languages = new Dictionary<string, string>
            {
                { "en", "English" },
            };

            return View("~/Views/AdminFaq/Index.cshtml", faqs);
        }

        [HttpPost("store")]
        public async Task<IActionResult> Store(string question, string answer, string language, string position = "bottom")
        {
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
                return BadRequest("Question and Answer are required.");

            var result = await _faqService.AddFaqAsync(question, answer, language, position);

            if (result)
                return RedirectToAction("Index");
            else
                return StatusCode(500, "An error occurred while storing the FAQ.");
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [FromForm] string question, [FromForm] string answer, [FromForm] string position, [FromForm] string language)
        {
            if (string.IsNullOrWhiteSpace(question) || question.Length > 150)
                return StatusCode(400, new { message = "Question is required and must be max 150 characters." });

            if (string.IsNullOrWhiteSpace(answer) || answer.Length > 500)
                return StatusCode(400, new { message = "Answer is required and must be max 500 characters." });

            var (success, message) = await _faqService.UpdateFaqAsync(id, question, answer, position, language);

            if (!success)
                return StatusCode(500, new { message });

            return Json(new
            {
                redirect = Url.Action("Index", "Faq", new { area = "admin" }),
                message
            });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(ulong id)
        {
            var (success, message) = await _faqService.DeleteFaqAsync(id);
            return Json(new { success, message });
        }
    }
}

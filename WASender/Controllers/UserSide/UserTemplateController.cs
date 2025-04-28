using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WASender.Contracts.UserSide;
using WASender.Services.UserSide;

namespace WASender.Controllers.UserSide
{
    [Route("UserTemplate")]
    public class UserTemplateController : Controller
    {
        private readonly IUserTemplateService _templateService;

        public UserTemplateController(IUserTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var templates = _templateService.GetUserTemplates(userId.Value, out var total, out var active, out var inactive, out var limit);

            ViewBag.TotalTemplates = total;
            ViewBag.ActiveTemplates = active;
            ViewBag.InactiveTemplates = inactive;
            ViewBag.Limit = limit;

            return View(templates);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(ulong id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Unauthorized action!";
                return RedirectToAction("Index");
            }

            var success = await _templateService.DeleteTemplateAsync(id, userId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Template removed successfully!" : "Template not found!";
            return RedirectToAction("Index");
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var (success, message) = await _templateService.CreateTemplateAsync(form, userId.Value);
            if (!success)
                return BadRequest(message);

            return Json(new { message = message });

        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(ulong id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var template = await _templateService.GetTemplateByIdAsync(id, userId.Value);
            if (template == null)
                return NotFound("Template not found.");

            string component = template.Type switch
            {
                "text-with-media" => "~/Views/UserTemplate/Partial/_Media.cshtml",
                "text-with-location" => "~/Views/UserTemplate/Partial/_Location.cshtml",
                "text-with-button" => "~/Views/UserTemplate/Partial/_Button.cshtml",
                "text-with-template" => "~/Views/UserTemplate/Partial/_Template.cshtml",
                "text-with-list" => "~/Views/UserTemplate/Partial/_List.cshtml",
                _ => "~/Views/UserTemplate/Partial/_Plaintext.cshtml",
            };

            ViewData["Component"] = component;
            return View(template);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(ulong id, IFormCollection form)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var (success, message) = await _templateService.UpdateTemplateAsync(id, form, userId.Value);

            if (!success)
                return BadRequest(message);

            return Json(new { message = message });
        }

        private ulong? GetUserIdFromClaims()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == "UserId" ||
                c.Type == "sub" ||
                c.Type == ClaimTypes.NameIdentifier
            );

            if (claim != null && ulong.TryParse(claim.Value, out var userId))
                return userId;

            return null;
        }
    }
}


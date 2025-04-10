using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WASender.Services.AdminSide;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WASender.Helpers;
using WASender.Contracts.AdminSide;

namespace WASender.Controllers.Admin
{
    [Authorize]
    public class DeveloperSettingsController : Controller
    {
        private readonly IEnvService _envService;

        public DeveloperSettingsController(IEnvService envService)
        {
            _envService = envService;
        }

        [HttpGet("/admin/developer-settings/{id}")]
        public IActionResult Show(string id)
        {
            switch (id)
            {
                case "app-settings":
                    ViewBag.Id = id;
                    ViewBag.TimeZones = TimeZoneInfo.GetSystemTimeZones();
                    ViewBag.Languages = _envService.GetOption("languages");
                    return View("App");

                case "mail-settings":
                    ViewBag.Id = id;
                    ViewBag.MailDriver = _envService.GetMailDriver();
                    return View("Smtp");

                case "storage-settings":
                    ViewBag.Id = id;
                    return View("Storage");

                case "wa-settings":
                    ViewBag.Id = id;
                    return View("Whatsapp");

                default:
                    return NotFound();
            }
        }

        [HttpPost("/admin/developer-settings/{id}")]
        public IActionResult Update(string id, IFormCollection request)
        {
            switch (id)
            {
                case "app-settings":
                    _envService.EditEnv("APP_NAME", Slugify(request["name"]));
                    _envService.EditEnv("APP_DEBUG", request["app_debug"] == "true" ? "true" : "false");
                    _envService.EditEnv("TIME_ZONE", request["timezone"]);
                    _envService.EditEnv("DEFAULT_LANG", request["default_lang"].FirstOrDefault() ?? "en");
                    return Json(new { message = "Global Settings Updated..." });

                case "mail-settings":
                    // You can continue mapping like your Laravel method
                    break;

                case "storage-settings":
                    // Add storage settings logic
                    break;

                case "wa-settings":
                    // Add WhatsApp settings logic
                    break;

                default:
                    return NotFound();
            }

            return Json(new { message = "Settings Updated." });
        }

        private string Slugify(string input)
        {
            return input.ToLower().Replace(" ", "-");
        }
    }

}

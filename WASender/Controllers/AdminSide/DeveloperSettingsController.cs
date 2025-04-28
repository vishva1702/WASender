using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace WASender.Controllers.Admin
{
    
    public class DeveloperSettingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public DeveloperSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: Admin/DeveloperSettings/Show/id
        public IActionResult Show(string id)
        {
            switch (id)
            {
                case "app-settings":
                    var tzlist = TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToList();
                    var languages = GetOption("languages"); // Use your existing method to get from DB
                    ViewBag.Id = id;
                    ViewBag.Tzlist = tzlist;
                    ViewBag.Languages = languages;
                    return View("App");

                case "mail-settings":
                    var mailDriver = _configuration["MAIL_DRIVER_TYPE"] == "MAIL_DRIVER"
                        ? _configuration["MAIL_DRIVER"]
                        : _configuration["MAIL_MAILER"];
                    ViewBag.Id = id;
                    ViewBag.MailDriver = mailDriver;
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

        // POST: Admin/DeveloperSettings/Update/id
        [HttpPost]
        public IActionResult Update(string id, IFormCollection form)
        {
            switch (id)
            {
                case "app-settings":
                    EditEnv("APP_NAME", Slugify(form["name"]));
                    EditEnv("APP_DEBUG", form["app_debug"] == "true" ? "true" : "false");
                    EditEnv("TIME_ZONE", form["timezone"]);
                    EditEnv("DEFAULT_LANG", string.IsNullOrEmpty(form["default_lang"]) ? "en" : form["default_lang"]);
                    return Json(new { message = "Global Settings Updated..." });

                case "wa-settings":
                    var waServerUrl = form["wa_server_url"].ToString().Replace(" ", "");
                    EditEnv("WA_SERVER_URL", waServerUrl);
                    EditEnv("WA_SERVER_HOST", form["host"]);
                    EditEnv("WA_SERVER_PORT", form["port"]);
                    // Add more fields as per your original code...
                    return Json(new { message = "WhatsApp Settings Updated..." });

                default:
                    return NotFound();
            }
        }

        // Helper method to update .env values
        private void EditEnv(string key, string value)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!System.IO.File.Exists(filePath)) return;

            var lines = System.IO.File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(key + "="))
                {
                    lines[i] = $"{key}={value}";
                    break;
                }
            }
            System.IO.File.WriteAllLines(filePath, lines);
        }

        private string Slugify(string value)
        {
            return value?.ToLower().Replace(" ", "-");
        }

        private List<string> GetOption(string key)
        {
            // ✅ You have existing models — fetch from DB
            // Example:
            // return _dbContext.Options.Where(x => x.Key == key).Select(x => x.Value).ToList();
            return new List<string> { "en", "fr", "de" }; // Mocked for now
        }
    }
}

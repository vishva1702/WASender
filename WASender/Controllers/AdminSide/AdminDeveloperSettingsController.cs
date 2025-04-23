using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminDeveloperSettingsController : Controller
    {
        public IActionResult Show(string id)
        {
            if (id == "app-settings")
            {
                var tzlist = TimeZoneInfo.GetSystemTimeZones();
                var languages = GetOption("languages", true);

                ViewBag.Id = id;
                ViewBag.TzList = tzlist;
                ViewBag.Languages = languages;
                ViewBag.AppName = Environment.GetEnvironmentVariable("APP_NAME") ?? "MyApp";
                ViewBag.AppDebug = Environment.GetEnvironmentVariable("APP_DEBUG") ?? "false";
                ViewBag.TimeZone = Environment.GetEnvironmentVariable("TIME_ZONE") ?? TimeZoneInfo.Local.Id;
                ViewBag.DefaultLang = Environment.GetEnvironmentVariable("DEFAULT_LANG") ?? "en";

                return View("App");
            }
            else if (id == "mail-settings")
            {
                var mailDriver = Environment.GetEnvironmentVariable("MAIL_DRIVER_TYPE") == "MAIL_DRIVER"
                    ? Environment.GetEnvironmentVariable("MAIL_DRIVER")
                    : Environment.GetEnvironmentVariable("MAIL_MAILER");

                ViewBag.Id = id;
                ViewBag.MailDriver = mailDriver;

                return View("Smtp");
            }
            else if (id == "storage-settings")
            {
                ViewBag.Id = id;
                return View("Storage");
            }
            else if (id == "wa-settings")
            {
                ViewBag.Id = id;
                return View("Whatsapp");
            }

            return NotFound();
        }

        private Dictionary<string, string> GetOption(string key, bool parseJson)
        {
            return new Dictionary<string, string>
            {
                { "en", "English" },
            };
        }
    }
}
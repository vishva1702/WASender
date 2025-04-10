using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WASender.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WASender.Contracts;

namespace WASender.Controllers.Admin
{
    public class AdminAboutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IFileService _fileService;

        public AdminAboutController(ApplicationDbContext context, IMemoryCache cache, IFileService fileService)
        {
            _context = context;
            _cache = cache;
            _fileService = fileService;
        }
        public IActionResult Index()
        {
            try
            {
                var languageOption = _context.Options
                    .Where(o => o.Key == "languages")
                    .ToList()
                    .FirstOrDefault();

                var aboutOption = _context.Options
                    .Where(o => o.Key == "about")
                    .ToList()
                    .FirstOrDefault();

                var counterOption = _context.Options
                    .Where(o => o.Key == "counter")
                    .ToList()
                    .FirstOrDefault();

                ViewData["languages"] = languageOption?.Value ?? "";
                ViewData["about"] = aboutOption?.Value ?? "";
                ViewData["counter_section"] = counterOption?.Value ?? "";

                return View(); // 👈 this will render Views/About/Index.cshtml by default if controller is AboutController
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        [Route("AdminAbout/Store")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(IFormCollection form)
        {
            try
            {
                var type = form["Type"].ToString();
                var lang = form["Lang"].ToString();

                Console.WriteLine($"Received Type: {type}, Lang: {lang}");

                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(lang))
                {
                    return Content("Error: Type or Lang is missing.");
                }

                if (type == "about")
                {
                    var aboutOption = await _context.Options
                        .AsTracking()
                        .FirstOrDefaultAsync(o => o.Key == "about" && o.Lang == lang);

                    if (aboutOption == null)
                    {
                        Console.WriteLine("Creating new aboutOption entry.");
                        aboutOption = new Option { Key = "about", Lang = lang };
                        await _context.Options.AddAsync(aboutOption);
                    }
                    else
                    {
                        Console.WriteLine("Updating existing aboutOption entry.");
                    }

                    var existingData = string.IsNullOrEmpty(aboutOption.Value)
                        ? new Dictionary<string, string>()
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(aboutOption.Value);

                    var data = new Dictionary<string, string>
            {
                { "breadcrumb_title", form["about_breadcrumb_title"].ToString() },
                { "section_title", form["about_section_title"].ToString() },
                { "experience", form["about_experience"].ToString() },
                { "experience_title", form["about_experience_title"].ToString() },
                { "description", form["about_description"].ToString() },
                { "button_title", form["about_button_title"].ToString() },
                { "button_link", form["about_button_link"].ToString() },
                { "facilities", form["about_facilities"].ToString() },
                { "introducing_video", form["about_introducing_video"].ToString() }
            };

                    data["about_image_1"] = existingData.ContainsKey("about_image_1") ? existingData["about_image_1"] : "";
                    data["about_image_2"] = existingData.ContainsKey("about_image_2") ? existingData["about_image_2"] : "";

                    var files = HttpContext.Request.Form.Files;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in files)
                    {
                        if (file.Name == "about_image_1" || file.Name == "about_image_2")
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            if (!string.IsNullOrEmpty(existingData[file.Name]))
                            {
                                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingData[file.Name].TrimStart('/'));
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                            }

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            data[file.Name] = "/uploads/" + fileName;
                        }
                    }

                    Console.WriteLine("New JSON Data: " + JsonConvert.SerializeObject(data));

                    aboutOption.Value = JsonConvert.SerializeObject(data);

                    try
                    {
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Database updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Database update error: " + ex.Message);
                        return Content("Database update error: " + ex.Message);
                    }

                    return RedirectToAction("SuccessPage");
                }

                return Content("Invalid request type.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return Content("Error: " + ex.Message);
            }
        }
    }
}

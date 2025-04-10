using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WASender.Models;

namespace WASender.Controllers.AdminSide
{
    public class AdminSeoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminSeoController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Show SEO list
        public async Task<IActionResult> Index()
        {
            var options = await _context.Options
                .Where(o => o.Key.Contains("seo"))
                .ToListAsync();

            var posts = options.Select(o => new
            {
                Id = o.Id,
                Key = o.Key.Replace("seo_", "").Replace("_", " "),
                Content = JsonSerializer.Deserialize<Dictionary<string, string>>(o.Value)
            }).ToList<dynamic>();

            return View(posts);
        }

        // Edit SEO entry
        public async Task<IActionResult> Edit(ulong id)
        {
            var option = await _context.Options
                .FirstOrDefaultAsync(o => o.Id == id && o.Key.Contains("seo"));

            if (option == null)
                return NotFound();

            var contents = JsonSerializer.Deserialize<Dictionary<string, string>>(option.Value ?? "");

            ViewBag.Id = id;
            return View(contents);
        }

        // Update SEO entry
        [HttpPost]
        public async Task<IActionResult> Update(ulong id, IFormFile? image, [FromForm] Dictionary<string, string> metadata)
        {
            var option = await _context.Options.FirstOrDefaultAsync(o => o.Id == id);
            if (option == null)
                return NotFound();

            var contents = JsonSerializer.Deserialize<Dictionary<string, string>>(option.Value ?? "") ?? new Dictionary<string, string>();

            if (image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Delete old image if it exists
                if (contents.TryGetValue("preview", out string? oldImagePath) && !string.IsNullOrEmpty(oldImagePath))
                {
                    var oldFullPath = Path.Combine(_environment.WebRootPath, "uploads", oldImagePath);
                    if (System.IO.File.Exists(oldFullPath))
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }

                metadata["preview"] = fileName;
            }

            option.Value = JsonSerializer.Serialize(metadata);
            await _context.SaveChangesAsync();

            return Json(new { message = "SEO settings updated successfully." });
        }
    }
}

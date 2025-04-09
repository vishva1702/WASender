using Microsoft.AspNetCore.Mvc;
using WASender.Models;
using Microsoft.EntityFrameworkCore;


namespace WASender.Controllers.AdminSide
{
    public class AdminPartenerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPartenerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;

            var brands = await _context.Categories
                .Where(c => c.Type == "brand")
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["brands"] = brands;
            ViewBag.totalBrands = await _context.Categories.CountAsync(c => c.Type == "brand");
            ViewBag.activeBrands = await _context.Categories.CountAsync(c => c.Type == "brand" && c.Status == 1);
            ViewBag.inActiveBrands = await _context.Categories.CountAsync(c => c.Type == "brand" && c.Status == 0);

            // Add this

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Store(IFormFile image, string? url, [FromForm] int status, string type)
        {
            // Input validation (existing code)
            if (image == null || image.Length == 0)
            {
                TempData["Error"] = "Image is required.";
                return RedirectToAction("Index");
            }

            try
            {
                // File upload (existing code)
                var preview = await FileUploader.SaveFile(image);
                Console.WriteLine($"Received Status: {status}"); // Debug log

                // Create entity (existing code)
                var category = new Category
                {
                    Title = string.IsNullOrWhiteSpace(url) ? "#" : url,
                    Status = status, // This might get overridden by DB default
                    Type = "brand",
                    Slug = preview,
                    Lang = type,
                    CreatedAt = DateTime.UtcNow
                };

                // First save (might get status=1 from DB default)
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // DEBUG: Check what was actually saved
                var initialStatus = category.Status;
                Console.WriteLine($"Status after first save: {initialStatus}");

                // If we wanted inactive (0) but got active (1), force update
                if (status == 0 && initialStatus == 1)
                {
                    Console.WriteLine("Overriding DB default to set status=0");
                    category.Status = 0;
                    _context.Entry(category).Property(x => x.Status).IsModified = true;
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Partner created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Something went wrong while creating the partner.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Destroy(ulong id)
        {
            try
            {
                var brand = await _context.Categories.FindAsync(id);

                if (brand == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Partner not found."
                    });
                }

                // Remove associated image if exists
                if (!string.IsNullOrEmpty(brand.Slug))
                {
                    try
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", brand.Slug.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Optional: log error
                    }
                }

                _context.Categories.Remove(brand);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Index", "AdminPartner"),
                    message = "Partner deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult Update(ulong id, string url, string type, int status, IFormFile image)
        {
            // Fetch entity by ID
            var brand = _context.Categories.FirstOrDefault(b => b.Id == id);
            if (brand == null) return NotFound();

            brand.Title = url;
            brand.Lang = type;
            brand.Status = status;

            if (image != null && image.Length > 0)
            {
                // Save image logic here and update brand.Slug
            }

            _context.SaveChanges();
            TempData["Success"] = "Partner updated successfully.";
            return RedirectToAction("Index");
        }


    }
}

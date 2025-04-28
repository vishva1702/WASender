using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;

namespace WASender.Controllers.Admin
{
    public class AdminTagController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCategoryController> _logger; // Declare logger

        public AdminTagController(ApplicationDbContext context, ILogger<AdminCategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.Type == "tags")
                                .OrderByDescending(c => c.Id)

                .ToListAsync(); // Fetch data as a List<Category>

            // Store additional computed data in ViewBag
            ViewBag.TotalTags = categories.Count;
            ViewBag.ActiveTags = categories.Count(c => c.Status == 1);
            ViewBag.InActiveTags = categories.Count(c => c.Status == 0);
            ViewBag.Languages = await _context.Categories.ToListAsync();

            // Compute PostCategoriesCount separately and store in ViewBag
            var postCategoriesCount = categories.ToDictionary(c => c.Id, c =>
                _context.Categories.Count(pc => pc.Id == c.Id) // Replace with actual relation
            );

            ViewBag.PostCategoriesCount = postCategoriesCount;

            return View(categories);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string Title, int Status, string Lang)
        {
            Console.WriteLine($"Title: {Title}, Status: {Status}, Lang: {Lang}"); // Debugging

            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Lang))
            {
                ModelState.AddModelError("", "Title and Language are required.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", _context.Categories.Where(c => c.Type == "tags").ToList());
            }

            var tag = new Category
            {
                Title = Title,
                Slug = Title.ToLower().Replace(" ", "-"),
                Type = "tags",
                Status = Status,
                Lang = Lang,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Categories.Add(tag);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ulong Id, string Title, int Status, string Lang)
        {
            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Lang))
            {
                TempData["Error"] = "Title and Language are required.";
                return RedirectToAction("Index");
            }

            var tag = _context.Categories.FirstOrDefault(c => c.Id == Id);
            if (tag == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            tag.Title = Title;
            tag.Slug = Title.ToLower().Replace(" ", "-");
            tag.Status = Status; // Fixing the status assignment
            tag.Lang = Lang;
            tag.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Categories.Update(tag);
                _context.SaveChanges();
                TempData["Success"] = "Tag updated successfully.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error updating category: {Message}", ex.InnerException?.Message ?? ex.Message);
                TempData["Error"] = "Failed to update category. Please try again.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ulong id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Tag not found." });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tag deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting category: {Message}", ex.InnerException?.Message ?? ex.Message);
                return Json(new { success = false, message = "Error deleting tag. Please check if it's being used elsewhere." });
            }
        }

    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add this
using System;
using System.Linq;
using WASender.Models;

namespace WASender.Controllers.Admin
{
    public class AdminCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCategoryController> _logger; // Declare logger

        public AdminCategoryController(ApplicationDbContext context, ILogger<AdminCategoryController> logger)
        {
            _context = context;
            _logger = logger; // Initialize logger
        }

        public IActionResult Index()
        {
            var categories = _context.Categories
                .Where(c => c.Type == "blog_category")
                .OrderByDescending(c => c.Id)
                .ToList();

            ViewData["TotalCategories"] = categories.Count;
            ViewData["ActiveCategories"] = categories.Count(c => c.Status == 1);
            ViewData["InActiveCategories"] = categories.Count(c => c.Status == 0);

            return View(categories);
        }

        public IActionResult Create(string Title, int Status, string Lang)
        {
            Console.WriteLine($"Title: {Title}, Status: {Status}, Lang: {Lang}"); // Debugging

            if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Lang))
            {
                ModelState.AddModelError("", "Title and Language are required.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", _context.Categories.Where(c => c.Type == "blog_category").ToList());
            }

            var category = new Category
            {
                Title = Title,
                Slug = Title.ToLower().Replace(" ", "-"),
                Type = "blog_category",
                Status = Status,
                Lang = Lang,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Categories.Add(category);
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

            var category = _context.Categories.FirstOrDefault(c => c.Id == Id);
            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            category.Title = Title;
            category.Slug = Title.ToLower().Replace(" ", "-");
            category.Status = Status; // Fixing the status assignment
            category.Lang = Lang;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error updating category: {Message}", ex.InnerException?.Message ?? ex.Message);
                TempData["Error"] = "Failed to update category. Please try again.";
            }

            return RedirectToAction("Index");
        }


        // POST: /Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ulong id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found." });
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Category deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting category: {Message}", ex.InnerException?.Message ?? ex.Message);
                return Json(new { success = false, message = "Error deleting category. Please check if it's being used elsewhere." });
            }
        }



    }
}

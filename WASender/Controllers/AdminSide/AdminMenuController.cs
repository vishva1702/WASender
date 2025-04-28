using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
<<<<<<< HEAD
    [Authorize(Roles = "admin,Admin")]
=======
>>>>>>> Dashboard
    public class AdminMenuController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminMenuController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            var menus = await _context.Menus?.OrderByDescending(m => m.Id).ToListAsync() ?? new List<Menu>();

            ViewBag.TotalMenus = menus.Count;
            ViewBag.TotalActiveMenus = menus.Count(m => m.Status == "1");
            ViewBag.TotalDraftMenus = menus.Count(m => m.Status == "0");

            // Ensure these values are always initialized to prevent null
            ViewBag.Languages = new[] { "English" };
            ViewBag.Positions = new[] { "main-menu" };

            return View(menus);
        }



        [HttpPost]
        public async Task<IActionResult> Store([FromForm] Menu menu) // ⬅ Change FromBody to FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { message = "Invalid data", errors });
            }

            // Ensure required fields are provided
            if (string.IsNullOrWhiteSpace(menu.Name))
            {
                return BadRequest(new { message = "Menu Name is required." });
            }

            if (string.IsNullOrWhiteSpace(menu.Position))
            {
                return BadRequest(new { message = "Menu Position is required." });
            }

            if (string.IsNullOrWhiteSpace(menu.Lang))
            {
                return BadRequest(new { message = "Menu Language is required." });
            }

            if (menu.Status != "1" && menu.Status != "0")
            {
                return BadRequest(new { message = "Invalid Status. It must be '1' (Active) or '0' (Draft)." });
            }

            // ✅ Fix: Ensure Data is not null or empty
            if (string.IsNullOrWhiteSpace(menu.Data))
            {
                menu.Data = "[]"; // Default to empty JSON array
            }

            // If the new menu is active, deactivate the previous active menu in the same position & language
            if (menu.Status == "1")
            {
                var existingMenu = await _context.Menus
                    .FirstOrDefaultAsync(m => m.Position == menu.Position && m.Lang == menu.Lang && m.Status == "1");

                if (existingMenu != null)
                {
                    existingMenu.Status = "0"; // Deactivate previous menu
                    _context.Update(existingMenu);
                    await _context.SaveChangesAsync();
                }
            }

            menu.CreatedAt = DateTime.UtcNow;
            menu.UpdatedAt = DateTime.UtcNow;

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            // ✅ Redirect to the index page after saving
            return RedirectToAction("Index", "AdminMenu");
        }




        // Store Date method (similar to the PHP storeDate method)
        [HttpPost]
        public async Task<IActionResult> StoreDate(ulong id, string data)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            try
            {
                JsonDocument.Parse(data); // Ensure the data is valid JSON
                menu.Data = data;
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid JSON format." });
            }
            _context.Update(menu);
            await _context.SaveChangesAsync();

            return Json(new { message = "Menu Updated Successfully." });
        }


        public async Task<IActionResult> Show(ulong id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            ViewBag.Contents = menu.Data ?? "[]";
            return View(menu);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Menu menu, string data)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(data))
            {
                data = "[]"; // Ensure Data is not empty
            }

            try
            {
                JsonDocument.Parse(data); // Validate JSON format
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid JSON format." });
            }

            var menuToUpdate = await _context.Menus.FindAsync(menu.Id);
            if (menuToUpdate == null)
            {
                return NotFound(new { message = "Menu not found." });
            }

            menuToUpdate.Name = menu.Name;
            menuToUpdate.Position = menu.Position;
            menuToUpdate.Status = menu.Status;
            menuToUpdate.Lang = menu.Lang;
            menuToUpdate.Data = data; // Assign data
            menuToUpdate.UpdatedAt = DateTime.UtcNow;

            _context.Update(menuToUpdate);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> Destroy(ulong id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return Json(new { success = false, message = "Menu not found." });
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Menu Removed Successfully." });
        }

    }

}

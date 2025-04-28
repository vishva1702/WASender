using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Helpers;
using WASender.Contracts.AdminSide;

namespace WASender.Controllers.Admin
{
    [Authorize]
    public class AdminTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly IAdminTemplateService _adminTemplateService;

        public AdminTemplateController(IAdminTemplateService adminTemplateService,ApplicationDbContext context, NotificationHelper notificationHelper)
        {
            _context = context;
            _notificationHelper = notificationHelper;
            _adminTemplateService = adminTemplateService;

        }
        public async Task<IActionResult> Index(string search, string type)
        {
            var templates = await _context.Templates
                .Include(t => t.User)
                .Include(t => t.Smstransactions)
                .Where(t => string.IsNullOrEmpty(search) ||
                    (type == "email" ? t.User.Email.Contains(search) : EF.Property<string>(t, type).Contains(search)))
                .OrderByDescending(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Type,
                    t.Status,
                    t.CreatedAt,
                    t.UserId,
                    UserName = t.User != null ? t.User.Name : "N/A",
                    SmsTransactionCount = t.Smstransactions.Count()
                })
                .ToListAsync();

            ViewBag.TotalActiveTemplates = await _context.Templates.CountAsync(t => t.Status == 1);
            ViewBag.TotalInactiveTemplates = await _context.Templates.CountAsync(t => t.Status == 0);
            ViewBag.TotalTemplates = await _context.Templates.CountAsync();
            ViewBag.SearchType = type;
            ViewBag.SearchQuery = search;

            return View(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                TempData["Error"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Template deleted successfully!";
            return RedirectToAction(nameof(Index));
        }


    }
}
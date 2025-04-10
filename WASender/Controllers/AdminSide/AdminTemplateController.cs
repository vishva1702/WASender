using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Helpers;

namespace WASender.Controllers.Admin
{
    [Authorize]
    public class AdminTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;

        public AdminTemplateController(ApplicationDbContext context, NotificationHelper notificationHelper)
        {
            _context = context;
            _notificationHelper = notificationHelper;
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

            return View("Templates", templates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ulong id, Template template)
        {
            try
            {
                var templte = await _context.Templates
                    .Include(t => t.Replies)
                    .Include(t => t.Schedulemessages)
                    .Include(t => t.Smstransactions)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (template == null)
                {
                    return Json(new { success = false, message = "Template not found." });
                }

                // Remove related records
                _context.Replies.RemoveRange(template.Replies);
                _context.Schedulemessages.RemoveRange(template.Schedulemessages);
                _context.Smstransactions.RemoveRange(template.Smstransactions);

                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();

                // Add notification if user exists
                if (template.UserId > 0)
                {
                    var notification = new Notification
                    {
                        UserId = (ulong)template.UserId,
                        Title = "Your template was removed by admin",
                        Url = "/user/template",
                        Seen = 0,
                        IsAdmin = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _notificationHelper.CreateNotification(notification);
                }

                return Json(new
                {
                    success = true,
                    message = "Template removed successfully.",
                    redirect = Url.Action("Index", "AdminTemplate")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error deleting template: {ex.Message}"
                });
            }
        }
        public class DeleteRequest
        {
            public ulong Id { get; set; }
        }
    }
    }
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using WASender.Models;

namespace WASender.Controllers.UserSide
{
    [Route("user/bulk-message")]
    public class BulkController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public BulkController(ApplicationDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public IActionResult Index()
        {
            // Get current user ID from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ulong.TryParse(userIdString, out ulong userId))
            {
                return Unauthorized();
            }

            // Sample data fetch for frontend binding (pagination & counts skipped)
            var posts = _context.Smstransactions
                .Include(s => s.Device)
                .Include(s => s.Template)
                .Where(s => s.UserId == userId && s.Type == "bulk-message")
                .OrderByDescending(s => s.Id)
                .Take(20)
                .ToList();

            var total = _context.Smstransactions
                .Count(s => s.UserId == userId && s.Type == "bulk-message");

            var today = DateTime.UtcNow.Date;
            var today_transaction = _context.Smstransactions
                .Count(s => s.UserId == userId && s.Type == "bulk-message" && s.CreatedAt.HasValue && s.CreatedAt.Value.Date == today);

            var last30_messages = _context.Smstransactions
                .Count(s => s.UserId == userId && s.Type == "bulk-message" && s.CreatedAt > DateTime.UtcNow.AddDays(-30));

            var devices = _context.Devices
                .Where(d => d.UserId == userId && d.Status == 1)
                .OrderByDescending(d => d.Id)
                .ToList();

            var templates = _context.Templates
                .Where(t => t.UserId == userId && t.Status == 1)
                .OrderByDescending(t => t.Id)
                .ToList();

            var groups = _context.Groups
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.Id)
                .ToList();

            ViewBag.Posts = posts;
            ViewBag.Total = total;
            ViewBag.TodayTransaction = today_transaction;
            ViewBag.Last30Messages = last30_messages;
            ViewBag.Devices = devices;
            ViewBag.Templates = templates;
            ViewBag.Groups = groups;

            return View("Views/User/Whatsapp/Bulk/Index.cshtml");
        }
    

// Helper method to get the current logged-in user ID (you might use the User.Identity or any other method you use for authentication)
private ulong GetCurrentUserId()
        {
            // Assuming you have a User object with Id available, adjust as per your authentication
            return ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private ulong GetUserId()
        {
            // example: return from claims or session
            return ulong.Parse(User.FindFirst("UserId").Value);
        }
    }
}
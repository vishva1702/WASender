using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using X.PagedList;

namespace WASender.Controllers.UserSide
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pageNumber = page ?? 1;
            var pageSize = 30;

            var logs = await _context.Smstransactions
                .Where(s => s.UserId == ulong.Parse(userId))
                .Include(s => s.Device)
                .Include(s => s.App)
                .Include(s => s.Template)
                .OrderByDescending(s => s.CreatedAt)
                .ToPagedListAsync(pageNumber, pageSize);

            // Store additional data in ViewBag
            ViewBag.TotalMessages = await _context.Smstransactions
                .Where(s => s.UserId == ulong.Parse(userId))
                .CountAsync();

            ViewBag.TodayMessages = await _context.Smstransactions
                .Where(s => s.UserId == ulong.Parse(userId) &&
                           s.CreatedAt.Value.Date == DateTime.Today)
                .CountAsync();

            ViewBag.Last30Messages = await _context.Smstransactions
                .Where(s => s.UserId == ulong.Parse(userId) &&
                           s.CreatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync();

            return View(logs);
        }
    }
}
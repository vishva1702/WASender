//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using WASender.Models;

//namespace WASender.Controllers.UserSide
//{
//    public class WebhookController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public WebhookController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var userId = GetLoggedInUserId(); // Replace with your actual logic for logged-in user

//            var hooks = await _context.Webhooks
//                .Where(w => w.UserId == userId)
//                .Include(w => w.Device)
//                .OrderByDescending(w => w.CreatedAt)
//                .Take(50) // Simulate pagination here or use library
//                .ToListAsync();

//            var total = await _context.Webhooks.CountAsync(w => w.UserId == userId);
//            var sentHooks = await _context.Webhooks.CountAsync(w => w.UserId == userId && w.Status == 1);
//            var failed = await _context.Webhooks.CountAsync(w => w.UserId == userId && w.Status == 0);

//            ViewData["Total"] = total;
//            ViewData["SentHooks"] = sentHooks;
//            ViewData["Failed"] = failed;

//            return View(hooks);
//        }

//        private ulong GetLoggedInUserId()
//        {
//            // Use your own authentication mechanism
//            return 1; // Placeholder
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;

namespace WASender.Controllers.UserSide
{
    [Authorize]
    public class WebhookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WebhookController(ApplicationDbContext context)
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
            var pageSize = 50;

            var hooks = await _context.Webhooks
                .Where(w => w.UserId == ulong.Parse(userId))
                .Include(w => w.Device)
                .OrderByDescending(w => w.CreatedAt)
                .ToPagedListAsync(pageNumber, pageSize);

            // Store additional data in ViewBag
            ViewBag.Total = await _context.Webhooks
                .Where(w => w.UserId == ulong.Parse(userId))
                .CountAsync();

            ViewBag.SentHooks = await _context.Webhooks
                .Where(w => w.UserId == ulong.Parse(userId) && w.Status == 1)
                .CountAsync();

            ViewBag.Failed = await _context.Webhooks
                .Where(w => w.UserId == ulong.Parse(userId) && w.Status == 0)
                .CountAsync();

            return View(hooks);
        }
    }
}
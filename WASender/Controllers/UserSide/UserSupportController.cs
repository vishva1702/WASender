using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WASender.Controllers.AdminSide;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers
{
<<<<<<< HEAD
    [Authorize(Roles = "user,User")]
=======
    [Authorize]
<<<<<<< HEAD
>>>>>>> Dashboard
=======
>>>>>>> 7a385f5 (UserSide (Home, Blogs, Features, Contactus, pricing page ))
    public class UserSupportController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UserSupportController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            await LoadGlobalDataAsync();

            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ulong.TryParse(userIdString, out ulong userId))
            {
                return RedirectToAction("Login", "Account");
            }

            int pageSize = 20;

            var supports = await _context.Supports
                .Where(s => s.UserId == userId)
                .Include(s => s.Supportlogs)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var openSupport = await _context.Supports
    .CountAsync(s => s.UserId == userId && s.Status == 1);

            var pendingSupport = await _context.Supports
    .CountAsync(s => s.UserId == userId && s.Status != 1 && s.Status != 0);


            var total = await _context.Supports
                .CountAsync(s => s.UserId == userId);

            ViewData["Supports"] = supports;
            ViewData["OpenSupport"] = openSupport;
            ViewData["PendingSupport"] = pendingSupport;
            ViewData["Total"] = total;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;

            return View(supports);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                ViewBag.ErrorMessage = "Subject and message are required.";
                return View("Create");
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ulong.TryParse(userIdString, out ulong userId))
            {
                ViewBag.ErrorMessage = "Invalid user ID.";
                return View("Create");
            }

            try
            {
                int newTicketNo = await _context.Supports.AnyAsync()
                    ? (await _context.Supports.MaxAsync(s => (int?)s.TicketNo) ?? 0) + 1
                    : 1;

                var support = new Support
                {
                    UserId = userId,
                    Subject = subject,
                    Status = 2,
                    CreatedAt = DateTime.UtcNow,
                    TicketNo = newTicketNo
                };

                _context.Supports.Add(support);
                await _context.SaveChangesAsync();

                var supportLog = new Supportlog
                {
                    SupportId = support.Id,
                    Comment = message,
                    IsAdmin = 0,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Supportlogs.Add(supportLog);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "New Ticket Generated Successfully";
                return View("Create");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the support ticket.";
                Console.WriteLine(ex.Message);
                return View("Create");
            }
        }

        public async Task<IActionResult> Show(ulong id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ulong.TryParse(userIdString, out ulong userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var support = await _context.Supports
                .Include(s => s.Supportlogs)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (support == null)
            {
                return NotFound();
            }

            return View(support);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ulong id, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { message = "Message is required." });
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ulong.TryParse(userIdString, out ulong userId))
            {
                return Unauthorized();
            }

            var support = await _context.Supports
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.Status == 1);

            if (support == null)
            {
                return NotFound();
            }

            var supportLog = new Supportlog
            {
                SupportId = support.Id,
                Comment = message,
                IsAdmin = 0,
                UserId = support.UserId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Supportlogs.Add(supportLog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = support.Id });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(ulong id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !ulong.TryParse(userIdString, out ulong userId))
            {
                return Unauthorized();
            }

            var support = await _context.Supports
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.Status == 1);

            if (support == null)
            {
                return NotFound();
            }

            support.Status = 0; 
            await _context.SaveChangesAsync();

            return Json(new
            {
                message = "Ticket closed successfully",
                redirect = Url.Action("Index")
            });
        }
    }
}
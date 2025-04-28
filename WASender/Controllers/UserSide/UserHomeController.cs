using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace WASender.Controllers.UserSide
{
    [Authorize(Roles = "user,User")]
    public class UserHomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UserHomeController(IGlobalDataService globalDataService, ILogger<UserHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        protected ulong? UserId
        {
            get
            {
                var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"Retrieved UserId: {userIdStr}");
                return ulong.TryParse(userIdStr, out var userId) ? userId : null;
            }
        }

        // Property to get the UserId from claims
        protected ulong? UserId
        {
            get
            {
                var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"Retrieved UserId: {userIdStr}");
                return ulong.TryParse(userIdStr, out var userId) ? userId : null;
            }
        }

        // Index action that loads the user's home page
        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            if (UserId == null)
            {
                TempData["saas_error"] = "User ID is not valid.";
                return View();
            }

            var user = await _context.Users.FindAsync(UserId.Value);
            if (user?.WillExpire != null)
            {
                var nextDate = DateTime.UtcNow.AddDays(7);
                var expirationDate = user.WillExpire.Value.ToDateTime(TimeOnly.MinValue);

                if (expirationDate <= DateTime.UtcNow)
                {
                    TempData["saas_error"] = $"Your subscription expired on {expirationDate:MMMM dd, yyyy}. Please renew your subscription.";
                }
                else if (expirationDate <= nextDate)
                {
                    TempData["saas_error"] = $"Your subscription is ending in {Math.Floor((expirationDate - DateTime.UtcNow).TotalDays)} days.";
                }
            }

            var totalDevices = await _context.Devices.CountAsync(d => d.UserId == UserId.Value);
            var totalMessages = await _context.Smstransactions.CountAsync(m => m.UserId == UserId.Value);
            var totalContacts = await _context.Contacts.CountAsync(c => c.UserId == UserId.Value);
            var pendingSchedules = await _context.Schedulemessages.CountAsync(s => s.Status == "pending" && s.UserId == UserId.Value);

            _logger.LogInformation($"Total Devices: {totalDevices}");
            _logger.LogInformation($"Total Messages: {totalMessages}");
            _logger.LogInformation($"Total Contacts: {totalContacts}");
            _logger.LogInformation($"Pending Schedules: {pendingSchedules}");

            ViewData["TotalDevices"] = totalDevices;
            ViewData["TotalMessages"] = totalMessages;
            ViewData["TotalContacts"] = totalContacts;
            ViewData["PendingSchedules"] = pendingSchedules;
            ViewData["RecentMessages"] = await GetMessagesTransaction(7);

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UserActivityOverview([FromBody] ActivityOverview request)
        {
            if (UserId == null)
            {
                _logger.LogError("User ID is invalid.");
                return Json(new { error = "User ID is invalid" });
            }

            var range = request.Type;
            var transactionsQuery = _context.Smstransactions.Where(m => m.UserId == UserId.Value);

            if (range == "Monthly")
            {
                var currentYear = DateTime.UtcNow.Year;
                var currentMonth = DateTime.UtcNow.Month;

                var transactions = await transactionsQuery
                    .Where(m => m.CreatedAt.HasValue &&
                                m.CreatedAt.Value.Year == currentYear &&
                                m.CreatedAt.Value.Month == currentMonth)
                    .GroupBy(m => m.CreatedAt.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Json(new { transactions });
            }
            else if (range == "Yearly")
            {
                var currentYear = DateTime.UtcNow.Year;

                var transactions = await transactionsQuery
                    .Where(m => m.CreatedAt.HasValue &&
                                m.CreatedAt.Value.Year == currentYear)
                    .GroupBy(m => m.CreatedAt.Value.Month)
                    .Select(g => new
                    {
                        Date = new DateTime(currentYear, g.Key, 1).ToString("MMMM"),
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Json(new { transactions });
            }
            else if (range == "Weekly")
            {
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                var transactions = await transactionsQuery
                    .Where(m => m.CreatedAt.HasValue &&
                                m.CreatedAt.Value.Date >= startOfWeek &&
                                m.CreatedAt.Value.Date <= endOfWeek)
                    .GroupBy(m => m.CreatedAt.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Json(new { transactions });
            }

            return Json(new { transactions = new List<object>() });
        }

        private async Task<List<object>> GetMessagesTransaction(int days)
        {
            if (UserId == null) return new List<object>();

            return await _context.Smstransactions
                .Where(m => m.UserId == UserId.Value && m.CreatedAt >= DateTime.UtcNow.AddDays(-days))
                .GroupBy(m => m.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    SmsTransactions = g.Count()
                })
                .ToListAsync<object>();
        }
    }

    public class ActivityOverview
    {
        public string Type { get; set; }
    }

    // Activity overview model for handling request types
    public class ActivityOverview
    {
        public string Type { get; set; }
    }
}

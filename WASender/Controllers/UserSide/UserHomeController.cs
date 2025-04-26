using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System.Security.Claims;
=======
>>>>>>> Dashboard
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
<<<<<<< HEAD
<<<<<<< HEAD
    [Authorize(Roles = "user, User")] // 🔒 Ensures only logged-in users can access this controller
    [Route("UserHome")]
=======
>>>>>>> Dashboard
    [Authorize(Roles = "user,User")]
=======
    [Authorize(Roles = "user, User")] 
>>>>>>> 7a385f5 (UserSide (Home, Blogs, Features, Contactus, pricing page ))
    public class UserHomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

<<<<<<< HEAD
<<<<<<< HEAD
        public UserHomeController(ApplicationDbContext context,IGlobalDataService globalDataService, ILogger<FeaturesController> logger)
             : base(globalDataService, logger) // Pass both required parameters to BaseController
        {
            this._context = context;
        }


        [HttpGet("")]
=======
>>>>>>> Dashboard
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
=======
        public UserHomeController(IGlobalDataService globalDataService, ILogger<FeaturesController> logger)
             : base(globalDataService, logger) 
        { }
>>>>>>> 7a385f5 (UserSide (Home, Blogs, Features, Contactus, pricing page ))

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
<<<<<<< HEAD
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.WillExpire != null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var nextWeek = today.AddDays(7);

                if (user?.WillExpire != null)
                {
                    //var today = DateOnly.FromDateTime(DateTime.Now);
                    //var nextWeek = today.AddDays(7);

                    if (user.WillExpire <= today)
                    {
                        var expirationDateTime = user.WillExpire.Value.ToDateTime(TimeOnly.MinValue);
                        TempData["saas_error"] = $"Your subscription was expired at {((DateTime?)expirationDateTime).ToRelativeTime()} please renew the subscription";
                    }
                    else if (user.WillExpire <= nextWeek)
                    {
                        var expirationDateTime = user.WillExpire.Value.ToDateTime(TimeOnly.MinValue);
                        TempData["saas_error"] = $"Your subscription is ending in {((DateTime?)expirationDateTime).ToRelativeTime()}";
                    }
                }
            }

            return View("~/Views/UserHome/Index.cshtml");
        }

        [HttpGet("dashboard-data")]
        public IActionResult DashboardData()
        {
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var data = new Dictionary<string, object>
            {
                ["devicesCount"] = _context.Devices.Count(d => d.UserId == userId),
                ["messagesCount"] = _context.Smstransactions.Count(s => s.UserId == userId),
                ["contactCount"] = _context.Contacts.Count(c => c.UserId == userId),
                ["scheduleCount"] = _context.Schedulemessages.Count(s => s.Status == "pending" && s.UserId == userId),
                ["devices"] = _context.Devices
                    .Where(d => d.UserId == userId)
                    .Include(d => d.Smstransactions)
                    .OrderByDescending(d => d.Status)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new
                    {
                        uuid = d.Uuid,
                        name = d.Name,
                        status = d.Status,
                        phone = d.Phone,
                        smstransaction_count = d.Smstransactions.Count
                    })
                    .ToList(),
                ["messagesStatics"] = GetMessagesTransaction(7, userId),
                ["typeStatics"] = MessagesStatics(7, userId),
                ["chatbotStatics"] = GetChatbotTransaction(7, userId)
            };

            return Json(data);
        }

        private List<dynamic> GetMessagesTransaction(int days, ulong userId)
        {
            var dateFrom = DateTime.Now.AddDays(-days).Date;

            return _context.Smstransactions
                .Where(s => s.UserId == userId && s.CreatedAt >= dateFrom)
                .OrderBy(s => s.Id)
                .GroupBy(s => s.CreatedAt.Value.Date)
                .Select(g => new
                {
                    date = g.Key,
                    smstransactions = g.Count()
                })
                .ToList<dynamic>();
        }

        private List<dynamic> GetChatbotTransaction(int days, ulong userId)
        {
            var dateFrom = DateTime.Now.AddDays(-days).Date;

            return _context.Smstransactions
                .Where(s => s.UserId == userId && s.Type == "chatbot" && s.CreatedAt >= dateFrom)
                .OrderBy(s => s.Id)
                .GroupBy(s => s.CreatedAt.Value.Date)
                .Select(g => new
                {
                    date = g.Key,
                    smstransactions = g.Count()
                })
                .ToList<dynamic>();
        }

        private List<dynamic> MessagesStatics(int days, ulong userId)
        {
            var dateFrom = DateTime.Now.AddDays(-days).Date;

            return _context.Smstransactions
                .Where(s => s.UserId == userId && s.CreatedAt >= dateFrom)
                .OrderBy(s => s.Id)
                .GroupBy(s => s.Type)
                .Select(g => new
                {
                    type = g.Key,
                    smstransactions = g.Count()
                })
                .ToList<dynamic>();
=======
>>>>>>> Dashboard

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

<<<<<<< HEAD
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return string.Empty;

            var span = DateTime.Now - dateTime.Value;

            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                return $"{years} year{(years == 1 ? "" : "s")} ago";
            }

            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                return $"{months} month{(months == 1 ? "" : "s")} ago";
            }

            if (span.Days > 0)
                return $"{span.Days} day{(span.Days == 1 ? "" : "s")} ago";

            if (span.Hours > 0)
                return $"{span.Hours} hour{(span.Hours == 1 ? "" : "s")} ago";

            if (span.Minutes > 0)
                return $"{span.Minutes} minute{(span.Minutes == 1 ? "" : "s")} ago";

            if (span.Seconds > 5)
                return $"{span.Seconds} seconds ago";

            return span.Seconds <= 5 ? "just now" : string.Empty;
        }
=======
>>>>>>> Dashboard
    public class ActivityOverview
    {
        public string Type { get; set; }
    }
}

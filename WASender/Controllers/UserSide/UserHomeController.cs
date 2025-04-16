using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.UserSide
{
    [Authorize(Roles = "user, User")] // 🔒 Ensures only logged-in users can access this controller
    [Route("UserHome")]
    public class UserHomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public UserHomeController(ApplicationDbContext context,IGlobalDataService globalDataService, ILogger<FeaturesController> logger)
             : base(globalDataService, logger) // Pass both required parameters to BaseController
        {
            this._context = context;
        }



        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            var userId = ulong.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.WillExpire != null)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var nextWeek = today.AddDays(7);

                if (user?.WillExpire != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var nextWeek = today.AddDays(7);

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
        }
    }

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
    }
}

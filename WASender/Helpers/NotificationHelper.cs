//using System;
//using System.Threading.Tasks;
//using WASender.Models;
//using Microsoft.Extensions.DependencyInjection;
//using System.Globalization;
//using System.Collections.Generic;
//using Microsoft.AspNetCore.Identity;


//namespace WASender.Helpers
//{
//    public class NotificationHelper
//    {
//        private readonly ApplicationDbContext _context;

//        public NotificationHelper(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task CreateNotificationAsync(Dictionary<string, object> data)
//        {
//            var notification = new Notification
//            {
//                UserId = Convert.ToUInt64(data["user_id"]),
//                Title = data["title"].ToString(),
//                Comment = data.ContainsKey("comment") ? data["comment"].ToString() : null,
//                Url = data["url"].ToString(),
//                IsAdmin = data.ContainsKey("is_admin") ? Convert.ToInt32(data["is_admin"]) : 0,
//            };

//            _context.Notifications.Add(notification);
//            await _context.SaveChangesAsync();
//        }

//        public async Task SendWillExpireEmailAsync(Dictionary<string, object> data)
//        {
//            var mailData = new Dictionary<string, string>
//            {
//                { "name", data["name"].ToString() },
//                { "plan_name", data["subscription"].ToString() },
//                { "plan_id", data["plan_id"].ToString() },
//                { "price", data["subscription"].ToString() },
//                { "will_expire", Convert.ToDateTime(data["will_expire"]).ToString("MMMM-dd-yyyy", CultureInfo.InvariantCulture) },
//                { "link", $"/user/subscription/{data["plan_id"]}" }
//            };

//            var title = "Subscription renewal notice";
//            var comment = $"Your subscription will end soon, the due date is {mailData["will_expire"]}";

//            var notification = new Dictionary<string, object>
//            {
//                { "user_id", data["id"] },
//                { "title", title },
//                { "comment", comment },
//                { "url", mailData["link"] }
//            };

//            await CreateNotificationAsync(notification);
//        }
//    }
//}
// In WASender/Helpers/NotificationHelper.cs
using WASender.Models;
using System.Threading.Tasks;

namespace WASender.Helpers
{
    public class NotificationHelper
    {
        private readonly ApplicationDbContext _context;

        public NotificationHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
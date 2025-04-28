using WASender.Models;
using WASender.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASender.Contracts.AdminSide;

namespace WASender.Services.AdminSide
{
    public class AdminScheduleService : IAdminScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;

        public AdminScheduleService(ApplicationDbContext context, NotificationHelper notificationHelper)
        {
            _context = context;
            _notificationHelper = notificationHelper;
        }

        public async Task<(List<Schedulemessage> schedules, int total, int pending, int delivered)> GetSchedulesAsync(string search, string type)
        {
            var query = _context.Schedulemessages
                .Include(s => s.User)
                .Include(s => s.Device)
                .Include(s => s.Schedulecontacts)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (type == "email")
                {
                    query = query.Where(s => s.User != null && s.User.Email == search);
                }
                else
                {
                    query = query.Where(s => EF.Functions.Like(EF.Property<string>(s, type), $"%{search}%"));
                }
            }

            var schedules = await query.OrderByDescending(s => s.Id).Take(30).ToListAsync();

            var total = await _context.Schedulemessages.CountAsync();
            var pending = await _context.Schedulemessages.CountAsync(s => s.Status == "pending");
            var delivered = await _context.Schedulemessages.CountAsync(s => s.Status == "delivered");

            return (schedules, total, pending, delivered);
        }

        public async Task<bool> DeleteScheduleAsync(ulong id)
        {
            var schedule = await _context.Schedulemessages.FindAsync(id);
            if (schedule == null) return false;

            _context.Schedulemessages.Remove(schedule);
            await _context.SaveChangesAsync();

            var notification = new Dictionary<string, object>
            {
                ["user_id"] = schedule.UserId ?? 0,
                ["title"] = "Your schedule was removed by admin",
                ["url"] = "/user/schedules"
            };

            await _notificationHelper.CreateNotificationAsync(notification);

            return true;
        }
    }
}

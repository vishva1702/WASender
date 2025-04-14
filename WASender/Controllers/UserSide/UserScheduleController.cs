using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using TimeZoneConverter;

namespace WASender.Controllers.UserSide
{
    [Authorize]
    public class UserScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Schedule
        public async Task<IActionResult> Index(int? page)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var pageNumber = page ?? 1;
            var pageSize = 20;

            var posts = await _context.Schedulemessages
                .Where(s => s.UserId == ulong.Parse(userId))
                .Include(s => s.Device)
                .OrderByDescending(s => s.CreatedAt)
                .ToPagedListAsync(pageNumber, pageSize);

            ViewBag.TotalSchedule = await _context.Schedulemessages
                .Where(s => s.UserId == ulong.Parse(userId))
                .CountAsync();

            ViewBag.PendingSchedule = await _context.Schedulemessages
                .Where(s => s.UserId == ulong.Parse(userId) && s.Status == "pending")
                .CountAsync();

            ViewBag.DeliveredSchedule = await _context.Schedulemessages
                .Where(s => s.UserId == ulong.Parse(userId) && s.Status == "delivered")
                .CountAsync();

            ViewBag.FailedSchedule = await _context.Schedulemessages
                .Where(s => s.UserId == ulong.Parse(userId) && s.Status == "rejected")
                .CountAsync();

            return View(posts);
        }

        // GET: Schedule/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Devices
            ViewBag.Devices = await _context.Devices
                .Where(d => d.UserId == ulong.Parse(userId) && d.Status == 1)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            // Templates
            ViewBag.Templates = await _context.Templates
                .Where(t => t.UserId == ulong.Parse(userId) && t.Status == 1)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Groups (manual join for Contacts)
            var groups = await _context.Groups
                .Where(g => g.UserId == ulong.Parse(userId))
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            var groupContacts = await _context.Groupcontacts
                .Where(gc => groups.Select(g => g.Id).Contains(gc.GroupId))
                .ToListAsync();

            var contactIds = groupContacts.Select(gc => gc.ContactId).Distinct().ToList();

            var contacts = await _context.Contacts
                .Where(c => contactIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name, c.Phone })
                .ToListAsync();

            var groupViewModels = groups.Select(group => new
            {
                Group = group,
                Contacts = groupContacts
                    .Where(gc => gc.GroupId == group.Id)
                    .Join(contacts,
                          gc => gc.ContactId,
                          c => c.Id,
                          (gc, c) => c)
                    .ToList()
            }).ToList();

            ViewBag.Groups = groupViewModels;

            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var date = DateTime.Parse(form["date"]);
                var timezone = form["timezone"];
                var timeZoneInfo = TZConvert.GetTimeZoneInfo(timezone);
                var dateTime = TimeZoneInfo.ConvertTimeToUtc(date, timeZoneInfo);

                var deviceId = ulong.Parse(form["device"]);
                var groupId = ulong.Parse(form["group"]);
                var messageType = form["message_type"];
                var title = form["title"];

                // Validate device
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == ulong.Parse(userId) && d.Status == 1);

                if (device == null)
                {
                    return NotFound("Device not found");
                }

                // Validate group
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == ulong.Parse(userId));

                if (group == null)
                {
                    return NotFound("Group not found");
                }

                // Fetch group contacts manually
                var groupContacts = await _context.Groupcontacts
                    .Where(gc => gc.GroupId == groupId)
                    .Select(gc => gc.ContactId)
                    .ToListAsync();

                if (!groupContacts.Any())
                {
                    return NotFound("Group has no contacts");
                }

                // Prepare schedule message
                var scheduleMessage = new Schedulemessage
                {
                    UserId = ulong.Parse(userId),
                    DeviceId = deviceId,
                    Title = title,
                    ScheduleAt = dateTime,
                    Zone = timezone,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Schedulemessages.Add(scheduleMessage);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View();
            }
        }


        // GET: Schedule/Show/5
        public async Task<IActionResult> Show(ulong id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var info = await _context.Schedulemessages
                .Include(s => s.Device)
                .Where(s => s.Id == id && s.UserId == ulong.Parse(userId))
                .Select(s => new
                {
                    Schedule = s,
                    ContactCount = s.Schedulecontacts.Count
                })
                .FirstOrDefaultAsync();

            if (info == null)
            {
                return NotFound();
            }

            ViewBag.Info = info.Schedule;
            ViewBag.ContactCount = info.ContactCount;

            var contacts = await _context.Schedulecontacts
                .Where(sc => sc.SchedulemessageId == id)
                .Include(sc => sc.Contact)
                .OrderBy(sc => sc.Id)
                .ToListAsync();

            return View(contacts);
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ulong id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var scheduleMessage = await _context.Schedulemessages
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == ulong.Parse(userId));

            if (scheduleMessage == null)
            {
                return NotFound();
            }

            _context.Schedulemessages.Remove(scheduleMessage);
            await _context.SaveChangesAsync();

            return Json(new
            {
                message = "Schedule Deleted Successfully",
                redirect = Url.Action("Index")
            });
        }

        private string FilterBody(string context)
        {
            return context.Replace("\\r", "\r").Replace("\\n", "\n");
        }
    }
}
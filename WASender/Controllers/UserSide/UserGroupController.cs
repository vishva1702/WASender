using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WASender.Models;

namespace WASender.Controllers
{
    public class UserGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get logged-in user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ulong ulongUserId = ulong.Parse(userId); // FIXED type issue

            // Fetch groups for user (latest with contact count)
            var groups = await _context.Groups
                .Where(g => g.UserId == ulongUserId)
                .OrderByDescending(g => g.Id)
                .ToListAsync();

            // Count of all groups for user
            var totalGroups = await _context.Groups
                .CountAsync(g => g.UserId == ulongUserId);

            // Group limit from user plan (JSON string)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == ulongUserId);
            int groupLimit = 0;

            if (user != null && !string.IsNullOrEmpty(user.Plan))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(user.Plan);
                    if (doc.RootElement.TryGetProperty("group_limit", out var limitProp))
                    {
                        groupLimit = limitProp.GetInt32();
                    }
                }
                catch
                {
                    // Handle invalid JSON if needed
                }
            }

            // Count of contacts per group
            var contactCounts = await _context.Groupcontacts
                .Where(gc => gc.Group.UserId == ulongUserId)
                .GroupBy(gc => gc.GroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.GroupId, g => g.Count);

            ViewBag.totalGroups = totalGroups;
            ViewBag.limit = groupLimit;
            ViewBag.ContactCounts = contactCounts;

            return View(groups);
        }

        // POST: /Group/Store
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store([FromForm] string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            {
                return Json(new { message = "Invalid group name." }, 400);
            }

            if (!ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out ulong userId))
            {
                return Json(new { message = "Invalid user ID." }, 400);
            }

            var group = new Group
            {
                Name = name,
                UserId = userId
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return Json(new
            {
                message = "Group Created Successfully",
                redirect = Url.Action("Index", "Group")
            });
        }

        //// POST: /Group/Update/{id}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Update(long id, [FromForm] string name)
        //{
        //    if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        //    {
        //        return Json(new { message = "Invalid group name." }, 400);
        //    }

        //    if (!ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out ulong userId))
        //    {
        //        return Json(new { message = "Invalid user ID." }, 400);
        //    }

        //    var group = await _context.Groups
        //        .Where(g => g.UserId == userId && g.Id == id)
        //        .FirstOrDefaultAsync();

        //    if (group == null)
        //    {
        //        return Json(new { message = "Group not found." }, 404);
        //    }

        //    group.Name = name;
        //    await _context.SaveChangesAsync();

        //    return Json(new
        //    {
        //        message = "Group Updated Successfully",
        //        redirect = Url.Action("Index", "Group")
        //    });
        //}

        //// POST: /Group/Delete/{id}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    if (!ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out ulong userId))
        //    {
        //        return Json(new { message = "Invalid user ID." }, 400);
        //    }

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var group = await _context.Groups
        //            .Include(g => g.GroupContacts)
        //            .Where(g => g.UserId == userId && g.Id == id)
        //            .FirstOrDefaultAsync();

        //        if (group == null)
        //        {
        //            return Json(new { message = "Group not found." }, 404);
        //        }

        //        var contactIds = group.GroupContacts.Select(gc => gc.ContactId).ToList();

        //        _context.Groups.Remove(group);
        //        _context.Contacts.RemoveRange(
        //            await _context.Contacts.Where(c => contactIds.Contains(c.Id) && c.UserId == userId).ToListAsync()
        //        );

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Json(new
        //        {
        //            message = "Group Deleted Successfully",
        //            redirect = Url.Action("Index", "Group")
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return Json(new { message = ex.Message }, 500);
        //    }
        //}
    }
}
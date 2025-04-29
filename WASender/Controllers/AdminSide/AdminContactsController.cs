using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace WASender.Controllers.Admin
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminContactsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminContactsController(
            IGlobalDataService globalDataService,
            ILogger<AdminContactsController> logger,
            ApplicationDbContext context
        ) : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string type)
        {
            await LoadGlobalDataAsync();

            var contacts = _context.Contacts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (type == "email")
                {
                    contacts = contacts.Where(c => c.User != null && c.User.Email == search);
                }
                else if (type == "name")
                {
                    contacts = contacts.Where(c => c.Name.Contains(search));
                }
                else if (type == "phone")
                {
                    contacts = contacts.Where(c => c.Phone.Contains(search));
                }
            }

            var totalContacts = await _context.Contacts.CountAsync();
            var scheduleContacts = await _context.Contacts.Where(c => c.Schedulecontacts.Any()).CountAsync();

            var paginatedContacts = await contacts.Include(c => c.User)
                                                  .OrderByDescending(c => c.CreatedAt)
                                                  .ToListAsync();

            ViewData["TotalContacts"] = totalContacts;
            ViewData["ScheduleContacts"] = scheduleContacts;
            ViewData["Search"] = search;
            ViewData["Type"] = type;

            return View(paginatedContacts);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return Json(new { success = false, message = "Contact not found" });
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Contact deleted successfully" });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WASender.Controllers.AdminSide;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminSupportController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminSupportController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string type)
        {
            await LoadGlobalDataAsync();

            var supportsQuery = _context.Supports.Include(s => s.User).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (type == "email")
                {
                    supportsQuery = supportsQuery.Where(s => s.User.Email == search);
                }
                else if (type == "ticket_no")
                {
                    if (int.TryParse(search, out int ticketNo))
                    {
                        supportsQuery = supportsQuery.Where(s => s.TicketNo == ticketNo);
                    }
                }
                else if (type == "subject")
                {
                    supportsQuery = supportsQuery.Where(s => s.Subject.Contains(search));
                }
            }

            var supports = await supportsQuery
                .Include(s => s.Supportlogs)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.TotalSupports = supports.Count;
            ViewBag.PendingSupport = supports.Count(s => s.Status == 2);
            ViewBag.OpenSupport = supports.Count(s => s.Status == 1);
            ViewBag.ClosedSupport = supports.Count(s => s.Status == 0);
            ViewBag.Supports = supports;
            ViewBag.Type = type;
            ViewBag.Search = search;

            return View();
        }

        public async Task<IActionResult> Show(ulong id)
        {
            var support = await _context.Supports
                .Include(s => s.Supportlogs)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (support == null)
            {
                return NotFound();
            }

            var seenLogs = _context.Supportlogs
                .Where(sl => sl.IsAdmin == 0 && sl.SupportId == id)
                .ToList();

            foreach (var log in seenLogs)
            {
                log.Seen = 1;
            }

            await _context.SaveChangesAsync();

            return View(support);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ulong id, string message, int status)
        {
            if (string.IsNullOrEmpty(message) || message.Length > 1000)
            {
                return BadRequest("Message is required and should not exceed 1000 characters.");
            }

            var support = await _context.Supports.FindAsync(id);
            if (support == null)
            {
                return NotFound();
            }

            support.Status = status;
            await _context.SaveChangesAsync();

            var supportLog = new Supportlog
            {
                Comment = message,
                IsAdmin = 1,
                Seen = 0,
                SupportId = id,
                UserId = support.UserId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Supportlogs.Add(supportLog);
            await _context.SaveChangesAsync();

            return RedirectToAction("Show", new { id = support.Id });
        }
    }
}
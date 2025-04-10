using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WASender.Services;
using Microsoft.AspNetCore.Authorization;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminHomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminHomeController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            ViewData["TotalOrders"] = await _context.Orders.CountAsync();

            ViewData["PendingOrders"] = await _context.Orders.CountAsync(o => o.Status == 2);

            ViewData["OpenSupport"] = await _context.Supports.CountAsync(s => s.Status == 1);

            ViewData["PendingSupport"] = await _context.Supports.CountAsync(s => s.Status == 2);

            ViewData["ActiveUsers"] = await _context.Users.CountAsync(u =>
                u.Status == 1 && u.WillExpire > DateOnly.FromDateTime(DateTime.UtcNow));

            ViewData["ActiveDevices"] = await _context.Devices.CountAsync(d => d.Status == 1 && d.Phone != null);

            ViewData["JunkDevices"] = await _context.Devices.CountAsync(d => d.Status == 0 && d.Phone == null);

            ViewData["TodaysMessages"] = await _context.Smstransactions
                .CountAsync(st => st.CreatedAt.HasValue && st.CreatedAt.Value.Date == DateTime.UtcNow.Date);

            ViewData["TodaysUsers"] = await _context.Users
                .CountAsync(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Date == DateTime.UtcNow.Date);

            ViewData["ServerStatus"] = "Running"; 

            ViewData["RecentOrders"] = await _context.Orders
                .Where(o => o.User != null && o.Plan != null)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new
                {
                    Avatar = o.User.Avatar != null
                        ? Url.Content(o.User.Avatar)
                        : $"https://ui-avatars.com/api/?name={o.User.Name}",
                    Name = o.User.Name,
                    Plan = o.Plan.Title,
                    Invoice = o.InvoiceNo,
                    Amount = $"{o.Amount:C}",
                    CreatedAt = o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("g") : "N/A",
                    Link = Url.Action("Details", "Order", new { id = o.Id })
                })
                .ToListAsync();


            ViewData["PopularPlans"] = await _context.Plans
                .Where(p => p.Orders.Any())
                .OrderByDescending(p => p.Orders.Count)
                .Select(p => new
                {
                    Name = p.Title,
                    ActiveUsers = _context.Users.Count(u => u.Id == p.Id),
                    OrdersCount = p.Orders.Count(),
                    TotalAmount = $"{p.Orders.Sum(o => o.Amount):C}"
                })
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> SalesOverview([FromBody] SalesOverview request)
        {
            var range = request.Type;
            var ordersQuery = _context.Orders.AsQueryable();

            if (range == "Monthly")
            {
                var currentYear = DateTime.UtcNow.Year;
                var currentMonth = DateTime.UtcNow.Month;

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt.HasValue &&
                                o.CreatedAt.Value.Year == currentYear &&
                                o.CreatedAt.Value.Month == currentMonth)
                    .GroupBy(o => o.CreatedAt.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Amount = g.Sum(o => o.Amount ?? 0)
                    })
                    .ToListAsync();

                return Json(new { orders });
            }
            else if (range == "Yearly")
            {
                var currentYear = DateTime.UtcNow.Year;

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt.HasValue &&
                                o.CreatedAt.Value.Year == currentYear)
                    .GroupBy(o => o.CreatedAt.Value.Month)
                    .Select(g => new
                    {
                        Date = new DateTime(currentYear, g.Key, 1).ToString("MMMM"),
                        Amount = g.Sum(o => o.Amount ?? 0)
                    })
                    .ToListAsync();

                return Json(new { orders });
            }
            else if (range == "Weekly")
            {
                var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                var orders = await ordersQuery
                    .Where(o => o.CreatedAt.HasValue &&
                                o.CreatedAt.Value.Date >= startOfWeek &&
                                o.CreatedAt.Value.Date <= endOfWeek)
                    .GroupBy(o => o.CreatedAt.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Amount = g.Sum(o => o.Amount ?? 0)
                    })
                    .ToListAsync();

                return Json(new { orders });
            }

            return Json(new { orders = new List<object>() });
        }
    }

    public class SalesOverview
    {
        public string Type { get; set; }
    }
}
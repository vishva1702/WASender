using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.Admin
{
    [Authorize] // Ensure authentication is applied
    public class AdminOrderController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(ApplicationDbContext context, IGlobalDataService globalDataService, ILogger<BaseController> logger)
            : base(globalDataService, logger)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.Plans = await _context.Plans.ToListAsync();
            ViewBag.Gateways = await _context.Gateways.ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Plan)
                .Include(o => o.Gateway)
                .Include(o => o.User)
                .ToListAsync();

            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalCompleteOrders = orders.Count(o => o.Status == 1);
            ViewBag.TotalPendingOrders = orders.Count(o => o.Status == 2);
            ViewBag.TotalDeclinedOrders = orders.Count(o => o.Status == 0);

            return View(orders);
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateOrder(Order model, string UserEmail)
        //{
        //    //if (!ModelState.IsValid)
        //    //{
        //    //    TempData["ErrorMessage"] = "Invalid order data. Please check your input.";
        //    //    return View("Index"); // Stay on the same page
        //    //}

        //    //var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail);
        //    //if (user == null)
        //    //{
        //    //    TempData["ErrorMessage"] = "User not found.";
        //    //    return View("Index"); // Stay on the same page
        //    //}

        //    //var order = new Order
        //    //{
        //    //    UserId = user.Id,
        //    //    PlanId = model.PlanId,
        //    //    GatewayId = model.GatewayId,
        //    //    PaymentId = model.PaymentId,
        //    //    Amount = model.Amount,
        //    //    Tax = model.Tax,
        //    //    Status = 2, // Pending
        //    //    CreatedAt = DateTime.UtcNow
        //    //};

        //    //_context.Orders.Add(order);
        //    //await _context.SaveChangesAsync();

        //    //TempData["SuccessMessage"] = "Order created successfully!";
        //    //return View("Index"); // Stay on the same page

        //    try
        //    {
        //        // Debugging purpose
        //        Console.WriteLine($"UserEmail: {UserEmail}, PlanId: {model.PlanId}, GatewayId: {model.GatewayId}, Amount: {model.Amount}, Tax: {model.Tax}");

        //        // Check if user exists
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail);
        //        if (user == null)
        //        {
        //            return Json(new { success = false, message = "User not found." });
        //        }

        //        // Create new order instance
        //        var order = new Order
        //        {
        //            UserId = user.Id,
        //            PlanId = model.PlanId,
        //            GatewayId = model.GatewayId,
        //            PaymentId = model.PaymentId,
        //            Amount = model.Amount ?? 0, // Handle null values
        //            Tax = model.Tax ?? 0, // Handle null values
        //            Status = 2, // Pending status
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        // Save order in database
        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();

        //        // Fetch updated order list
        //        var orders = await _context.Orders
        //            .Include(o => o.Plan)
        //            .Include(o => o.Gateway)
        //            .Include(o => o.User)
        //            .ToListAsync();

        //        return Json(new { success = true, message = "Order created successfully!", orders });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error: " + ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order model, string UserEmail)
        {
            Console.WriteLine("CreateOrder method hit!"); // Debugging
            Console.WriteLine($"UserEmail: {UserEmail}, PlanId: {model.PlanId}, GatewayId: {model.GatewayId}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var order = new Order
            {
                UserId = user.Id,
                PlanId = model.PlanId,
                GatewayId = model.GatewayId,
                PaymentId = model.PaymentId,
                Amount = model.Amount ?? 0,
                Tax = model.Tax ?? 0,
                Status = 2,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order created successfully!" });
        }




        // GET: Orders/Details/5
        public async Task<IActionResult> Show(ulong id)
        {
            await LoadGlobalDataAsync(); // Load global data

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Plan)
                .Include(o => o.Gateway)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Update/5
        [HttpPost]
        public async Task<IActionResult> Update(ulong id, int status, bool assignOrder)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            if (assignOrder)
            {
                order.User.PlanId = (int)order.PlanId;
                order.User.WillExpire = order.WillExpire;
                order.User.Plan = order.Plan?.Data;

                _context.Users.Update(order.User);
                await _context.SaveChangesAsync();
            }

            string statusText = status == 2 ? "pending" : (status == 1 ? "approved" : "declined");
            string title = $"({order.InvoiceNo}) Subscription order is {statusText}";

            // Create Notification (Needs Notification Service)
            var notification = new Notification
            {
                UserId = order.UserId,
                Title = title,
                Url = "/user/subscription-history"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(new { message = "Order status updated" });
        }

        // GET: Admin/Invoice/GetSettings
        [HttpGet("GetSettings")]
        public async Task<IActionResult> GetSettings()
        {
            var adminUser = await _context.Users
                .Where(u => u.Role == "Admin")
                .FirstOrDefaultAsync();

            if (adminUser == null)
            {
                return NotFound(new { message = "Admin user not found." });
            }

            return Ok(new
            {
                CompanyName = adminUser.Name,
                CompanyAddress = adminUser.Address,
                CompanyCity = "N/A",  // Change if stored
                PostCode = "000000",  // Change if stored
                Country = "N/A"       // Change if stored
            });
        }

        // POST: Admin/Invoice/SaveSettings
        [HttpPost("SaveSettings")]
        public async Task<IActionResult> SaveSettings([FromBody] User updatedUser)
        {
            if (updatedUser == null)
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            if (adminUser == null)
            {
                return NotFound(new { message = "Admin user not found." });
            }

            // Update the User model directly
            adminUser.Name = updatedUser.Name;
            adminUser.Address = updatedUser.Address;

            _context.Users.Update(adminUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invoice settings updated successfully!" });
        }

    }
}


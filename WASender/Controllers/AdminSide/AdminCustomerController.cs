using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WASender.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WASender.Controllers.AdminSide
{
    public class AdminCustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCustomerController> _logger;

        public AdminCustomerController(ApplicationDbContext context, ILogger<AdminCustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: AdminCustomer/Index
        public async Task<IActionResult> Index(string search, string type)
        {

            var customers = _context.Users.Where(c => c.Role == "user");

            if (!string.IsNullOrEmpty(search))
            {
                if (type == "email")
                    customers = customers.Where(c => c.Email.Contains(search));
                else if (type == "name")
                    customers = customers.Where(c => c.Name.Contains(search));
            }

            var customerList = await customers
                .Include(c => c.Orders)
                .Include(c => c.Smstransactions)
                .Include(c => c.Devices)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewData["TotalCustomers"] = await _context.Users.CountAsync(c => c.Role == "user");
            ViewData["TotalActiveCustomers"] = await _context.Users.CountAsync(c => c.Role == "user" && c.Status == 1);
            ViewData["TotalSuspendedCustomers"] = await _context.Users.CountAsync(c => c.Role == "user" && c.Status == 0);
            ViewData["TotalExpiredCustomers"] = await _context.Users
                .CountAsync(c => c.Role == "user" && c.WillExpire.HasValue &&
                                 c.WillExpire.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.UtcNow);

            return View(customerList);
        }

        // GET: AdminCustomer/Show/{id}
              public async Task<IActionResult> Show(ulong id)
        {
            
            return View();
        }


        // GET: AdminCustomer/Edit/{id}
        public async Task<IActionResult> Edit(ulong id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: AdminCustomer/Update/{id}
        public async Task<IActionResult> Update(User model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Validation failed. Please check your inputs.";
                return View("Edit", model);
            }

            var customer = await _context.Users.FindAsync(model.Id);
            if (customer == null)
            {
                TempData["Error"] = "User not found.";
                return View("Edit", model);
            }

            customer.Name = model.Name;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.Address = model.Address;

            // Ensure Status is updated
            customer.Status = model.Status;

            // Handle Password Change
            if (!string.IsNullOrEmpty(model.Password))
            {
                customer.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            try
            {
                _context.Users.Update(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User updated successfully.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Error updating user: {0}", ex.InnerException?.Message ?? ex.Message);
                TempData["Error"] = "Failed to update user. Please try again.";
                return View("Edit", model);
            }

            return RedirectToAction(nameof(Index));
        }


        // POST: AdminCustomer/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer == null) return NotFound();

            _context.Users.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

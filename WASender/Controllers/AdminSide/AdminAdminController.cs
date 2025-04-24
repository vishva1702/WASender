using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminAdminController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;


        public AdminAdminController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
            : base(globalDataService, logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            var users = await _context.Users
                .Where(u => u.Role == "admin" && u.Id != 1)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: Create admin form
        public async Task<IActionResult> Create()
        {
            await LoadGlobalDataAsync();

            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Store()
        {
            var name = Request.Form["name"].ToString();
            var email = Request.Form["email"].ToString();
            var password = Request.Form["password"].ToString();
            var passwordConfirmation = Request.Form["password_confirmation"].ToString();
            var roles = Request.Form["roles[]"].ToArray(); // handles multiple select

            // ✅ Validation
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || roles.Length == 0)
            {
                return BadRequest("Please fill all fields.");
            }

            if (password != passwordConfirmation)
            {
                return BadRequest("Password and confirmation do not match.");
            }

            // ✅ Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return BadRequest("Email already exists.");
            }

            // ✅ Create user
            var user = new User
            {
                Name = name,
                Email = email,
                Role = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "AdminAdmin", new { area = "Admin" });

        }


        public IActionResult Edit(ulong id)
        {
             LoadGlobalDataAsync();

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            var roles = _context.Roles.ToList();
            ViewBag.Roles = roles ?? new List<Role>();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ulong id, User updatedUser, string Role, string password, string passwordConfirmation)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // If password is not being changed, remove validation for it
            if (string.IsNullOrEmpty(password) && string.IsNullOrEmpty(passwordConfirmation))
            {
                // Remove ModelState errors related to password fields
                ModelState.Remove("Password");
                ModelState.Remove("PasswordConfirmation");
            }
            else if (password != passwordConfirmation)
            {
                TempData["Error"] = "Password and confirmation do not match.";
                return RedirectToAction("Edit", new { id });
            }

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();

                TempData["Error"] = "Update failed due to the following issue(s): " + string.Join(" ", allErrors);
                return RedirectToAction("Edit", new { id });
            }

            // Update user details
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.Status = updatedUser.Status;

            if (!string.IsNullOrEmpty(password))
            {
                user.Password = _passwordHasher.HashPassword(user, password);
            }

            if (!string.IsNullOrEmpty(Role))
            {
                user.Role = "admin";
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Admin updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            if (id == 1)
            {
                return Json(new { success = false, message = "Super admin cannot be deleted." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "admin");

            if (user == null)
            {
                return Json(new { success = false, message = "Error! Admin not found." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new { success = true, message = "Admin deleted successfully!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error deleting admin: {ex.Message}" });
            }
        }

    }
}

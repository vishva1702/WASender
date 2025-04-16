using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WASender.Models;
using Microsoft.Extensions.Logging;
using WASender.Controllers.AdminSide;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    public class AdminProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminProfileController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();

            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(Convert.ToUInt64(userId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View("Index", user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string type, IFormCollection form, IFormFile avatar)
        {
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(Convert.ToUInt64(userId));
            if (user == null) return RedirectToAction("Login", "Account");

            try
            {
                if (type == "password")
                {
                    string oldPassword = form["oldpassword"];
                    string newPassword = form["password"];
                    string confirmPassword = form["password_confirmation"];

                    var passwordHasher = new PasswordHasher<User>();
                    if (passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Failed)
                    {
                        ViewBag.Error = "Old password is incorrect.";
                        return await Index(); // Return view with error message
                    }

                    if (newPassword != confirmPassword)
                    {
                        ViewBag.Error = "New password and confirm password do not match.";
                        return await Index(); // Return view with error message
                    }

                    user.Password = passwordHasher.HashPassword(user, newPassword);
                    ViewBag.Success = "Password changed successfully!";
                }
                else
                {
                    // General settings update logic
                    user.Name = form["name"];
                    user.Email = form["email"];
                    user.Phone = form["phone"];
                    user.Address = form["address"];

                    if (avatar != null)
                    {
                        // Avatar upload logic
                        string userFolder = Path.Combine("wwwroot/uploads", user.Id.ToString(), DateTime.Now.ToString("yy/MM"));
                        Directory.CreateDirectory(userFolder);
                        string fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(avatar.FileName)}";
                        string filePath = Path.Combine(userFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await avatar.CopyToAsync(stream);
                        }

                        user.Avatar = $"/uploads/{user.Id}/{DateTime.Now:yy/MM}/{fileName}";
                    }

                    ViewBag.Success = "Settings updated successfully!";
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while updating your profile.";
            }

            return await Index(); 
        }

    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WASender.Models;
using Microsoft.Extensions.Logging;

namespace WASender.Controllers.UserSide
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public UserProfileController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var user = await _dbContext.Users.FindAsync(Convert.ToUInt64(userId));

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View("Index", user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string type, IFormCollection form, IFormFile avatar)
        {
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var user = await _dbContext.Users.FindAsync(Convert.ToUInt64(userId));
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
                        TempData["Error"] = "Old password is incorrect.";
                        return RedirectToAction("Index");
                    }
 

                    if (newPassword != confirmPassword)
                    {
                        TempData["Error"] = "New password and confirm password do not match.";
                        return RedirectToAction("Index");
                    }

                    user.Password = passwordHasher.HashPassword(user, newPassword);
                    TempData["Success"] = "Password changed successfully!";
                }
                else
                {
                    user.Name = form["name"];
                    user.Email = form["email"];
                    user.Phone = form["phone"];
                    user.Address = form["address"];

                    if (avatar != null)
                    {
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

                    TempData["Success"] = "Settings updated successfully!";
                }

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating your profile.";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> AuthKey()
        {
            var userId = User.FindFirstValue("UserId"); 

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _dbContext.Users.FindAsync(Convert.ToUInt64(userId)); 

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View("AuthKey", user); 

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateAuthKey()
        {
            var userId = User.FindFirstValue("UserId"); 

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var user = await _dbContext.Users.FindAsync(Convert.ToUInt64(userId)); 

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.Authkey = Guid.NewGuid().ToString();

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true, newAuthKey = user.Authkey });
        }

    }
}

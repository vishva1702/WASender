using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WASender.Services;
using WASender.Contracts;

namespace WASender.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IForgotPasswordService _forgotPasswordService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IGlobalDataService globalDataService,
            IForgotPasswordService forgotPasswordService,
            ILogger<AccountController> logger)
            : base(globalDataService, logger)
        {
            _forgotPasswordService = forgotPasswordService;
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPasswordAsync()
        {
            await LoadGlobalDataAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            bool emailSent = await _forgotPasswordService.SendPasswordResetEmailAsync(email);
            ViewBag.Status = emailSent ? "Password reset link has been sent." : "Failed to send reset email. Please try again.";

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string email, string password, string password_confirmation)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Invalid request. Missing token or email.";
                return View();
            }
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password_confirmation))
            {
                ViewBag.Error = "Both password fields are required.";
                return View();
            }
            if (password != password_confirmation)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            bool success = await _forgotPasswordService.ResetPasswordAsync(email, token, password);

            if (success)
            {
                TempData["SuccessMessage"] = "Password has been reset successfully!";
                return RedirectToAction("Index", "Login"); // Redirect to the login page
            }
            else
            {
                ViewBag.Error = "Invalid token, expired token, or email mismatch.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            await _forgotPasswordService.TestSmtpConnectionAsync();
            return Content("SMTP Test Executed! Check Console.");
        }
    }
}

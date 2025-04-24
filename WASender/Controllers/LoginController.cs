using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using WASender.Controllers;
using WASender.Models;
using WASender.Services;

public class LoginController : BaseController
{
    private readonly ILoginService _loginService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IGlobalDataService globalDataService, ILogger<LoginController> logger, ILoginService loginService, ApplicationDbContext context)
        : base(globalDataService, logger)
    {
        _loginService = loginService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await LoadGlobalDataAsync();
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {

        _logger.LogInformation("Login attempt for email: {Email}", email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found.");
            ViewBag.Error = "Invalid email or password";
            return View("Index");
        }

        if (!await _loginService.ValidateUserAsync(email, password))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Email}", email);
            ViewBag.Error = "Invalid email or password";
            return View("Index");
        }

        var role = await _loginService.GetUserRoleAsync(email);
        var token = await _loginService.GenerateJwtTokenAsync(email);

        // IMPORTANT: Add ClaimTypes.NameIdentifier so that TemplateController can get the user id.
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, email),
            new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role))
            // Optionally add additional claims if needed
        };
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim("UserId", user.Id.ToString()), // Add this
    new Claim("Avatar", user.Avatar ?? ""), // Add this if Avatar exists in user
    new Claim(ClaimTypes.Name, email),
    new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(role))
};


        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                ExpiresUtc = DateTime.UtcNow.AddMinutes(120)
            });

        // Set the JWT token in a cookie (if needed)
        Response.Cookies.Append("authkey", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to true if using HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(120)
        });

        _logger.LogInformation("Login successful for {Email}, role: {Role}", email, role);
        await LoadGlobalDataAsync();

        return role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "AdminHome")
            : RedirectToAction("Index", "UserHome");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Sign out the user from cookie authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remove the authkey cookie if it exists
        if (Request.Cookies.ContainsKey("authkey"))
        {
            Response.Cookies.Delete("authkey");
        }

        // Optionally clear the session if used
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home"); // Redirect to login page after logout
        return RedirectToAction("Index", "Login"); // Redirect to login page after logout
    }

    [HttpPost]
    public async Task<IActionResult> LoginApi(string email, string password)
    {
        _logger.LogInformation("API Login attempt for email: {Email}", email);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("API Login failed: User not found.");
            return Unauthorized("Invalid email or password");
        }

        if (!await _loginService.ValidateUserAsync(email, password))
        {
            _logger.LogWarning("API Login failed: Invalid password.");
            return Unauthorized("Invalid email or password");
        }

        var role = await _loginService.GetUserRoleAsync(email);
        var token = await _loginService.GenerateJwtTokenAsync(email);

        _logger.LogInformation("API Login successful for {Email}, role: {Role}", email, role);
        return Ok(new { Token = token, Role = role });
    }
}
}

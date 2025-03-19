using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public IActionResult Index()
    {
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserId", user.Id.ToString()) // Ensure UserId claim exists
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
            });

        Response.Cookies.Append("authkey", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(30)
        });

        _logger.LogInformation("Login successful for {Email}, role: {Role}", email, role);
        await LoadGlobalDataAsync();

        return role == "admin" ? RedirectToAction("Dashboard", "Admin") : RedirectToAction("Index", "UserHome");
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

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User logged out.");

        // Clear authentication session
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remove all cookies
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        return RedirectToAction("Index", "Home");
    }

}

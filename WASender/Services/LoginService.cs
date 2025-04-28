using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WASender.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

namespace WASender.Services
{
    public class LoginService : ILoginService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public LoginService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new { u.Email, u.Password })
                .FirstOrDefaultAsync();

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return false; // User not found or password is missing
            }

            // ✅ Check if password is stored in bcrypt format (from PHP or older .NET systems)
            if (user.Password.StartsWith("$2a$") || user.Password.StartsWith("$2b$") || user.Password.StartsWith("$2y$"))
            {
                return BCrypt.Net.BCrypt.Verify(password, user.Password);
            }

            // ✅ Fallback: Check if password is stored in ASP.NET Identity format
            var result = _passwordHasher.VerifyHashedPassword(new object(), user.Password, password);

            return result == PasswordVerificationResult.Success;
        }

        public async Task<string> GetUserRoleAsync(string email)
        {
            var role = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            return string.IsNullOrEmpty(role) ? "user" : role; // Default role = "user"
        }

        public async Task<string> GenerateJwtTokenAsync(string email)
        {
            var role = await GetUserRoleAsync(email);

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString) || keyString.Length < 32)
            {
                throw new ArgumentException("JWT Key is invalid! It must be at least 32 characters.");
            }

            var key = Encoding.UTF8.GetBytes(keyString);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60), // Set token expiry to 1 hour
            signingCredentials: credentials
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Hash password using bcrypt (same as PHP)
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12); // PHP-compatible hashing
        }
    }
}

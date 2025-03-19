using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Services;

namespace WASender.Services
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ForgotPasswordService> _logger;

        public ForgotPasswordService(ApplicationDbContext context, IConfiguration configuration, ILogger<ForgotPasswordService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return false;

                var resetToken = Guid.NewGuid().ToString();
                user.RememberToken = resetToken;
                await _context.SaveChangesAsync();

                var resetLink = $"https://localhost:7287/Account/ResetPassword?token={resetToken}&email={email}";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("WASender", "krishahun7@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Password Reset";
                message.Body = new TextPart("plain") { Text = $"Click the link below to reset your password:\n{resetLink}" };

                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync("smtp.gmail.com", 587, false);
                    await smtp.AuthenticateAsync("krishahun7@gmail.com", "bmnkunptvkmynjqj"); // Use your App Password here
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);
                }

                _logger.LogInformation("Password reset email sent successfully to {Email}.", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending password reset email: {Error}", ex.Message);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.RememberToken == token);
            if (user == null)
                return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.RememberToken = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task TestSmtpConnectionAsync()
        {
            using (var smtp = new SmtpClient())
            {
                try
                {
                    await smtp.ConnectAsync("smtp.gmail.com", 587, false);
                    await smtp.AuthenticateAsync("krishahun7@gmail.com", "bmnkunptvkmynjqj"); // Use your App Password
                    _logger.LogInformation("✅ SMTP Connection Successful!");
                    await smtp.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError("❌ SMTP Connection Failed: {Error}", ex.Message);
                }
            }
        }
    }
}

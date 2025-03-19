using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<RegisterService> _logger; // ✅ Add logger

        // ✅ Update constructor to accept ILogger
        public RegisterService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ILogger<RegisterService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger; // ✅ Assign it
        }

        public async Task<(string PlanTitle, ulong PlanId)> GetPlanDetails(ulong? id)
        {
            if (id.HasValue)
            {
                var plan = await _context.Plans.FindAsync(id.Value);
                if (plan != null)
                {
                    return (plan.Title, plan.Id);
                }
            }
            return ("Premium Plan", 1);
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterUserAsync(User model)
        {
            try
            {
                _logger.LogInformation("Starting registration process for email: {Email}", model.Email);

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.LogWarning("Email already exists: {Email}", model.Email);
                    return (false, "Email already exists.");
                }

                _logger.LogInformation("Generating auth key...");
                model.Authkey = GenerateAuthKey();

                _logger.LogInformation("Hashing password with BCrypt...");
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password); // ✅ FIXED!

                model.Status = 1;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                model.PlanId = model.PlanId ?? 1;

                _logger.LogInformation("Fetching plan details for PlanId: {PlanId}", model.PlanId);
                var plan = await _context.Plans.FindAsync((ulong)model.PlanId);
                if (plan != null)
                {
                    model.Plan = plan.Data ?? ""; // ✅ FIXED (Null Check)
                }

                _logger.LogInformation("Saving user to database...");
                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Email}", model.Email);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                return (false, "An internal error occurred.");
            }
        }



        private string GenerateAuthKey()
        {
            string key;
            do
            {
                key = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            } while (_context.Users.Any(u => u.Authkey == key));

            return key;
        }
    }

}
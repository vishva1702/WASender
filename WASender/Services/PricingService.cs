using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Plan>> GetActivePlansAsync()
        {
            return await _context.Plans
                .Where(p => p.Status == 1) // Use Status instead of IsActive
                .ToListAsync();
        }


        public async Task<List<object>> GetFaqsAsync()
        {
            try
            {
                return await _context.Posts
                    .Where(p => p.Type == "faq" && p.Status == 1 && p.Slug != "top")
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => (object)new
                    {
                        p.Title,
                        p.Slug,
                        Excerpt = p.Postmetas
                         .Where(m => m.Key == "excerpt") // Assuming "excerpt" is stored as a key in Postmeta
                         .Select(m => m.Value)
                         .FirstOrDefault()
                    })

                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<object>();
            }
        }



        public async Task<List<object>> GetWhyChooseAsync()
        {
            // Implement the logic to fetch "Why Choose" data
            // This is a placeholder; replace with actual implementation
            return await Task.FromResult(new List<object>());
        }
    }
}
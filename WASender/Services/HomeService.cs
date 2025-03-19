using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WASender.Models;

namespace WASender.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;

        public HomeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JObject> GetHeroSectionAsync()
        {
            var option = await _context.Options.Where(o => o.Id == 16).Select(o => o.Value).FirstOrDefaultAsync();
            return !string.IsNullOrEmpty(option) ? JObject.Parse(option) : null;
        }

        public async Task<JObject> GetHeaderFooterAsync()
        {
            var option = await _context.Options.Where(o => o.Id == 15).Select(o => o.Value).FirstOrDefaultAsync();
            return !string.IsNullOrEmpty(option) ? JObject.Parse(option) : null;
        }

        public async Task<List<Category>> GetBrandsAsync()
        {
            return await _context.Categories.Where(c => c.Type == "brand" && c.Status == 1)
                                            .OrderByDescending(c => c.Id)
                                            .ToListAsync();
        }

        public async Task<List<Post>> GetTestimonialsAsync()
        {
            return await _context.Posts.Where(p => p.Type == "testimonial")
                                       .Include(p => p.Postmetas)
                                       .OrderByDescending(p => p.Id)
                                       .ToListAsync();
        }

        public async Task<List<object>> GetFaqsAsync()
        {
            var faqs = await _context.Posts
                .Where(p => p.Type == "faq" && p.Featured == 1 && p.Lang == "en")
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return faqs.Select(faq => (object)new
            {
                faq.Id,
                faq.Title,
                faq.Slug,
                Excerpt = faq.Postmetas.FirstOrDefault(m => m.Key == "excerpt")?.Value ?? ""
            }).ToList();
        }


        public async Task<JObject> GetHomePageDataAsync()
        {
            var option = await _context.Options.FirstOrDefaultAsync(o => o.Id == 16);
            return !string.IsNullOrEmpty(option?.Value) ? JObject.Parse(option.Value) : null;
        }

        public async Task<List<object>> GetFeaturesAsync()
        {
            var features = await _context.Posts
                .Where(p => p.Type == "feature" && p.Featured == 1 && p.Status == 1) // Fixed: Status is int
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync();

            return features.Select(feature => (object)new
            {
                feature.Id,
                feature.Title,
                feature.Slug,
                Preview = feature.Postmetas.FirstOrDefault(m => m.Key == "preview")?.Value ?? "",
                Excerpt = feature.Postmetas.FirstOrDefault(m => m.Key == "excerpt")?.Value ?? ""
            }).ToList();
        }

        public async Task<List<dynamic>> GetPricingPlansAsync()
{
    var option = await _context.Options.Where(o => o.Id == 16)
                                       .Select(o => o.Value)
                                       .FirstOrDefaultAsync();
    if (string.IsNullOrEmpty(option))
        return new List<dynamic>();

    var pricingData = JObject.Parse(option);
    var plans = pricingData["plans"]?.ToObject<List<dynamic>>();

    return plans ?? new List<dynamic>();
}

    }
}

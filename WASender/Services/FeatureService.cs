using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WASender.Models;

namespace WASender.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly ApplicationDbContext _context;

        public FeatureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<object>> GetFeaturesAsync()
        {
            var features = await _context.Posts
                .Where(p => p.Type == "feature" && p.Status == 1 && p.Lang == "en")
                .Select(p => new
                {
                    Post = p,
                    Preview = p.Postmetas.FirstOrDefault(m => m.Key == "preview"),
                    Excerpt = p.Postmetas.FirstOrDefault(m => m.Key == "excerpt")
                })
                .OrderByDescending(p => p.Post.CreatedAt)
                .ToListAsync();

            return features.Select(f => new
            {
                f.Post,
                Preview = f.Preview?.Value,
                Excerpt = f.Excerpt?.Value
            }).Cast<object>().ToList();
        }

        public async Task<object> GetFeatureDetailsAsync(string slug)
        {
            var feature = await _context.Posts
                .Where(p => p.Type == "feature" && p.Lang == "en" && p.Slug == slug)
                .Select(p => new
                {
                    Post = p,
                    Preview = p.Postmetas.FirstOrDefault(m => m.Key == "preview"),
                    Excerpt = p.Postmetas.FirstOrDefault(m => m.Key == "excerpt"),
                    LongDescription = p.Postmetas.FirstOrDefault(m => m.Key == "main_description"),
                    Banner = p.Postmetas.FirstOrDefault(m => m.Key == "banner")
                })
                .FirstOrDefaultAsync();

            return feature == null ? null : new
            {
                feature.Post,
                Preview = feature.Preview?.Value,
                Excerpt = feature.Excerpt?.Value,
                LongDescription = feature.LongDescription?.Value,
                Banner = feature.Banner?.Value
            };
        }

        public async Task<List<object>> GetFaqsAsync()
        {
            var faqs = await _context.Posts
                .Where(p => p.Type == "faq" && p.Status == 1 && p.Lang == "en")
                .Select(p => new
                {
                    Post = p,
                    Excerpt = p.Postmetas.FirstOrDefault(m => m.Key == "excerpt")
                })
                .OrderByDescending(p => p.Post.CreatedAt)
                .ToListAsync();

            return faqs.Select(f => new
            {
                f.Post.Title,
                Excerpt = f.Excerpt?.Value
            }).Cast<object>().ToList();
        }

        public async Task<List<object>> GetWhyChooseAsync()
        {
            // Implement logic if `WhyChoose` is retrieved from DB
            return new List<object>();
        }
    }
}

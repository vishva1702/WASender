using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WASender.Models;

namespace WASender.Services
{
    public class AboutService : IAboutService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AboutService> _logger;

        public AboutService(ApplicationDbContext context, ILogger<AboutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LoadAboutDataAsync(ViewDataDictionary viewData)
        {
            var option = await _context.Options
                .Where(o => o.Key == "about" && o.Lang == "en")
                .FirstOrDefaultAsync();

            if (option != null && !string.IsNullOrEmpty(option.Value))
            {
                using var doc = JsonDocument.Parse(option.Value);
                var root = doc.RootElement;

                viewData["AboutImage1"] = root.GetProperty("about_image_1").GetString();
                viewData["AboutImage2"] = root.GetProperty("about_image_2").GetString();
                viewData["BreadcrumbTitle"] = root.GetProperty("breadcrumb_title").GetString();
                viewData["SectionTitle"] = root.GetProperty("section_title").GetString();
                viewData["ExperienceTitle"] = root.GetProperty("experience_title").GetString();
                viewData["Experience"] = root.GetProperty("experience").GetString();
                viewData["ButtonTitle"] = root.GetProperty("button_title").GetString();
                viewData["ButtonLink"] = root.GetProperty("button_link").GetString();
                viewData["IntroducingVideo"] = root.GetProperty("introducing_video").GetString();

                viewData["Descriptions"] = root.TryGetProperty("description", out var descriptionElement) &&
                                           descriptionElement.ValueKind == JsonValueKind.String
                    ? descriptionElement.GetString()?.Split("<br>").Select(d => d.Trim()).ToList()
                    : new List<string>();

                viewData["Facilities"] = root.TryGetProperty("facilities", out var facilitiesElement) &&
                                         facilitiesElement.ValueKind == JsonValueKind.String
                    ? facilitiesElement.GetString()?.Split(",").Select(f => f.Trim()).ToList()
                    : new List<string>();
            }
        }

        public async Task LoadCounterDataAsync(ViewDataDictionary viewData)
        {
            var counterOption = await _context.Options
                .Where(o => o.Key == "counter" && o.Lang == "en")
                .FirstOrDefaultAsync();

            if (counterOption != null && !string.IsNullOrEmpty(counterOption.Value))
            {
                using var doc = JsonDocument.Parse(counterOption.Value);
                var root = doc.RootElement;

                viewData["Experience"] = root.GetProperty("experience").GetString();
                viewData["ActiveCustomers"] = root.GetProperty("active_customers").GetString();
                viewData["PositiveReviews"] = root.GetProperty("positive_reviews").GetString();
                viewData["SatisfiedCustomers"] = root.GetProperty("satisfied_customers").GetString();
            }
        }

        public async Task LoadFaqDataAsync(ViewDataDictionary viewData)
        {
            try
            {
                var faqs = await _context.Posts
                    .Where(p => p.Type == "faq" && p.Status == 1 && p.Slug != "top")
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Title,
                        p.Slug,
                        Excerpt = p.Postmetas.FirstOrDefault(pm => pm.Key == "excerpt").Value
                    })
                    .ToListAsync();

                viewData["Faqs"] = faqs;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching FAQ data: {ex}");
                viewData["Faqs"] = new List<object>();
            }
        }

        public async Task<List<Plan>> GetPlansAsync()
        {
            return await _context.Plans.Where(p => p.Status == 1).ToListAsync();
        }

        public async Task<List<object>> GetFeaturesAsync()
        {
            var features = await _context.Posts
                .Where(p => p.Type == "feature" && p.Status == 1 && p.Lang == "en")
                .OrderBy(p => p.CreatedAt)
                .Take(3)
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
            }).ToList<object>();
        }

        public async Task<List<object>> GetTeamMembersAsync()
        {
            return await _context.Posts
                .Where(p => p.Type == "team" && p.Lang == "en")
                .Select(p => new
                {
                    Post = p,
                    Excerpt = p.Postmetas.FirstOrDefault(m => m.Key == "excerpt")!.Value,
                    Description = p.Postmetas.FirstOrDefault(m => m.Key == "description")!.Value,
                    Preview = p.Postmetas.FirstOrDefault(m => m.Key == "preview")!.Value
                })
                .ToListAsync<object>();
        }

        public async Task<object> GetTeamMemberDetailsAsync(string slug)
        {
            return await _context.Posts
                .Where(p => p.Type == "team" && p.Lang == "en" && p.Slug == slug)
                .Select(p => new
                {
                    Post = p,
                    Preview = p.Postmetas.FirstOrDefault(m => m.Key == "preview") != null
                        ? p.Postmetas.FirstOrDefault(m => m.Key == "preview").Value
                        : null
                })
                .FirstOrDefaultAsync();
        }

    }
}

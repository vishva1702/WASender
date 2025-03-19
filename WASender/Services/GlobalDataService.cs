using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public class GlobalDataService : IGlobalDataService
    {
        private readonly ApplicationDbContext _context;

        public GlobalDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LoadGlobalDataAsync(ViewDataDictionary viewData, ILogger logger)
        {
            logger.LogInformation("Loading global data (header, footer, and primary data).");

            var headerFooterOption = await _context.Options
                .Where(o => o.Key == "header_footer" && o.Lang == "en")
                .FirstOrDefaultAsync();

            var primaryOption = await _context.Options
                .Where(o => o.Key == "primary_data" && o.Lang == "en")
                .FirstOrDefaultAsync();

            if (headerFooterOption != null && !string.IsNullOrEmpty(headerFooterOption.Value))
            {
                logger.LogInformation("Parsing header and footer data.");
                using var doc = JsonDocument.Parse(headerFooterOption.Value);
                var root = doc.RootElement;

                if (root.TryGetProperty("header", out var header))
                {
                    viewData["AnnouncementType"] = header.GetProperty("announcement_type").GetString();
                    viewData["AnnouncementTitle"] = header.GetProperty("announcement_title").GetString();
                    viewData["AnnouncementLink"] = header.GetProperty("announcement_link").GetString();
                }

                if (root.TryGetProperty("footer", out var footer))
                {
                    viewData["FooterTitle"] = footer.GetProperty("title").GetString();
                    viewData["FooterDescription"] = footer.GetProperty("description").GetString();
                    viewData["RightImageLink"] = footer.GetProperty("right_image_link").GetString();
                    viewData["LeftImageLink"] = footer.GetProperty("left_image_link").GetString();
                }

                viewData["FooterButtonImage"] = root.GetProperty("footer_button_image").GetString();
                viewData["FooterLeftButtonImage"] = root.GetProperty("footer_left_button_image").GetString();
            }
            else
            {
                logger.LogWarning("No header/footer data found.");
                viewData["AnnouncementType"] = "";
                viewData["AnnouncementTitle"] = "";
                viewData["AnnouncementLink"] = "#";
                viewData["FooterTitle"] = "";
                viewData["FooterDescription"] = "";
                viewData["RightImageLink"] = "#";
                viewData["LeftImageLink"] = "#";
                viewData["FooterButtonImage"] = "";
                viewData["FooterLeftButtonImage"] = "";
            }

            if (primaryOption != null && !string.IsNullOrEmpty(primaryOption.Value))
            {
                logger.LogInformation("Parsing primary data.");
                using var doc = JsonDocument.Parse(primaryOption.Value);
                var root = doc.RootElement;

                viewData["Logo"] = root.GetProperty("logo").GetString();
                viewData["Favicon"] = root.GetProperty("favicon").GetString();
                viewData["ContactEmail"] = root.GetProperty("contact_email").GetString();
                viewData["ContactPhone"] = root.GetProperty("contact_phone").GetString();
                viewData["Address"] = root.GetProperty("address").GetString();
                viewData["FooterLogo"] = root.GetProperty("footer_logo").GetString();

                if (root.TryGetProperty("socials", out var socials))
                {
                    viewData["Facebook"] = socials.GetProperty("facebook").GetString();
                    viewData["YouTube"] = socials.GetProperty("youtube").GetString();
                    viewData["Twitter"] = socials.GetProperty("twitter").GetString();
                    viewData["Instagram"] = socials.GetProperty("instagram").GetString();
                    viewData["LinkedIn"] = socials.GetProperty("linkedin").GetString();
                }
            }
            else
            {
                logger.LogWarning("No primary data found.");
                viewData["Logo"] = "/default-logo.png";
                viewData["Favicon"] = "/default-favicon.png";
                viewData["ContactEmail"] = "contact@default.com";
                viewData["ContactPhone"] = "0000000000";
                viewData["Address"] = "Default Address";
                viewData["FooterLogo"] = "/default-footer-logo.png";
            }

            viewData["Languages"] = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("en", "English"),
                new KeyValuePair<string, string>("fr", "Français")
            };
            viewData["Locale"] = "en";
        }

        public async Task<List<Plan>> GetActivePlansAsync()
        {
            return await _context.Plans
                .Where(p => p.Status == 1)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<string, string>> WhyChooseAsync()
        {
            var option = await _context.Options
                .Where(o => o.Key == "why-choose" && o.Lang == "en")
                .Select(o => o.Value)
                .FirstOrDefaultAsync();

            return option != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(option) : new Dictionary<string, string>();
        }
    }
}

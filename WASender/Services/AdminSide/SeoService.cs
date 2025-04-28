using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WASender.Contracts.AdminSide;
using WASender.Models;

namespace WASender.Services.AdminSide
{
    public class SeoService : ISeoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SeoService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<dynamic>> GetSeoOptionsAsync()
        {
            var options = await _context.Options
                .Where(o => o.Key.Contains("seo"))
                .ToListAsync();

            return options.Select(o => new
            {
                Id = o.Id,
                Key = o.Key.Replace("seo_", "").Replace("_", " "),
                Content = JsonSerializer.Deserialize<Dictionary<string, string>>(o.Value)
            }).ToList<dynamic>();
        }

        public async Task<Option?> GetSeoOptionByIdAsync(ulong id)
        {
            return await _context.Options.FirstOrDefaultAsync(o => o.Id == id && o.Key.Contains("seo"));
        }

        public Task<Dictionary<string, string>> GetSeoMetadataAsync(Option option)
        {
            var contents = JsonSerializer.Deserialize<Dictionary<string, string>>(option.Value ?? "") ?? new Dictionary<string, string>();
            return Task.FromResult(contents);
        }

        public async Task<string> UpdateSeoAsync(ulong id, IFormFile? image, Dictionary<string, string> metadata)
        {
            var option = await _context.Options.FirstOrDefaultAsync(o => o.Id == id);
            if (option == null) return "SEO entry not found.";

            var contents = JsonSerializer.Deserialize<Dictionary<string, string>>(option.Value ?? "") ?? new Dictionary<string, string>();

            if (image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Delete old image
                if (contents.TryGetValue("preview", out string? oldImagePath) && !string.IsNullOrEmpty(oldImagePath))
                {
                    var oldFullPath = Path.Combine(_environment.WebRootPath, "uploads", oldImagePath);
                    if (System.IO.File.Exists(oldFullPath))
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }

                metadata["preview"] = fileName;
            }

            option.Value = JsonSerializer.Serialize(metadata);
            await _context.SaveChangesAsync();

            return "SEO settings updated successfully.";
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;

namespace WASender.Controllers.AdminSide
{
    public class AdminFeaturesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFeaturesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get features
            var posts = await _context.Posts
                .Where(p => p.Type == "feature")
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get languages (simulate fetching from settings or config)
            var languages = new Dictionary<string, string>
        {
            { "en", "English" },
            { "fr", "French" },
            { "es", "Spanish" }
        };

            ViewBag.Posts = posts;
            ViewData["Languages"] = languages;

            return View();
        }


        [HttpGet]
        public IActionResult Create()
        {
            var languages = GetOption("languages");
            ViewBag.Languages = languages;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Store(IFormCollection form)
        {
            try
            {
                var title = form["title"];
                var description = form["description"];
                var mainDescription = form["main_description"];
                var lang = form["language"].FirstOrDefault() ?? "en";
                var status = form["status"].Count > 0 ? 1 : 0;
                var featured = form["featured"].Count > 0 ? 1 : 0;

                var previewImage = Request.Form.Files["preview_image"];
                var bannerImage = Request.Form.Files["banner_image"];

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(mainDescription) || previewImage == null || bannerImage == null)
                {
                    return BadRequest("Validation failed.");
                }

                var post = new Post
                {
                    Title = title,
                    Slug = GenerateSlug(title),
                    Type = "feature",
                    Lang = lang,
                    Status = status,
                    Featured = featured,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                var metas = new List<Postmeta>
            {
                new Postmeta { PostId = post.Id, Key = "excerpt", Value = description },
                new Postmeta { PostId = post.Id, Key = "main_description", Value = mainDescription }
            };

                var previewPath = await FileUploader.SaveFile(previewImage);
                var bannerPath = await FileUploader.SaveFile(bannerImage);

                metas.Add(new Postmeta { PostId = post.Id, Key = "preview", Value = previewPath });
                metas.Add(new Postmeta { PostId = post.Id, Key = "banner", Value = bannerPath });

                _context.Postmetas.AddRange(metas);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    redirect = Url.Action("Index", "Features", new { area = "Admin" }),
                    message = "Feature created successfully..."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Dummy implementation - replace with actual logic
        private Dictionary<string, string> GetOption(string key)
        {
            return new Dictionary<string, string>
        {
            { "en", "English" },
            { "fr", "French" },
            { "hi", "Hindi" }
        };
        }

        private string GenerateSlug(string input)
        {
            return input.ToLower().Replace(" ", "-").Replace(".", "").Replace(",", "").Trim('-');
        }
    }
}

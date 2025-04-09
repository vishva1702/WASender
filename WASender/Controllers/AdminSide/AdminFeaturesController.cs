using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            { "en", "English" }
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
                    TempData["Error"] = "Please fill in all required fields.";
                    return RedirectToAction("Create");
                }

                var post = new Post
                {
                    Title = title,
                    Slug = GenerateSlug(title),
                    Type = "feature",
                    Lang = lang,
                    Status = status, // ✅ fixed here
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

                TempData["Success"] = "Feature created successfully.";
                return RedirectToAction("Index", "AdminFeatures", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("Create");
            }
        }



        // Dummy implementation - replace with actual logic
        private Dictionary<string, string> GetOption(string key)
        {
            return new Dictionary<string, string>
        {
            { "en", "English" }
        };
        }

        private string GenerateSlug(string input)
        {
            return input.ToLower().Replace(" ", "-").Replace(".", "").Replace(",", "").Trim('-');
        }

        [HttpPost]
        public async Task<IActionResult> Destroy(ulong id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Postmetas)
                    .FirstOrDefaultAsync(p => p.Type == "feature" && p.Id == id);

                if (post == null)
                {
                    return Json(new { success = false, message = "Feature not found." });
                }

                // Remove associated images if they exist
                var previewMeta = post.Postmetas.FirstOrDefault(pm => pm.Key == "preview");
                var bannerMeta = post.Postmetas.FirstOrDefault(pm => pm.Key == "banner");

                if (previewMeta != null)
                {
                    FileUploader.RemoveFile(previewMeta.Value);
                }

                if (bannerMeta != null)
                {
                    FileUploader.RemoveFile(bannerMeta.Value);
                }

                // Remove postmetas and post
                _context.Postmetas.RemoveRange(post.Postmetas);
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    redirect = Url.Action("Index", "AdminFeatures"),
                    message = "Feature deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        public async Task<IActionResult> Edit(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "feature");

            if (post == null)
            {
                return NotFound();
            }

            var excerpt = post.Postmetas.FirstOrDefault(m => m.Key == "excerpt");
            var preview = post.Postmetas.FirstOrDefault(m => m.Key == "preview");
            var longDescription = post.Postmetas.FirstOrDefault(m => m.Key == "main_description");
            var banner = post.Postmetas.FirstOrDefault(m => m.Key == "banner");

            ViewBag.Info = post;
            ViewBag.Excerpt = excerpt?.Value;
            ViewBag.Preview = preview?.Value;
            ViewBag.LongDescription = longDescription?.Value;
            ViewBag.Banner = banner?.Value;
            ViewBag.Languages = GetOption("languages");
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ulong id, IFormCollection form, IFormFile? preview_image, IFormFile? banner_image)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var title = form["title"];
            var description = form["description"];
            var mainDescription = form["main_description"];
            var language = form["language"];
            var status = form["status"].Count > 0;
            var featured = form["featured"].Count > 0;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                ModelState.AddModelError(string.Empty, "Title and description are required.");
                return View("Edit");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                post.Title = title!;
                post.Slug = GenerateSlug(title!); // Implement your slug logic
                post.Type = "feature";
                post.Lang = string.IsNullOrEmpty(language) ? "en" : language!;
                post.Status = status ? 1 : 0;
                post.Featured = featured ? 1 : 0;

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                await UpdateOrCreatePostmeta(post.Id, "excerpt", description!);
                await UpdateOrCreatePostmeta(post.Id, "main_description", mainDescription!);

                if (preview_image != null)
                {
                    var path = await FileUploader.SaveFile(preview_image);
                    await UpdateOrCreatePostmeta(post.Id, "preview", path);
                }

                if (banner_image != null)
                {
                    var path = await FileUploader.SaveFile(banner_image);
                    await UpdateOrCreatePostmeta(post.Id, "banner", path);
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Feature updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        private async Task UpdateOrCreatePostmeta(ulong postId, string key, string value)
        {
            var meta = await _context.Postmetas
                .FirstOrDefaultAsync(m => m.PostId == postId && m.Key == key);

            if (meta != null)
            {
                meta.Value = value;
                _context.Postmetas.Update(meta);
            }
            else
            {
                _context.Postmetas.Add(new Postmeta
                {
                    PostId = postId,
                    Key = key,
                    Value = value
                });
            }

            await _context.SaveChangesAsync();
        }


    }
}

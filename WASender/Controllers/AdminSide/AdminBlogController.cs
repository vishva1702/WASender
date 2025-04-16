using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using WASender.Models;

namespace WASender.Controllers.AdminSide
{
    public class AdminBlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var postsQuery = _context.Posts
                .Include(p => p.Postmetas)
                .Where(p => p.Type == "blog")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(search));
            }

            var posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.TotalPosts = await _context.Posts.CountAsync(p => p.Type == "blog");
            ViewBag.TotalActivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 1);
            ViewBag.TotalInActivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 0);

            // Check for success/error messages
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            return View(posts);
        }

         
        // GET: admin/blog/create
        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await _context.Categories
                .Where(c => c.Type == "tags")
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Status == 1)
                .ToListAsync();

            // You can replace this with your actual settings logic
            ViewBag.Languages = new Dictionary<string, string>
{
    { "en", "English" },
    
};


            return View();
        }

        // POST: admin/blog/store
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Store(Post post, IFormFile preview, IFormFile meta_image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set basic post properties
                    post.Type = "blog";
                    post.Status = Request.Form["status"] == "1" ? 1 : 0;
                    post.Featured = Request.Form["featured"] == "1" ? 1 : 0;
                    post.CreatedAt = DateTime.Now;

                    // Generate slug if empty
                    if (string.IsNullOrEmpty(post.Slug))
                    {
                        post.Slug = GenerateSlug(post.Title);
                    }

                    _context.Add(post);
                    await _context.SaveChangesAsync();

                    // Handle file uploads and metadata
                    var metas = new List<Postmeta>
            {
                new Postmeta { PostId = post.Id, Key = "short_description", Value = Request.Form["short_description"] },
                new Postmeta { PostId = post.Id, Key = "main_description", Value = Request.Form["main_description"] },
                new Postmeta { PostId = post.Id, Key = "meta_title", Value = Request.Form["meta_title"] },
                new Postmeta { PostId = post.Id, Key = "meta_description", Value = Request.Form["meta_description"] },
                new Postmeta { PostId = post.Id, Key = "meta_tags", Value = Request.Form["meta_tags"] }
            };

                    // Upload preview image
                    if (preview != null && preview.Length > 0)
                    {
                        var previewPath = await UploadFile(preview, "blog/preview");
                        metas.Add(new Postmeta { PostId = post.Id, Key = "preview", Value = previewPath });
                    }

                    // Upload meta image
                    if (meta_image != null && meta_image.Length > 0)
                    {
                        var metaImagePath = await UploadFile(meta_image, "blog/meta");
                        metas.Add(new Postmeta { PostId = post.Id, Key = "meta_image", Value = metaImagePath });
                    }

                    // Handle categories and tags
                    var categoryIds = Request.Form["categories"].ToList();
                    if (categoryIds.Any())
                    {
                        metas.Add(new Postmeta
                        {
                            PostId = post.Id,
                            Key = "category_ids",
                            Value = string.Join(",", categoryIds)
                        });
                    }

                    var tagIds = Request.Form["tags"].ToList();
                    if (tagIds.Any())
                    {
                        metas.Add(new Postmeta
                        {
                            PostId = post.Id,
                            Key = "tag_ids",
                            Value = string.Join(",", tagIds)
                        });
                    }

                    await _context.Postmetas.AddRangeAsync(metas);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Blog post created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed
                ViewBag.Tags = await _context.Categories
                    .Where(c => c.Type == "tags")
                    .ToListAsync();

                ViewBag.Categories = await _context.Categories
                    .Where(c => c.Type == "blog_category" && c.Status == 1)
                    .ToListAsync();

                ViewBag.Languages = new Dictionary<string, string> { { "en", "English" } };

                return View("Create", post);
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "An error occurred while creating the blog post.";
                return RedirectToAction(nameof(Create));
            }
        }

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", folder, uniqueFileName);
        }

        private string GenerateSlug(string title)
        {
            var slug = title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace(".", "-")
                .Replace(",", "-");

            // Remove invalid chars
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces into one space
            slug = Regex.Replace(slug, @"\s+", " ").Trim();

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s", "-");

            return slug;
        }

        // GET: admin/blog/edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "blog");

            if (post == null)
                return NotFound();

            // Convert Postmetas to Dictionary
            var metaDict = post.Postmetas.ToDictionary(pm => pm.Key, pm => pm.Value);

            ViewBag.ShortDescription = metaDict.ContainsKey("short_description") ? metaDict["short_description"] : "";
            ViewBag.LongDescription = metaDict.ContainsKey("long_description") ? metaDict["long_description"] : "";

            ViewBag.Seo = new Dictionary<string, string>
    {
        { "title", metaDict.ContainsKey("meta_title") ? metaDict["meta_title"] : "" },
        { "description", metaDict.ContainsKey("meta_description") ? metaDict["meta_description"] : "" },
        { "tags", metaDict.ContainsKey("meta_tags") ? metaDict["meta_tags"] : "" }
    };

            // Selected Categories and Tags from postmeta
            ViewBag.SelectedCategoryIds = metaDict.ContainsKey("category_ids")
                ? metaDict["category_ids"].Split(',').Select(ulong.Parse).ToList()
                : new List<ulong>();

            ViewBag.SelectedTagIds = metaDict.ContainsKey("tag_ids")
                ? metaDict["tag_ids"].Split(',').Select(ulong.Parse).ToList()
                : new List<ulong>();

            // Dropdown data
            ViewBag.Tags = (await _context.Categories
                .Where(c => c.Type == "tags")
                .ToDictionaryAsync(c => c.Id, c => c.Title));

            ViewBag.Categories = (await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Status == 1)
                .ToDictionaryAsync(c => c.Id, c => c.Title));

            ViewBag.Languages = new Dictionary<string, string>
    {
        { "en", "English" },
        { "gu", "Gujarati" },
        { "hi", "Hindi" }
    };

            return View(post);
        }


        // POST: admin/blog/update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ulong id, Post updatedPost)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                post.Title = updatedPost.Title;
                post.Status = updatedPost.Status;
                post.Slug = updatedPost.Slug;
                // ... update other fields

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("Edit", post);
        }

        // POST: admin/blog/delete/5
        [HttpPost]
        public async Task<IActionResult> Destroy(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "blog");

            if (post == null)
                return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Json(new { message = "Blog deleted successfully." });
        }

        // POST: admin/blog/massdestroy
        [HttpPost]
        public async Task<IActionResult> MassDestroy(ulong[] ids)
        {
            var posts = await _context.Posts
                .Where(p => ids.Contains(p.Id) && p.Type == "blog")
                .ToListAsync();

            _context.Posts.RemoveRange(posts);
            await _context.SaveChangesAsync();

            return Json(new { message = "Selected blogs deleted successfully." });
        }

    }
}

   
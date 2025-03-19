using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers
{
    public class BlogsController : BaseController
    {
        private readonly IBlogService _blogService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogsController> _logger;

        public BlogsController(IGlobalDataService globalDataService, IBlogService blogService, ApplicationDbContext context, ILogger<BlogsController> logger)
            : base(globalDataService, logger)
        {
            _blogService = blogService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string search)
        {
            await LoadGlobalDataAsync();

            var result = await _blogService.GetBlogsAsync(search);
            ViewBag.Blogs = result.Blogs;
            ViewBag.RecentBlogs = result.RecentBlogs;
            ViewBag.Categories = result.Categories;
            ViewBag.Tags = result.Tags;

            return View("Index");
        }

        public async Task<IActionResult> Details(string slug)
        {
            await LoadGlobalDataAsync();

            if (string.IsNullOrEmpty(slug))
                return BadRequest("Invalid blog slug.");

            var blog = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Type == "blog" && p.Slug == slug && p.Status == 1);

            if (blog == null)
            {
                _logger.LogWarning($"Blog not found for slug: {slug}");
                return NotFound("Blog not found.");
            }

            var recentBlogs = await _blogService.GetBlogsAsync(null);
            var categories = await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Lang == "en")
                .Take(3)
                .ToListAsync();

            var tags = await _context.Categories
                .Where(c => c.Type == "tags" && c.Lang == "en")
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.RecentBlogs = recentBlogs.RecentBlogs;
            ViewBag.Tags = tags;

            return View("Details", blog); // Pass blog as the model
        }


        public async Task<IActionResult> Category(string slug)
        {
            await LoadGlobalDataAsync();

            if (string.IsNullOrEmpty(slug))
                return BadRequest("Invalid category slug.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug && c.Type == "blog_category" && c.Lang == "en");

            if (category == null)
            {
                _logger.LogWarning($"Category not found for slug: {slug}");
                return NotFound("Category not found.");
            }

            var postIds = await _context.Postcategories
                .Where(pc => pc.CategoryId == category.Id)
                .Select(pc => pc.PostId)
                .ToListAsync();

            var blogs = await _context.Posts
                .Where(p => postIds.Contains(p.Id) && p.Status == 1)
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!blogs.Any())
            {
                _logger.LogInformation($"No blogs found for category: {slug}");
                return NotFound("No blogs found for this category.");
            }

            var recentBlogs = await _blogService.GetBlogsAsync(null);
            var categories = await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Lang == "en")
                .Take(3)
                .ToListAsync();

            var tags = await _context.Categories
                .Where(c => c.Type == "tags" && c.Lang == "en")
                .ToListAsync();

            // Pass the data using ViewBag
            ViewBag.Blogs = blogs;
            ViewBag.Category = category;
            ViewBag.Categories = categories;
            ViewBag.RecentBlogs = recentBlogs.RecentBlogs;
            ViewBag.Tags = tags;

            return View("Index", blogs);
        }

        public async Task<IActionResult> Tag(string slug)
        {
            await LoadGlobalDataAsync();

            if (string.IsNullOrEmpty(slug))
                return BadRequest("Invalid tag slug.");

            // Check if the clicked tag is "support"
            if (slug.ToLower() == "support")
            {
                ViewBag.BlogNotFoundMessage = "Blog Post Not Found";
                return View("Index"); // Stay on the same page
            }

            var tag = await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug && c.Type == "tags" && c.Lang == "en");

            if (tag == null)
            {
                _logger.LogWarning($"Tag not found for slug: {slug}");
                return NotFound("Tag not found.");
            }

            var postIds = await _context.Postcategories
                .Where(pc => pc.CategoryId == tag.Id)
                .Select(pc => pc.PostId)
                .ToListAsync();

            var blogs = await _context.Posts
                .Where(p => postIds.Contains(p.Id) && p.Status == 1)
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!blogs.Any())
            {
                _logger.LogInformation($"No blogs found for tag: {slug}");
                return NotFound("No blogs found for this tag.");
            }

            var recentBlogs = await _blogService.GetBlogsAsync(null);
            var categories = await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Lang == "en")
                .Take(3)
                .ToListAsync();

            var tags = await _context.Categories
                .Where(c => c.Type == "tags" && c.Lang == "en")
                .ToListAsync();

            // Pass the data using ViewBag
            ViewBag.Blogs = blogs;
            ViewBag.Tag = tag;
            ViewBag.Categories = categories;
            ViewBag.RecentBlogs = recentBlogs.RecentBlogs;
            ViewBag.Tags = tags;

            return View("Index", blogs);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Helpers;

namespace WASender.Controllers.Admin
{
    [Route("admin/[controller]")]
    public class AdminBlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlogHelper _blogHelper;

        public AdminBlogController(ApplicationDbContext context, BlogHelper blogHelper)
        {
            _context = context;
            _blogHelper = blogHelper;
        }

        // GET: admin/blogs
        [HttpGet]
        public async Task<IActionResult> Index(string search = "", string type = "")
        {
            var postsQuery = _context.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(search));
            }

            var posts = await postsQuery
                .Where(p => p.Type == "blog")
                .Include(p => p.Postmetas) // 🛟 Make sure Postmetas are included
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.TotalPosts = await _context.Posts.CountAsync(p => p.Type == "blog");
            ViewBag.TotalActivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 1);
            ViewBag.TotalInactivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 0);
            ViewBag.Search = search;

            return View("Index", posts);
        }

        // GET: admin/blogs/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await _context.Categories.Where(c => c.Type == "tags").ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.Type == "blog_category" && c.Status == 1).ToListAsync();
            ViewBag.Languages = new[] { "en", "fr", "es" };

            return View("Create");
        }

        // POST: admin/blogs
        [HttpPost("store")]
        public async Task<IActionResult> Store([FromForm] IFormCollection request)
        {
            try
            {
                await _blogHelper.CreateAsync(request);
                TempData["Success"] = "Blog Created Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Create");
            }
        }

        // GET: admin/blogs/{id}/edit
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "blog");

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.Tags = await _context.Categories.Where(c => c.Type == "tags").ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => c.Type == "blog_category").ToListAsync();

            ViewBag.SelectedCategories = post.Postmetas
                .Where(m => m.Key == "categories")
                .Select(m => m.Value)
                .ToList();

            var seoMeta = post.Postmetas.FirstOrDefault(m => m.Key == "seo");
            ViewBag.Seo = seoMeta != null
                ? Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(seoMeta.Value)
                : new Dictionary<string, string>();

            ViewBag.Languages = new[] { "en", "fr", "es" };

            return View("Edit", post);
        }

        // PUT: admin/blogs/{id}
        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(ulong id, [FromForm] IFormCollection request)
        {
            try
            {
                await _blogHelper.UpdateAsync(request, id);
                TempData["Success"] = "Blog Updated Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Edit", new { id });
            }
        }

        // POST: admin/blogs/mass-destroy
        [HttpPost("mass-destroy")]
        public async Task<IActionResult> MassDestroy([FromForm] List<ulong> ids)
        {
            try
            {
                var posts = await _context.Posts.Where(p => ids.Contains(p.Id)).ToListAsync();
                _context.Posts.RemoveRange(posts);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Selected Blog Posts Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}

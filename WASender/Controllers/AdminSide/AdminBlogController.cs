using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
                .Where(p => p.Type == "blog");

            if (!string.IsNullOrEmpty(search))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(search));
            }

            var posts = await postsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();

            ViewBag.TotalPosts = await _context.Posts.CountAsync(p => p.Type == "blog");
            ViewBag.TotalActivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 1);
            ViewBag.TotalInActivePosts = await _context.Posts.CountAsync(p => p.Type == "blog" && p.Status == 0);

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
            ViewBag.Languages = new[] { "en", "gu", "hi" };

            return View();
        }

        // POST: admin/blog/store
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(Post post)
        {
            if (ModelState.IsValid)
            {
                post.Type = "blog";
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("Create", post);
        }

        // GET: admin/blog/edit/5
        public async Task<IActionResult> Edit(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "blog");

            if (post == null)
                return NotFound();

            ViewBag.Tags = await _context.Categories
                .Where(c => c.Type == "tags")
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Type == "blog_category")
                .ToListAsync();

            ViewBag.Languages = new[] { "en", "gu", "hi" };

            return View(post);
        }

        // POST: admin/blog/update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Post updatedPost)
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

   
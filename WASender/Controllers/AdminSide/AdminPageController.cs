using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WASender.Controllers.AdminSide
{
    public class AdminPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminPage/
        public async Task<IActionResult> Index()
        {
            var pages = await _context.Posts
                .Where(p => p.Type == "page")
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var totalActivePosts = await _context.Posts.CountAsync(p => p.Type == "page" && p.Status == 1);
            var totalInActivePosts = await _context.Posts.CountAsync(p => p.Type == "page" && p.Status == 0);
            var totalPosts = await _context.Posts.CountAsync(p => p.Type == "page");

            ViewBag.TotalActivePosts = totalActivePosts;
            ViewBag.TotalInActivePosts = totalInActivePosts;
            ViewBag.TotalPosts = totalPosts;

            return View(pages);
        }

        // GET: /AdminPage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /AdminPage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model)
        {
            if (ModelState.IsValid)
            {
                _context.Posts.Add(model);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    message = "Page Created Successfully",
                    redirect = Url.Action("Index")
                });
            }

            return BadRequest(ModelState);
        }

        // GET: /AdminPage/Edit/5
        public async Task<IActionResult> Edit(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var seoMeta = post.Postmetas.FirstOrDefault(m => m.Key == "seo");
            ViewBag.Seo = seoMeta != null ? seoMeta.Value : "";

            return View(post);
        }

        // POST: /AdminPage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, Post model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Posts.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        message = "Page Updated Successfully",
                        redirect = Url.Action("Index")
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
            }

            return BadRequest(ModelState);
        }

        // POST: /AdminPage/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // optional: clear cache here if implemented
            return Json(new
            {
                message = "Page Deleted Successfully",
                redirect = Url.Action("Index")
            });
        }
    }
}

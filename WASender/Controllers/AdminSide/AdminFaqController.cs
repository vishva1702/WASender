using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Controllers.Admin
{
    [Route("AdminFaq")]
    public class FaqController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FaqController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var faqs = await _context.Posts
                .Where(p => p.Type == "faq")
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Languages = new Dictionary<string, string>
    {
        { "en", "English" },
      
        // Add more if needed
    };

            return View("~/Views/AdminFaq/Index.cshtml", faqs);
        }


        [HttpPost("store")]
        public async Task<IActionResult> Store(string question, string answer, string language, string position = "bottom")
        {
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
            {
                return BadRequest("Question and Answer are required.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var post = new Post
                {
                    Title = question,
                    Slug = position,
                    Type = "faq",
                    Lang = language ?? "en",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                var postmeta = new Postmeta
                {
                    PostId = post.Id,
                    Key = "excerpt",
                    Value = answer
                };

                _context.Postmetas.Add(postmeta);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [FromForm] string question, [FromForm] string answer, [FromForm] string position, [FromForm] string language)
        {
            if (string.IsNullOrWhiteSpace(question) || question.Length > 150)
            {
                return StatusCode(400, new { message = "Question is required and must be max 150 characters." });
            }

            if (string.IsNullOrWhiteSpace(answer) || answer.Length > 500)
            {
                return StatusCode(400, new { message = "Answer is required and must be max 500 characters." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var post = await _context.Posts
                    .Include(p => p.Postmetas)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                    return NotFound(new { message = "Post not found." });

                post.Title = question;
                post.Slug = string.IsNullOrEmpty(position) ? "bottom" : position;
                post.Type = "faq";
                post.Lang = string.IsNullOrEmpty(language) ? "en" : language;
                post.UpdatedAt = DateTime.UtcNow;

                _context.Posts.Update(post);

                var excerptMeta = post.Postmetas.FirstOrDefault(m => m.Key == "excerpt");

                if (excerptMeta != null)
                {
                    excerptMeta.Value = answer;
                    _context.Postmetas.Update(excerptMeta);
                }
                else
                {
                    _context.Postmetas.Add(new Postmeta
                    {
                        PostId = post.Id,
                        Key = "excerpt",
                        Value = answer
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    redirect = Url.Action("Index", "Faq", new { area = "admin" }),
                    message = "FAQ updated successfully..."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPost("delete/{id}")]
        public IActionResult Delete(ulong id)

        {
            var post = _context.Posts.FirstOrDefault(p => p.Type == "faq" && p.Id == id);
            if (post == null)
            {
                return Json(new { success = false, message = "FAQ not found" });
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();

            return Json(new { success = true, message = "FAQ deleted successfully!" });
        }


    }
}

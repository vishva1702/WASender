<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminTeamController : BaseController
=======
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WASender.Models;

namespace WASender.Controllers.AdminSide
{
    public class AdminTeamController : Controller
>>>>>>> Dashboard
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

<<<<<<< HEAD
        public AdminTeamController(IGlobalDataService globalDataService,ILogger<BaseController> logger,ApplicationDbContext context,IWebHostEnvironment env)
          : base(globalDataService, logger) 
=======

        public AdminTeamController(ApplicationDbContext context, IWebHostEnvironment env)
>>>>>>> Dashboard
        {
            _context = context;
            _env = env;
        }

<<<<<<< HEAD

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
=======
        public async Task<IActionResult> Index()
        {
>>>>>>> Dashboard
            try
            {
                // Check database connectivity
                if (!_context.Database.CanConnect())
                {
                    throw new Exception("Database connection failed.");
                }

                // Fetch the posts
                var posts = await _context.Posts
                    .Where(p => p.Type == "team")
                    .Include(p => p.Postmetas)
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

                // Debugging: Print the count of posts
                Console.WriteLine($"Total posts fetched: {posts.Count}");

                ViewData["TotalPosts"] = posts.Count;
                ViewData["TotalActivePosts"] = posts.Count(p => p.Status == 1);
                ViewData["TotalInactivePosts"] = posts.Count(p => p.Status != 1);

                return View(posts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Index method: {ex.Message}");
                return View("Error"); // Create an Error.cshtml if needed
            }
        }


        // Create Method (Display the form)
<<<<<<< HEAD
        public async Task<IActionResult> Create()
        {
           await LoadGlobalDataAsync();

=======
        public IActionResult Create()
        {
>>>>>>> Dashboard
            var post = new Post
            {
                Status = 0  // Default to inactive status
            };

            return View(post);
        }


        [HttpPost]
<<<<<<< HEAD
        public async Task<IActionResult> Store(IFormFile profile_picture, string member_name, string member_position, string about, [FromForm] string status, Dictionary<string, string> socials)
=======
        public async Task<IActionResult> Store(IFormFile profile_picture,string member_name,string member_position,string about,[FromForm] string status,Dictionary<string, string> socials)
>>>>>>> Dashboard
        {
            int parsedStatus = status == "1" ? 1 : 0; // Ensure correct parsing

            Console.WriteLine($"DEBUG: Received status = {status}, Parsed = {parsedStatus}");

            if (string.IsNullOrEmpty(member_name) || string.IsNullOrEmpty(member_position) || profile_picture == null)
            {
                ViewBag.ErrorMessage = "Please fill in all required fields.";
                return View("Create");
            }

            try
            {
                var post = new Post
                {
                    Title = member_name,
                    Slug = member_position,
<<<<<<< HEAD
                    Status = parsedStatus, // Initial status
=======
                    Status = parsedStatus, // Store correct status
>>>>>>> Dashboard
                    Type = "team",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

<<<<<<< HEAD
                // DEBUG: Check if DB default changed the status
                var initialStatus = post.Status;
                Console.WriteLine($"Status after first save: {initialStatus}");

                // If DB default changed it, manually override
                if (parsedStatus == 0 && initialStatus == 1)
                {
                    Console.WriteLine("Overriding DB default to set status=0");
                    post.Status = 0;
                    _context.Entry(post).Property(x => x.Status).IsModified = true;
                    await _context.SaveChangesAsync();
                }

=======
>>>>>>> Dashboard
                var socialJson = JsonSerializer.Serialize(socials ?? new Dictionary<string, string>());
                _context.Postmetas.Add(new Postmeta { PostId = post.Id, Key = "excerpt", Value = socialJson });
                _context.Postmetas.Add(new Postmeta { PostId = post.Id, Key = "description", Value = about });

                var previewPath = await FileUploader.SaveFile(profile_picture);
                _context.Postmetas.Add(new Postmeta { PostId = post.Id, Key = "preview", Value = previewPath });

                await _context.SaveChangesAsync();

<<<<<<< HEAD
                TempData["Success"] = "Team member created successfully.";
=======
>>>>>>> Dashboard
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("Create");
            }
        }



<<<<<<< HEAD

        // GET: Edit Team Member
        public async Task<IActionResult> Edit(ulong id)
        {
            await LoadGlobalDataAsync();

=======
        // GET: Edit Team Member
        public async Task<IActionResult> Edit(ulong id)
        {
>>>>>>> Dashboard
            var post = await _context.Posts
                .Where(p => p.Id == id && p.Type == "team") // Ensure fetching the correct type
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            // Extract metadata
            var description = post.Postmetas.FirstOrDefault(m => m.Key == "description")?.Value ?? "";
            var excerptJson = post.Postmetas.FirstOrDefault(m => m.Key == "excerpt")?.Value;
            var socials = !string.IsNullOrEmpty(excerptJson) ?
                          JsonSerializer.Deserialize<Dictionary<string, string>>(excerptJson) :
                          new Dictionary<string, string>();

            // Pass data to the view
            ViewBag.Description = description;
            ViewBag.Socials = socials;

            return View(post);  // ✅ Setting the model properly
        }


        [HttpPost]
        public async Task<IActionResult> Update(ulong id, IFormFile profile_picture, string member_name, string member_position, string about, string status, Dictionary<string, string> socials)
        {
            if (string.IsNullOrEmpty(member_name) || string.IsNullOrEmpty(member_position))
            {
                ViewBag.ErrorMessage = "Please fill in all required fields.";
                return View("Edit");
            }

            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "team");

            if (post == null)
            {
                return NotFound();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
<<<<<<< HEAD
                int parsedStatus = status == "1" ? 1 : 0; // Consistent parsing logic

                post.Title = member_name;
                post.Slug = member_position;
                post.Status = parsedStatus;

                await _context.SaveChangesAsync();

                // Confirm saved status is what we intended
                if (parsedStatus == 0 && post.Status == 1)
                {
                    Console.WriteLine("Overriding DB default to set status=0 (during update)");
                    post.Status = 0;
                    _context.Entry(post).Property(x => x.Status).IsModified = true;
                    await _context.SaveChangesAsync();
                }

                // Update social media links
                var socialJson = JsonSerializer.Serialize(socials ?? new Dictionary<string, string>());
=======
                post.Title = member_name;
                post.Slug = member_position;
                post.Status = int.TryParse(status, out int parsedStatus) ? parsedStatus : post.Status;

                await _context.SaveChangesAsync();

                // Update social media links
                var socialJson = JsonSerializer.Serialize(socials);
>>>>>>> Dashboard
                var excerptMeta = post.Postmetas.FirstOrDefault(m => m.Key == "excerpt");
                if (excerptMeta != null)
                {
                    excerptMeta.Value = socialJson;
                }

                // Update description
                var descMeta = post.Postmetas.FirstOrDefault(m => m.Key == "description");
                if (descMeta != null)
                {
                    descMeta.Value = about;
                }

                // Handle profile picture upload
                if (profile_picture != null)
                {
                    var previewPath = await FileUploader.SaveFile(profile_picture);

                    var previewMeta = post.Postmetas.FirstOrDefault(m => m.Key == "preview");
                    if (previewMeta != null)
                    {
                        previewMeta.Value = previewPath;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Team member updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ViewBag.ErrorMessage = ex.Message;
                return View("Edit", post);
            }
        }



        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "team");

            if (post == null)
            {
                return Json(new { success = false, message = "Error! Post not found." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Remove associated metadata
                _context.Postmetas.RemoveRange(post.Postmetas);

                // Remove post
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new { success = true, message = "Member deleted successfully!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error deleting member: {ex.Message}" });
            }
        }


    }
}

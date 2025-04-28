using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WASender.Helpers;
using WASender.Models;

namespace WASender.Services.AdminSide
{
    public class TestimonialService : ITestimonialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TestimonialService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Post>> GetTestimonialsAsync()
        {
            return await _context.Posts
                .Include(p => p.Postmetas)
                .Where(p => p.Type == "testimonial")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetTestimonialByIdAsync(ulong id)
        {
            return await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "testimonial");
        }

        public async Task<bool> CreateTestimonialAsync(IFormCollection form, IFormFile reviewerAvatar)
        {
            try
            {
                var post = new Post
                {
                    Title = form["ReviewerName"],
                    Slug = form["ReviewerPosition"],
                    Type = "testimonial",
                    Lang = form["Star"],
                    CreatedAt = DateTime.UtcNow

                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync(); // Save to get the Post ID

                // Save comment as excerpt
                var excerptMeta = new Postmeta
                {
                    PostId = post.Id,
                    Key = "excerpt",
                    Value = form["Comment"]
                };
                _context.Postmetas.Add(excerptMeta);

                // Save image as preview
                if (reviewerAvatar != null && reviewerAvatar.Length > 0)
                {
                    var imagePath = await FileHelper.SaveFileAsync(reviewerAvatar, _env);
                    var previewMeta = new Postmeta
                    {
                        PostId = post.Id,
                        Key = "preview",
                        Value = imagePath
                    };
                    _context.Postmetas.Add(previewMeta);
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> EditTestimonialAsync(IFormCollection form, IFormFile? reviewerAvatar)
        {
            if (!ulong.TryParse(form["Id"], out var id)) return false;

            var testimonial = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (testimonial == null) return false;

            testimonial.Title = form["ReviewerName"];
            testimonial.Slug = form["ReviewerPosition"];
            testimonial.Lang = form["Star"].ToString();
            testimonial.UpdatedAt = DateTime.UtcNow;

            var comment = form["Comment"];

            var excerptMeta = testimonial.Postmetas.FirstOrDefault(m => m.Key == "excerpt");
            if (excerptMeta != null) excerptMeta.Value = comment;
            else testimonial.Postmetas.Add(new Postmeta { Key = "excerpt", Value = comment });

            if (reviewerAvatar != null)
            {
                var previewMeta = testimonial.Postmetas.FirstOrDefault(m => m.Key == "preview");
                if (previewMeta != null)
                {
                    UploaderHelper.RemoveFile(previewMeta.Value);
                    previewMeta.Value = await UploaderHelper.SaveFileAsync(reviewerAvatar, _env);
                }
                else
                {
                    testimonial.Postmetas.Add(new Postmeta
                    {
                        Key = "preview",
                        Value = await UploaderHelper.SaveFileAsync(reviewerAvatar, _env)
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateTestimonialAsync(IFormCollection form, IFormFile? reviewerAvatar)
        {
            if (!ulong.TryParse(form["Id"], out var id))
                return false;

            var testimonial = await _context.Posts.FindAsync(id);
            if (testimonial == null)
                return false;

            testimonial.Title = form["ReviewerName"];
            testimonial.Slug = form["ReviewerPosition"];
            testimonial.Lang = form["Star"]; // Already string, no conversion needed

            var comment = await _context.Postmetas.FirstOrDefaultAsync(p => p.PostId == id && p.Key == "excerpt");
            if (comment != null)
                comment.Value = form["Comment"];

            if (reviewerAvatar != null)
            {
                var preview = await _context.Postmetas.FirstOrDefaultAsync(p => p.PostId == id && p.Key == "preview");
                if (preview != null)
                {
                    UploaderHelper.RemoveFile(preview.Value);
                    preview.Value = await UploaderHelper.SaveFileAsync(reviewerAvatar, _env);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestimonialAsync(ulong testimonialId)
        {
            var testimonial = await _context.Posts
                .FirstOrDefaultAsync(t => t.Id == testimonialId);

            if (testimonial == null) return false;

            _context.Posts.Remove(testimonial);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}

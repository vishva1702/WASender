using Microsoft.EntityFrameworkCore;
using WASender.Contracts.AdminSide;
using WASender.Models;

namespace WASender.Services.AdminSide
{
    public class FaqService : IFaqService
    {
        private readonly ApplicationDbContext _context;

        public FaqService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllFaqsAsync()
        {
            return await _context.Posts
                .Where(p => p.Type == "faq")
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddFaqAsync(string question, string answer, string language, string position = "bottom")
        {
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
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<(bool success, string message)> UpdateFaqAsync(ulong id, string question, string answer, string position, string language)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var post = await _context.Posts.Include(p => p.Postmetas).FirstOrDefaultAsync(p => p.Id == id);
                if (post == null)
                    return (false, "Post not found.");

                post.Title = question;
                post.Slug = string.IsNullOrEmpty(position) ? "bottom" : position;
                post.Type = "faq";
                post.Lang = string.IsNullOrEmpty(language) ? "en" : language;
                post.UpdatedAt = DateTime.UtcNow;

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

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "FAQ updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteFaqAsync(ulong id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Type == "faq" && p.Id == id);
            if (post == null)
                return (false, "FAQ not found");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return (true, "FAQ deleted successfully!");
        }
    }
}

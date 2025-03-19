using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogService> _logger;

        public BlogService(ApplicationDbContext context, ILogger<BlogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<Post> Blogs, List<Post> RecentBlogs, List<Category> Categories, List<Category> Tags)> GetBlogsAsync(string search)
        {
            var query = _context.Posts
                .AsNoTracking()
                .Where(p => p.Type == "blog" && p.Lang == "en" && p.Status == 1)
                .Include(p => p.Postmetas)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Title.Contains(search) ||
                    p.Postmetas.Any(m => (m.Key == "short_description" || m.Key == "main_description") && m.Value.Contains(search))
                );
            }

            var blogs = await query.OrderByDescending(p => p.CreatedAt).Take(4).ToListAsync();
            var recentBlogs = await _context.Posts
                .Where(p => p.Type == "blog" && p.Lang == "en" && p.Status == 1)
                .Include(p => p.Postmetas)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.Type == "blog_category" && c.Lang == "en")
                .Take(3)
                .ToListAsync();

            var tags = await _context.Categories
                .Where(c => c.Type == "tags" && c.Lang == "en")
                .Take(3)
                .ToListAsync();

            return (blogs, recentBlogs, categories, tags);
        }
    }
}
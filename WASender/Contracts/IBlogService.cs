using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public interface IBlogService
    {
        Task<(List<Post> Blogs, List<Post> RecentBlogs, List<Category> Categories, List<Category> Tags)> GetBlogsAsync(string search);
    }
}
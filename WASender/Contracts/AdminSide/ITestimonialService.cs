using Microsoft.AspNetCore.Http;
using WASender.Models;

public interface ITestimonialService
{
    Task<IEnumerable<Post>> GetTestimonialsAsync();
    Task<Post?> GetTestimonialByIdAsync(ulong id);
    Task<bool> CreateTestimonialAsync(IFormCollection form, IFormFile reviewerAvatar);
    Task<bool> UpdateTestimonialAsync(IFormCollection form, IFormFile? reviewerAvatar);
    Task<bool> EditTestimonialAsync(IFormCollection form, IFormFile? reviewerAvatar);
    Task<bool> DeleteTestimonialAsync(ulong testimonialId);

}
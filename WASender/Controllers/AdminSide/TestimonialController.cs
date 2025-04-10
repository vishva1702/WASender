using Microsoft.AspNetCore.Mvc;
using WASender.Services.AdminSide;

namespace WASender.Controllers.AdminSide
{
    [Route("admin/testimonials")]
    public class TestimonialController : Controller
    {
        private readonly ITestimonialService _testimonialService;

        public TestimonialController(ITestimonialService testimonialService)
        {
            _testimonialService = testimonialService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var testimonials = await _testimonialService.GetTestimonialsAsync();
            return View(testimonials);
        }

        [HttpGet("create")]
        public IActionResult CreateTestimonial()
        {
            return View(); // ✅ No partial view, full view
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestimonial(IFormCollection form, IFormFile reviewerAvatar)
        {
            try
            {
                var result = await _testimonialService.CreateTestimonialAsync(form, reviewerAvatar);
                if (result)
                {
                    TempData["Success"] = "Testimonial created successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to create testimonial. Please check your input.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        [HttpPost("edit")]
        public async Task<IActionResult> Edit(IFormCollection form, IFormFile? reviewerAvatar)
        {
            try
            {
                var result = await _testimonialService.UpdateTestimonialAsync(form, reviewerAvatar);
                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Testimonial updated successfully!",
                        redirect = Url.Action("Index", "Testimonial", new { area = "admin" })
                    });
                }

                return Json(new { success = false, message = "Failed to update testimonial. Please check your input." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(ulong id)
        {
            try
            {
                var result = await _testimonialService.DeleteTestimonialAsync(id);
                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Testimonial deleted successfully!",
                        redirect = Url.Action("Index", "Testimonial", new { area = "admin" })
                    });
                }

                return Json(new { success = false, message = "Testimonial not found or couldn't be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



    }
}

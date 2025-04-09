using Microsoft.AspNetCore.Mvc;

namespace WASender.Controllers.AdminSide
{
    public class AdminBlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

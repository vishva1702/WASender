using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.UserSide
{
    [Authorize(Roles = "user, User")] // 🔒 Ensures only logged-in users can access this controller
    [Route("UserHome")]
    public class UserHomeController : BaseController
    {

        public UserHomeController(IGlobalDataService globalDataService, ILogger<FeaturesController> logger)
             : base(globalDataService, logger) // Pass both required parameters to BaseController
        { }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            return View();
        }
    }
}
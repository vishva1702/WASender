using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.UserSide
{
    [Authorize(Roles = "user, User")] 
    public class UserHomeController : BaseController
    {

        public UserHomeController(IGlobalDataService globalDataService, ILogger<FeaturesController> logger)
             : base(globalDataService, logger) 
        { }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            return View();
        }
    }
}
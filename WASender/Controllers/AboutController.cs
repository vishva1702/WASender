using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WASender.Services;
using WASender.Models;

namespace WASender.Controllers
{
    public class AboutController : BaseController
    {
        private readonly IAboutService _aboutService;

        public AboutController(IGlobalDataService globalDataService, IAboutService aboutService, ILogger<AboutController> logger)
            : base(globalDataService, logger)
        {
            _aboutService = aboutService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                await LoadGlobalDataAsync();
                await _aboutService.LoadAboutDataAsync(ViewData);
                await _aboutService.LoadCounterDataAsync(ViewData);
                await _aboutService.LoadFaqDataAsync(ViewData);

                ViewBag.Features = await _aboutService.GetFeaturesAsync();
                ViewBag.TeamMembers = await _aboutService.GetTeamMembersAsync();

                var plans = await _aboutService.GetPlansAsync();
                return View(plans);
            }
            catch
            (Exception ex)
            {
                _logger.LogError($"Error fetching data: {ex}");
                ViewData["ErrorMessage"] = "Error fetching data.";
                return View();
            }
            }

        public async Task<IActionResult> TeamDetails(string slug)
        {
            var teamMember = await _aboutService.GetTeamMemberDetailsAsync(slug);
            if (teamMember == null) return NotFound();

            ViewBag.TeamMember = teamMember;
            return View("Show");
        }
    }
}

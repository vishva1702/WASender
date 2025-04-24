using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers
{
    public class RegisterController : BaseController
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IGlobalDataService globalDataService, IRegisterService registerService, ILogger<RegisterController> logger)
            : base(globalDataService, logger)
        {
            _registerService = registerService;
        }

        // ✅ Load Registration Form with PlanId
        [HttpGet]
        public async Task<IActionResult> Index(ulong? id)
        {
            await LoadGlobalDataAsync();

            var (planTitle, planId) = await _registerService.GetPlanDetails(id);
            ViewBag.PlanTitle = planTitle;
            ViewBag.PlanId = planId;
            return View(new User());
        }

        // ✅ Handle Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPlan(User model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                return View("Index", model);
            }

            try
            {
                var result = await _registerService.RegisterUserAsync(model);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.ErrorMessage;
                    return View("Index", model);
                }

                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                TempData["ErrorMessage"] = "An error occurred while registering. Please try again.";
                return View("Index", model);
            }
        }
    }
}
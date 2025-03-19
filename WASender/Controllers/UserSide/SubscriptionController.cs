using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using WASender.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly IPlanService _planService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubscriptionController(IPlanService planService, IHttpContextAccessor httpContextAccessor)
    {
        _planService = planService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IActionResult> Index()
    {
        // Get user's PlanId from claims
        var userPlanId = _httpContextAccessor.HttpContext?.User?.FindFirst("PlanId")?.Value;
        int.TryParse(userPlanId, out int currentPlanId);

        // Get active plans from service
        var plans = await _planService.GetActivePlansAsync(currentPlanId);

        return View(plans);
    }
}
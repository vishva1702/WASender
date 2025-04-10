//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using WASender.Models;
//using WASender.Services;

//namespace WASender.Controllers
//{
//    public class SubscriptionController : BaseController
//    {
//        private readonly ISubscriptionService _subscriptionService;

//        public SubscriptionController(
//            IGlobalDataService globalDataService,
//            ILogger<BaseController> logger,
//            ISubscriptionService subscriptionService)
//            : base(globalDataService, logger)
//        {
//            _subscriptionService = subscriptionService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            await LoadGlobalDataAsync();
//            var plans = await _subscriptionService.GetActivePlans();
//            return View(plans);
//        }

//        //    public async Task<IActionResult> Show(int id)
//        //    {
//        //        await LoadGlobalDataAsync();
//        //        var plan = await _subscriptionService.GetPlanById(id);
//        //        if (plan == null || (decimal)(plan.Price ?? 0) <= 0)
//        //        {
//        //            TempData["saas_error"] = "Please select a valid plan.";
//        //            return RedirectToAction("Index");
//        //        }

//        //        var gateways = await _subscriptionService.GetActiveGateways();
//        //        //var tax = (decimal)(_subscriptionService.CalculateTax(plan.Price) ?? 0);
//        //        var total = (decimal)(plan.Price ?? 0) + tax;

//        //        HttpContext.Session.SetInt32("plan_id", (int)plan.Id);
//        //        HttpContext.Session.Remove("payment_info");
//        //        HttpContext.Session.Remove("call_back");

//        //        ViewBag.Plan = plan;
//        //        ViewBag.Gateways = gateways;
//        //        ViewBag.Tax = tax;
//        //        ViewBag.Total = total;

//        //        return View("Payment");
//        //    }

//        //    public async Task<IActionResult> Log()
//        //    {
//        //        await LoadGlobalDataAsync();
//        //        var userId = GetUserId();
//        //        var orders = await _subscriptionService.GetUserOrders(userId);
//        //        return View(orders);
//        //    }

//        //    [HttpPost]
//        //    public async Task<IActionResult> Subscribe(int gatewayId, int planId, string phone, IFormFile image, string comment)
//        //    {
//        //        var plan = await _subscriptionService.GetPlanById(planId);
//        //        var gateway = await _subscriptionService.GetGatewayById(gatewayId);

//        //        if (plan == null || gateway == null)
//        //        {
//        //            TempData["error"] = "Invalid plan or gateway.";
//        //            return RedirectToAction("Index");
//        //        }

//        //        var tax = (decimal)(_subscriptionService.CalculateTax(plan.Price) ?? 0);
//        //        var total = (decimal)(plan.Price ?? 0) + tax;
//        //        var payable = total * (decimal)gateway.Multiply + (decimal)gateway.Charge;

//        //        if (payable < (int)gateway.MinAmount || (gateway.MaxAmount != unchecked((ulong)-1) && payable > (int)gateway.MaxAmount))
//        //        {
//        //            TempData["min-max"] = "Invalid transaction amount.";
//        //            return RedirectToAction("Show", new { id = planId });
//        //        }

//        //        if (gateway.IsAuto == 0 && (string.IsNullOrWhiteSpace(comment) || image == null))
//        //        {
//        //            TempData["error"] = "Payment proof and comment are required.";
//        //            return RedirectToAction("Show", new { id = planId });
//        //        }

//        //        //var paymentData = new PaymentData
//        //        //{
//        //        //    Currency = gateway.Currency ?? "USD",
//        //        //    Email = GetUserEmail(),
//        //        //    Name = GetUserName(),
//        //        //    Phone = phone,
//        //        //    Amount = total,
//        //        //    Charge = (decimal)gateway.Charge,
//        //        //    PayAmount = payable,
//        //        //    GatewayId = gateway.Id,
//        //        //    Screenshot = UploadImage(image),
//        //        //    Comment = comment
//        //        //};

//        //        HttpContext.Session.SetInt32("plan_id", (int)plan.Id);
//        //        HttpContext.Session.SetString("call_back_success", Url.Action("Success", "Subscription"));
//        //        HttpContext.Session.SetString("call_back_fail", Url.Action("Failed", "Subscription"));

//        //        return Redirect(await _subscriptionService.MakePayment(paymentData));
//        //    }

//        //    public IActionResult Status(string status)
//        //    {
//        //        if (HttpContext.Session.GetInt32("plan_id") == null)
//        //            return NotFound();

//        //        return status == "success" ? RedirectToAction("Success") : RedirectToAction("Failed");
//        //    }

//        //    public async Task<IActionResult> Success()
//        //    {
//        //        if (!HttpContext.Session.Keys.Contains("payment_info"))
//        //            return NotFound();

//        //        var paymentInfo = HttpContext.Session.GetString("payment_info");
//        //        var planId = HttpContext.Session.GetInt32("plan_id");
//        //        var plan = await _subscriptionService.GetPlanById(planId ?? 0);

//        //        if (plan == null)
//        //            return NotFound();

//        //        //if (paymentInfo != null && paymentInfo.Contains("Status:1"))
//        //        //{
//        //        //    await _subscriptionService.CreateOrder(GetUserId(), plan, new PaymentInfo { Status = 1 });
//        //        //    TempData["success"] = "Your subscription payment is complete.";
//        //        //}
//        //        //else
//        //        //{
//        //        //    TempData["error"] = "Payment needs admin review.";
//        //        //}

//        //        return RedirectToAction("Log");
//        //    }

//        //    public IActionResult Failed()
//        //    {
//        //        HttpContext.Session.Clear();
//        //        TempData["error"] = "Payment failed.";
//        //        return RedirectToAction("Index");
//        //    }

//        //    private int GetUserId() => 1;
//        //    private string GetUserEmail() => "user@example.com";
//        //    private string GetUserName() => "User Name";
//        //    private string UploadImage(IFormFile file) => "/path/to/uploaded/file";
//    }
//}


using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using WASender.Models;
using Microsoft.AspNetCore.Authorization;
using WASender.Services;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly ISubscriptionService _planService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SubscriptionController(ISubscriptionService planService, IHttpContextAccessor httpContextAccessor)
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
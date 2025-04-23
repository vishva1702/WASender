using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminPlanController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminPlanController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
             : base(globalDataService, logger)
        {
            _context = context;
        }

        // GET: Admin/Plan
        public async Task<IActionResult> Index()
        {
           await LoadGlobalDataAsync();

            var plans = _context.Plans
                .Include(p => p.Orders)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();  // Return a list of Plan objects

            Console.WriteLine($"Plans fetched: {plans.Count}"); // Debugging output

            ViewBag.Plans = plans;
            return View();
        }



        // GET: Admin/Plan/Create
        public async Task<IActionResult> Create()
        {
             await LoadGlobalDataAsync();

            return View();
        }

        // POST: Admin/Plan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(Plan plan, [FromForm] Dictionary<string, string> plan_data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    TempData["Error"] = "Invalid input: " + string.Join(", ", errors);
                    return RedirectToAction("Create");
                }

                // Convert plan_data dictionary to JSON
                string jsonData = System.Text.Json.JsonSerializer.Serialize(plan_data);

                // Set checkbox values manually, since unchecked checkboxes are not posted.
                // If the key exists in Request.Form, then the checkbox was checked.
                int isFeatured = Request.Form.ContainsKey("is_featured") ? 1 : 0;
                int isRecommended = Request.Form.ContainsKey("is_recommended") ? 1 : 0;
                int isTrial = Request.Form.ContainsKey("is_trial") ? 1 : 0;
                int status = Request.Form.ContainsKey("status") ? 1 : 0;

                // Create a new plan instance and assign values
                var newPlan = new Plan
                {
                    Title = plan.Title,
                    Days = plan.Days,
                    Price = plan.Price,
                    Labelcolor = plan.Labelcolor,
                    Iconname = plan.Iconname,
                    Data = jsonData, // Store additional data as JSON
                    IsFeatured = isFeatured,
                    IsRecommended = isRecommended,
                    IsTrial = isTrial,
                    TrialDays = plan.TrialDays,
                    Status = status
                };

                // Save to database (assuming _context is your DbContext)
                _context.Plans.Add(newPlan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Plan created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the plan. Details: " + ex.InnerException?.Message;
                Console.WriteLine(ex.ToString());
                return RedirectToAction("Create");
            }
        }



        // GET: Admin/Plan/Edit/5
        public async Task<IActionResult> Edit(ulong id)
        {
            await LoadGlobalDataAsync();

            var plan = _context.Plans.Find(id);
            if (plan == null) return NotFound();

            // Deserialize JSON data
            var planData = string.IsNullOrEmpty(plan.Data)
                ? new Dictionary<string, string>()
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(plan.Data);

            ViewBag.Plan = plan;
            ViewBag.PlanData = planData;

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ulong id, Plan plan, [FromForm] Dictionary<string, string> planData)
        {
            var existingPlan = _context.Plans.Find(id);
            if (existingPlan == null)
            {
                TempData["Error"] = "Plan not found.";
                return RedirectToAction("Edit", new { id });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                TempData["Error"] = "Validation Errors: " + string.Join(", ", errors);
                ViewBag.Plan = plan;
                ViewBag.PlanData = planData;
                return View("Edit", plan);
            }

            try
            {
                existingPlan.Title = plan.Title;
                existingPlan.Price = plan.Price;
                existingPlan.Labelcolor = plan.Labelcolor;
                existingPlan.Iconname = plan.Iconname;
                existingPlan.Days = plan.Days;
                existingPlan.TrialDays = plan.TrialDays;

                // **Fix: Properly retrieve checkbox values**
                existingPlan.IsFeatured = Request.Form.ContainsKey("IsFeatured") ? 1 : 0;
                existingPlan.IsRecommended = Request.Form.ContainsKey("IsRecommended") ? 1 : 0;
                existingPlan.IsTrial = Request.Form.ContainsKey("IsTrial") ? 1 : 0;
                existingPlan.Status = Request.Form.ContainsKey("Status") ? 1 : 0;

                existingPlan.Data = JsonConvert.SerializeObject(planData);
                existingPlan.UpdatedAt = DateTime.Now;

                _context.Plans.Update(existingPlan);
                _context.SaveChanges();

                TempData["Success"] = "Plan updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the plan.";
                return RedirectToAction("Edit", new { id });
            }
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var plan = _context.Plans.Find(id);
            if (plan == null)
            {
                return Json(new { success = false, message = "Plan not found!" });
            }  

            _context.Plans.Remove(plan);
            _context.SaveChanges();

            return Json(new { success = true, message = "Plan deleted successfully!" });
        }


    }
}

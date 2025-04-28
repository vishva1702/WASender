using Microsoft.AspNetCore.Mvc;
using WASender.Services.AdminSide;
using System.Threading.Tasks;
using WASender.Contracts.AdminSide;

namespace WASender.Areas.Admin.Controllers
{
  
    public class AdminScheduleController : Controller
    {
        private readonly IAdminScheduleService _scheduleService;

        public AdminScheduleController(IAdminScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }
        public async Task<IActionResult> Index(string search = "", string type = "")
        {
            var (schedules, total, pending, delivered) = await _scheduleService.GetSchedulesAsync(search, type);

            ViewBag.Type = type;
            ViewBag.Search = search;
            ViewBag.TotalSchedules = total;
            ViewBag.PendingSchedules = pending;
            ViewBag.DeliveredSchedules = delivered;

            return View((schedules, total, pending, delivered));  // This is passing a tuple
        }



        [HttpPost]
        public async Task<IActionResult> Delete(ulong id)
        {
            var success = await _scheduleService.DeleteScheduleAsync(id);
            if (!success) return NotFound();

            return Json(new
            {
                redirect = Url.Action("Index"),
                message = "Schedule removed successfully."
            });
        }
    }
}

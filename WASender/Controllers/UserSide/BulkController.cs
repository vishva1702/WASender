using Microsoft.AspNetCore.Mvc;
using WASender.Models;

namespace WASender.Controllers.UserSide
{
    [Route("user/bulk-message")]
    public class BulkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BulkController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
        //    var model = new BulkTransactionViewModel
        //    {
        //        Templates = _context.Templates.ToList(),
        //        Groups = _context.Groups.ToList(),
        //        Devices = _context.Devices.ToList(),
        //        Transactions = _context.Transactions
        //                            .Include(t => t.Template)
        //                            .Include(t => t.Device)
        //                            .Include(t => t.Group)
        //                            .Include(t => t.User) // if needed
        //                            .ToList(),
        //        TotalMessages = _context.Transactions.Sum(t => t.TotalMessages),
        //        Last30DaysMessages = _context.Transactions
        //                                     .Where(t => t.CreatedAt >= DateTime.Now.AddDays(-30))
        //                                     .Sum(t => t.TotalMessages),
        //        TodaysTransaction = _context.Transactions
        //                                    .Where(t => t.CreatedAt.Date == DateTime.Today)
        //                                    .ToList()
        //    };

            return View();
        }
    }
}
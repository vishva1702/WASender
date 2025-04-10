//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using WASender.Models;

//namespace YourNamespace.Services
//{
//    public class SubscriptionService : ISubscriptionService
//    {
//        private readonly ApplicationDbContext _context;

//        public SubscriptionService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<Plan>> GetActivePlans()
//        {
//            return await _context.Plans.Where(p => p.Status == 1).ToListAsync();
//        }

//        public async Task<Plan> GetPlanById(int id)
//        {
//            return await _context.Plans.FirstOrDefaultAsync(p => p.Id == (ulong)id);
//        }

//        public async Task<List<Gateway>> GetActiveGateways()
//        {
//            return await _context.Gateways.Where(g => g.Status == 1).ToListAsync();
//        }

//        public async Task<Gateway> GetGatewayById(int id)
//        {
//            return await _context.Gateways.FirstOrDefaultAsync(g => g.Id == (ulong)id);
//        }

//        public async Task<List<Order>> GetUserOrders(int userId)
//        {
//            return await _context.Orders.Where(o => o.UserId == (ulong)userId).ToListAsync();
//        }

//        public decimal CalculateTax(decimal amount)
//        {
//            decimal taxRate = 0.10m; // TODO: Load tax rate from configuration
//            return Math.Round(amount * taxRate, 2);
//        }

//        public async Task<string> MakePayment(string currency, string email, string name, string phone, decimal amount, decimal charge, decimal payAmount, int gatewayId, string screenshot, string comment)
//        {
//            // TODO: Implement actual payment logic
//            return await Task.FromResult("/payment/redirect-url");
//        }

//        public async Task CreateOrder(int userId, Plan plan, decimal amount, int status) // Matching return type
//        {
//            var order = new Order
//            {
//                UserId = (ulong)userId,
//                PlanId = plan.Id,
//                Amount = Convert.ToDouble(amount),
//                Status = status,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Orders.Add(order);
//            await _context.SaveChangesAsync(); // Async save to match Task return type
//        }
//    }
//}


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;

    public SubscriptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Plan>> GetActivePlansAsync(int? currentPlanId)
    {
        return await _context.Plans
            .Where(p => p.Status == 1 && p.Price > 0 && (!currentPlanId.HasValue || p.Id != (ulong)currentPlanId.Value))
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }
}
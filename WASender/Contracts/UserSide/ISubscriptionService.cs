//using System.Collections.Generic;
//using System.Threading.Tasks;
//using WASender.Models;

//namespace YourNamespace.Services
//{
//    public interface ISubscriptionService
//    {
//        Task<List<Plan>> GetActivePlans();
//        Task<Plan> GetPlanById(int id);
//        Task<List<Gateway>> GetActiveGateways();
//        Task<Gateway> GetGatewayById(int id);
//        Task<List<Order>> GetUserOrders(int userId);
//        decimal CalculateTax(decimal amount);
//        Task<string> MakePayment(string currency, string email, string name, string phone, decimal amount, decimal charge, decimal payAmount, int gatewayId, string screenshot, string comment);
//        Task CreateOrder(int userId, Plan plan, decimal amount, int status); // Changed void to Task
//    }
//}


using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;

public interface ISubscriptionService
{
    Task<List<Plan>> GetActivePlansAsync(int? currentPlanId);
}
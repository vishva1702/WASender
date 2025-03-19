using System.Collections.Generic;
using System.Threading.Tasks;
using WASender.Models;

public interface IPlanService
{
    Task<List<Plan>> GetActivePlansAsync(int? currentPlanId);
}
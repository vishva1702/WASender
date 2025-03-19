using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WASender.Models;

public class PlanService : IPlanService
{
    private readonly ApplicationDbContext _context;

    public PlanService(ApplicationDbContext context)
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
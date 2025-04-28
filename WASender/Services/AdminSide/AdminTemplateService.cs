using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WASender.Contracts.AdminSide;
using WASender.Models;

public class AdminTemplateService : IAdminTemplateService
{
    private readonly ApplicationDbContext _context;

    public AdminTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> DeleteTemplateAsync(ulong templateId, ulong userId)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == templateId);

        if (template == null) return false;

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}

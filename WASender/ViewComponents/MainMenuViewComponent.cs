using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using WASender.Models;

public class MainMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MainMenuViewComponent> _logger;

    public MainMenuViewComponent(ApplicationDbContext context, ILogger<MainMenuViewComponent> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var menuData = await _context.Menus
            .Where(m => m.Name == "Header" && m.Lang == "en")
            .Select(m => m.Data)
            .FirstOrDefaultAsync();

        JArray menuItems = new JArray();

        if (!string.IsNullOrEmpty(menuData))
        {
            try
            {
                menuItems = JArray.Parse(menuData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing JSON in MainMenu: {ex.Message}");
            }
        }

        return View(menuItems);
    }

}

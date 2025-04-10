using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

[Authorize(Policy = "page-settings")]
[Route("AdminPageSettings")]
public class AdminPageSettingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;

    public AdminPageSettingsController(ApplicationDbContext context, IWebHostEnvironment env, IMemoryCache cache)
    {
        _context = context;
        _env = env;
        _cache = cache;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        var primaryData = await GetOptionJson("primary_data", locale);
        var themePath = (await _context.Options.FirstOrDefaultAsync(x => x.Key == "theme_path"))?.Value ?? "frontend.index";
        var headerFooter = await GetOptionJson("header_footer", locale);
        var contactPage = await GetOptionJson("contact-page", locale);
        var home = await GetOptionJson("home-page", locale);
        var whyChoose = await GetOptionJson("why-choose", locale);

        ViewBag.PrimaryData = primaryData;
        ViewBag.ThemePath = themePath;
        ViewBag.HeaderFooter = headerFooter;
        ViewBag.ContactPage = contactPage;
        ViewBag.Home = home;
        ViewBag.WhyChoose = whyChoose;

        ViewBag.FeaturesArea = GetNestedStatus(home, "brand");
        ViewBag.BrandArea = GetNestedStatus(home, "brand");
        ViewBag.AccountArea = GetNestedStatus(home, "account_area");

        return View("~/Views/AdminPageSettings/Index.cshtml");
    }

    [HttpPost("Update/{id}")]
    public async Task<IActionResult> Update(string id)
    {
        var lang = Request.Form["lang"].ToString() ?? "en";

        var data = new Dictionary<string, string>();
        foreach (var key in Request.Form.Keys)
        {
            if (!string.IsNullOrWhiteSpace(key) && key != "__RequestVerificationToken" && key != "lang" && key != "_method")
            {
                var value = Request.Form[key].ToString();
                if (!string.IsNullOrEmpty(value))
                    data[key] = value;
            }
        }

        var files = Request.Form.Files;
        if (id == "primary_data" && files != null && files.Any())
        {
            if (files["logo"] is IFormFile logo && logo.Length > 0)
                data["logo"] = await SaveFile(logo);
            if (files["footer_logo"] is IFormFile footerLogo && footerLogo.Length > 0)
                data["footer_logo"] = await SaveFile(footerLogo);
            if (files["favicon"] is IFormFile favicon && favicon.Length > 0)
                data["favicon"] = await SaveFile(favicon);
        }

        var nested = ConvertFlatToNested(data);
        var json = JsonSerializer.Serialize(nested);

        var option = await _context.Options.FirstOrDefaultAsync(x => x.Key == id && x.Lang == lang);
        if (option == null)
        {
            option = new Option { Key = id, Lang = lang, Value = json };
            _context.Options.Add(option);
        }
        else
        {
            option.Value = json;
            _context.Options.Update(option);
        }

        await _context.SaveChangesAsync();
        _cache.Remove("options_cache");

        var message = id switch
        {
            "primary_data" => "Primary settings updated...!",
            "header_footer" => "Header Footer settings updated...!",
            "contact-page" => "Contact Page settings updated...!",
            "home-page" => "Home Page settings updated...!",
            "why-choose" => "Section settings updated...!",
            _ => "Settings updated...!"
        };

        return Json(new { message });
    }

    private async Task<JsonElement> GetOptionJson(string key, string lang)
    {
        var option = await _context.Options.FirstOrDefaultAsync(x => x.Key == key && x.Lang == lang);
        try
        {
            return !string.IsNullOrWhiteSpace(option?.Value)
                ? JsonSerializer.Deserialize<JsonElement>(option.Value)
                : new JsonElement();
        }
        catch
        {
            return new JsonElement();
        }
    }

    private string GetNestedStatus(JsonElement json, string section)
    {
        if (json.ValueKind == JsonValueKind.Object &&
            json.TryGetProperty(section, out var subSection) &&
            subSection.ValueKind == JsonValueKind.Object &&
            subSection.TryGetProperty("status", out var status))
        {
            return status.GetString() ?? "active";
        }
        return "active";
    }

    private async Task<string> SaveFile(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/" + fileName;
    }

    private Dictionary<string, object> ConvertFlatToNested(Dictionary<string, string> flat)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in flat)
        {
            var keys = kvp.Key.Split('.');
            var current = result;

            for (int i = 0; i < keys.Length; i++)
            {
                if (i == keys.Length - 1)
                {
                    current[keys[i]] = kvp.Value;
                }
                else
                {
                    if (!current.ContainsKey(keys[i]))
                        current[keys[i]] = new Dictionary<string, object>();

                    current = (Dictionary<string, object>)current[keys[i]];
                }
            }
        }

        return result;
    }
}

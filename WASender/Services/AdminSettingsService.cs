//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;    
//using WASender.Contracts;
//using WASender.Models;

//namespace WASender.Services
//{
//    public class AdminSettingsService : IAdminSettingsService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _env;

//        public AdminSettingsService(ApplicationDbContext context, IWebHostEnvironment env)
//        {
//            _context = context;
//            _env = env;
//        }

//        public async Task<string?> GetOptionValueAsync(string key, string? lang = null)
//        {
//            return await _context.Options
//                .Where(o => o.Key == key && (lang == null || o.Lang == lang))
//                .Select(o => o.Value)
//                .FirstOrDefaultAsync(); // Now works
//        }

//        public async Task<JsonElement?> GetOptionJsonAsync(string key, string? lang = null)
//        {
//            var value = await GetOptionValueAsync(key, lang);
//            if (string.IsNullOrEmpty(value)) return null;
//            return JsonSerializer.Deserialize<JsonElement>(value);
//        }

//        public async Task UpsertOptionAsync(string key, string lang, object value)
//        {
//            //var option = await _context.Options
//            //    .FirstOrDefaultAsync(o => o.Key == key && o.Lang == lang);

//            //if (option == null)
//            //{
//            //    option = new Option { Key = key, Lang = lang };
//            //    _context.Options.Add(option);
//            //}

//            //// 🛠 Handle if value is already JSON string (don't double serialize)
//            //option.Value = value is string str ? str : JsonSerializer.Serialize(value);
//            //await _context.SaveChangesAsync();

//            var existingOption = await _context.Options
//        .FirstOrDefaultAsync(o => o.Key == key && o.Lang == lang);

//            var jsonValue = value is string str ? str : JsonSerializer.Serialize(value);

//            if (existingOption != null)
//            {
//                // Update existing value
//                existingOption.Value = jsonValue;

//                _context.Options.Update(existingOption); 
//            }
//            else
//            {
//                // Create new entry
//                var newOption = new Option
//                {
//                    Key = key,
//                    Lang = lang,
//                    Value = jsonValue
//                };

//                await _context.Options.AddAsync(newOption);
//            }

//            await _context.SaveChangesAsync(); 
//        }

//        public async Task<string> SaveFileAsync(IFormFile file)
//        {
//            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
//            var uploadPath = Path.Combine(_env.WebRootPath, "uploads");

//            if (!Directory.Exists(uploadPath))
//                Directory.CreateDirectory(uploadPath);

//            var fullPath = Path.Combine(uploadPath, fileName);
//            using var stream = new FileStream(fullPath, FileMode.Create);
//            await file.CopyToAsync(stream);

//            return "/uploads/" + fileName;
//        }
//    }
//}
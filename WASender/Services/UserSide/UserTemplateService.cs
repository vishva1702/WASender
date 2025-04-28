using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WASender.Contracts.UserSide;
using WASender.Models;

namespace WASender.Services.UserSide
{
    public class UserTemplateService : IUserTemplateService
    {
        private readonly ApplicationDbContext _context;

        public UserTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Template> GetUserTemplates(ulong userId, out int totalTemplates, out int activeTemplates, out int inactiveTemplates, out string limit)
        {
            var templates = _context.Templates
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            activeTemplates = templates.Count(t => t.Status == 1);
            inactiveTemplates = templates.Count(t => t.Status == 0);
            totalTemplates = templates.Count;

            var user = _context.Users.Find(userId);
            limit = "0";

            if (!string.IsNullOrEmpty(user?.Plan))
            {
                var plan = JsonSerializer.Deserialize<Dictionary<string, object>>(user.Plan);

                if (plan.TryGetValue("messages_limit", out var limitValue))
                {
                    int messagesLimit = limitValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String
                        ? int.Parse(jsonElement.GetString() ?? "0")
                        : Convert.ToInt32(limitValue);

                    limit = (messagesLimit == -1) ? totalTemplates.ToString() : $"{totalTemplates} / {messagesLimit}";
                }
            }

            return templates;
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

        public async Task<(bool success, string message)> CreateTemplateAsync(IFormCollection form, ulong userId)
        {
            try
            {
                string type = form["templateType"];
                string templateName = form["Title"]; // ✅ matches the form field name

                if (string.IsNullOrEmpty(templateName) || templateName.Length > 100)
                    return (false, "Template name is required and must be less than 100 characters.");

                string? body = form["Body"];
                string? attachment = null;


                switch (type)
                {
                    case "text-with-media":
                        var file = form.Files["file"]; // ✅ this matches the <input name="file" />
                        var mediaMessage = form["Body"]; // ✅ this matches the <textarea name="Body">

                        string msg = mediaMessage.ToString();

                        if (file == null || string.IsNullOrEmpty(msg) || msg.Length > 1000)
                            return (false, "File and message are required.");

                        attachment = await SaveFileAsync(file);
                        body = msg;
                        break;



                    case "text-with-vcard":
                        if (string.IsNullOrEmpty(form["FullName"]) ||
                            string.IsNullOrEmpty(form["OrgName"]) ||
                            string.IsNullOrEmpty(form["ContactNumber"]) ||
                            string.IsNullOrEmpty(form["WaNumber"]))
                            return (false, "All fields are required for vCard.");
                        break;


                    case "text-with-button":
                    case "text-with-template":
                        string? footer = form["FooterText"]; // ✅ matches form
                        string? textMessage = form["Body"];  // ✅ matches form

                        if (string.IsNullOrEmpty(footer) || string.IsNullOrEmpty(textMessage))
                            return (false, "Footer text and message are required.");

                        var buttons = form.Keys.Where(k => k.StartsWith("Buttons")).ToArray();
                        if (buttons.Length > 9) // 3 buttons * 3 fields each
                            return (false, "Maximum button limit is 3.");

                        body = textMessage;
                        break;


                    case "text-with-location":
                        string? lat = form["Latitude"];
                        string? lng = form["Longitude"];
                        if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
                            return (false, "Latitude and Longitude are required.");
                        break;


                    case "text-with-list":
                        if (string.IsNullOrEmpty(form["HeaderTitle"]) || string.IsNullOrEmpty(form["Body"]) ||
                            string.IsNullOrEmpty(form["FooterText"]) || string.IsNullOrEmpty(form["ButtonText"]))
                            return (false, "All fields are required for list template.");

                        body = form["Body"];
                        break;

                }

                var template = new Template
                {
                    Uuid = Guid.NewGuid(),
                    UserId = userId,
                    Title = templateName,
                    Body = body,
                    Type = type,
                    Status = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Templates.Add(template);
                await _context.SaveChangesAsync();

                return (true, "Template created successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/uploads/" + uniqueFileName;
        }

        public async Task<Template?> GetTemplateByIdAsync(ulong templateId, ulong userId)
        {
            return await _context.Templates
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == templateId);
        }

        public async Task<(bool success, string message)> UpdateTemplateAsync(ulong templateId, IFormCollection form, ulong userId)
        {
            var template = await GetTemplateByIdAsync(templateId, userId);
            if (template == null)
                return (false, "Template not found.");

            string templateName = form["Title"];
            if (string.IsNullOrEmpty(templateName) || templateName.Length > 100)
                return (false, "Template name is required and must be under 100 characters.");

            string type = template.Type;
            string? body = null;
            string? attachment = null;

            switch (type)
            {
                case "text-with-media":
                    var file = form.Files["file"];
                    string? mediaMessage = form["Body"];

                    if (file == null && string.IsNullOrEmpty(mediaMessage))
                        return (false, "File and message are required.");

                    if (file != null)
                    {
                        attachment = await SaveFileAsync(file);
                    }
                    else
                    {
                        attachment = template.Body;
                    }

                    body = mediaMessage;
                    break;

                case "text-with-button":
                case "text-with-template":
                    string? footer = form["FooterText"];
                    string? message = form["Body"];
                    var buttons = form.Keys.Where(k => k.StartsWith("Buttons")).ToArray();

                    if (string.IsNullOrEmpty(footer) || string.IsNullOrEmpty(message))
                        return (false, "Footer text and message are required.");

                    if (buttons.Length > 9)
                        return (false, "Maximum Button Limit Is 3.");

                    body = message;
                    break;

                case "text-with-location":
                    string? latitude = form["Latitude"];
                    string? longitude = form["Longitude"];

                    if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
                        return (false, "Latitude and Longitude are required.");

                    body = $"Location: {latitude}, {longitude}";
                    break;

                case "text-with-list":
                    string? header = form["HeaderTitle"];
                    string? listMessage = form["Body"];
                    string? listFooter = form["FooterText"];
                    string? buttonText = form["ButtonText"];
                    var sections = form.Keys.Where(k => k.StartsWith("Section")).ToArray();

                    if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(listMessage) ||
                        string.IsNullOrEmpty(listFooter) || string.IsNullOrEmpty(buttonText))
                        return (false, "All fields are required for list template.");

                    if (sections.Length > 20)
                        return (false, "Maximum Section Limit Is 20.");

                    body = listMessage;
                    break;
            }

            // Update template
            template.Title = templateName;
            template.Body = body;
            template.UpdatedAt = DateTime.UtcNow;

            _context.Templates.Update(template);
            await _context.SaveChangesAsync();

            return (true, "Template Updated Successfully..!!");
        }
    }
}

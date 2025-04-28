using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WASender.Models;

namespace WASender.Services
{
    public class WhatsAppService
    {
        private readonly ApplicationDbContext _context; // Replace with your actual DbContext
        private readonly HttpClient _httpClient;

        public WhatsAppService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<Dictionary<string, object>> SendMessageAsync(ulong fromDeviceId, string receiver, string message, string type, bool filter = false, int delay = 0)
        {
            if (delay < 500) delay = 1;
            else delay = delay / 1000;

            await Task.Delay(delay * 1000);

            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Status == 1 && d.Id == fromDeviceId);
            if (device == null) return new Dictionary<string, object> { { "status", 404 }, { "message", "Device not found" } };

            var sessionId = $"device_{fromDeviceId}";
            var formattedMessage = FormatMessage(message, device.UserId);
            var body = new Dictionary<string, object>
    {
        { "receiver", receiver },
        { "delay", 0 },
        { "message", formattedMessage }
    };

            var response = await _httpClient.PostAsJsonAsync($"https://your-whatsapp-server-url/chats/send?id={sessionId}", body);
            return await HandleResponse(response);
        }

        private object FormatMessage(string message, ulong? userId)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, object>> GetChatsAsync(long deviceId)
        {
            var sessionId = $"device_{deviceId}";
            var response = await _httpClient.GetAsync($"https://your-whatsapp-server-url/chats?id={sessionId}");
            return await HandleResponse(response);
        }

        public async Task<Dictionary<string, object>> SaveTemplateAsync(Template templateData, string message, string type, ulong userId, ulong? templateId = null)
        {
            Template template;

            if (templateId == null)
            {
                template = new Template();
            }
            else
            {
                template = await _context.Templates.FindAsync(templateId);
                if (template == null) throw new Exception("Template not found");
                template.Status = templateData.Status;
            }

            template.Title = templateData.Title;
            template.UserId = userId;
            template.Body = FormatMessage(message, userId);
            template.Type = type;

            _context.Templates.Update(template);
            await _context.SaveChangesAsync();

            return new Dictionary<string, object> { { "status", 200 }, { "message", "Template saved successfully" } };
        }

        private string FormatMessage(string message, ulong userId)
        {
            // Implement your message formatting logic here
            return message; // Placeholder
        }

        private async Task<Dictionary<string, object>> HandleResponse(HttpResponseMessage response)
        {
            var responseData = new Dictionary<string, object>();
            if (!response.IsSuccessStatusCode)
            {
                responseData["status"] = (int)response.StatusCode;
                responseData["message"] = await response.Content.ReadAsStringAsync();
            }
            else
            {
                responseData["status"] = 200;
                // Deserialize the response content to a dynamic object or a specific type
                var responseBody = await response.Content.ReadAsStringAsync();
                responseData["data"] = JsonSerializer.Deserialize<object>(responseBody); // Adjust based on your expected response
            }
            return responseData;
        }
    }
}
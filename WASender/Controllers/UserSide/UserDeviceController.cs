using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WASender.Models;
using QRCoder;
using System.Drawing;
using System.IO;

namespace WASender.Controllers.UserSide
{
    public class UserDeviceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserDeviceController> _logger;

        public UserDeviceController(
            ApplicationDbContext context,
            IConfiguration config,
            ILogger<UserDeviceController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetLoggedInUserId();
            var devices = await _context.Devices
                .Where(d => d.UserId == userId)
                .Include(d => d.Smstransactions)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            ViewBag.SmsTransactionCounts = devices.ToDictionary(
                d => d.Id,
                d => d.Smstransactions.Count
            );

            return View(devices);
        }

        [HttpGet]
        public async Task<IActionResult> DeviceStatics()
        {
            var userId = GetLoggedInUserId();

            int total = await _context.Devices.CountAsync(d => d.UserId == userId);
            int active = await _context.Devices.CountAsync(d => d.UserId == userId && d.Status == 1);
            int inactive = await _context.Devices.CountAsync(d => d.UserId == userId && d.Status == 0);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            string plan = user?.Plan ?? "{}";

            int? deviceLimit = 0;

            try
            {
                var planData = JsonDocument.Parse(plan);
                if (planData.RootElement.TryGetProperty("device_limit", out var limitProp))
                {
                    deviceLimit = limitProp.GetInt32();
                }
            }
            catch
            {
                // ignore JSON parse errors
            }

            var result = new
            {
                total = (deviceLimit == -1) ? total.ToString() : $"{total} / {deviceLimit}",
                active,
                inActive = inactive
            };

            return Json(result);
        }

        public async Task<IActionResult> Create()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Store(string name, string webhook_url)
        {
            try
            {
                var userId = GetLoggedInUserId();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return Unauthorized();
                if (!CheckDeviceLimit(user.Id))
                    return StatusCode(401, new { message = "Maximum Device Limit Exceeded" });

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(webhook_url))
                    return BadRequest(new { message = "Device name and webhook URL are required." });

                var device = new Device
                {
                    UserId = user.Id,
                    Name = name,
                    HookUrl = webhook_url,
                    Uuid = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Status = 0 // Initially disconnected
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    redirect = Url.Action("ScanQr", "UserDevice", new { id = device.Uuid }),
                    message = "Device Created Successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        public async Task<IActionResult> ScanQr(Guid id)
        {
            var userId = GetLoggedInUserId();
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Uuid == id);

            if (device == null) return NotFound();

            return View("Qr", device);
        }

        [HttpGet("GetQr/{id}")]
        public async Task<IActionResult> GetQr(Guid id)
        {
            try
            {
                var userId = GetLoggedInUserId();
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Uuid == id);

                if (device == null) return NotFound("Device not found");

                // Always generate local QR first as fallback
                var localQr = GenerateQrCodeImage($"whatsapp:{device.Id}:{device.Uuid}");

                // Try WhatsApp server if configured
                var waServerUrl = _config["WhatsAppSettings:ServerUrl"];
                if (!string.IsNullOrEmpty(waServerUrl))
                {
                    try
                    {
                        using var client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(10); // Add timeout

                        var payload = new
                        {
                            id = $"device_{device.Id}",
                            webhookUrl = device.HookUrl,
                            metadata = new { userId = userId }
                        };

                        var response = await client.PostAsync(
                            $"{waServerUrl}/sessions/add",
                            new StringContent(
                                JsonConvert.SerializeObject(payload),
                                Encoding.UTF8,
                                "application/json"));

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            try
                            {
                                dynamic result = JsonConvert.DeserializeObject(content);
                                if (result?.qrCode != null)
                                {
                                    return Ok(new
                                    {
                                        qr_code_url = result.qrCode.ToString(),
                                        connected = false,
                                        source = "server"
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to connect to WhatsApp server, using local QR");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to connect to WhatsApp server");
                    }
                }

                // Return local QR if server fails or not configured
                return Ok(new
                {
                    qr_code_url = $"data:image/png;base64,{localQr}",
                    connected = false,
                    source = "local"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQr endpoint");
                return StatusCode(500, new
                {
                    error = "QR generation failed",
                    message = ex.Message
                });
            }
        }

        [HttpPost("CheckConnection/{id}")]
        public async Task<IActionResult> CheckConnection(Guid id)
        {
            var userId = GetLoggedInUserId();
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Uuid == id);

            if (device == null) return NotFound();

            // Option 1: Check with real WhatsApp server if configured
            var waServerUrl = _config["WhatsAppSettings:ServerUrl"];
            if (!string.IsNullOrEmpty(waServerUrl))
            {
                try
                {
                    using var client = new HttpClient();
                    var response = await client.GetAsync($"{waServerUrl}/sessions/device_{device.Id}/status");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var status = JsonConvert.DeserializeObject<dynamic>(content);
                        return Ok(new { connected = status?.connected == true });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check connection with WhatsApp server");
                }
            }


            var isConnected = device.CreatedAt.HasValue &&
                             device.CreatedAt.Value.AddSeconds(10) <= DateTime.UtcNow;
            // Update status if changed
            if (isConnected != (device.Status == 1))
            {
                device.Status = isConnected ? 1 : 0;
                await _context.SaveChangesAsync();
            }

            return Ok(new { connected = isConnected });
        }

        [HttpPost("LogoutDevice/{id}")]
        public async Task<IActionResult> LogoutDevice(Guid id)
        {
            var userId = GetLoggedInUserId();
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Uuid == id);

            if (device == null) return NotFound();

            device.Status = 0;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private string GenerateQrCodeImage(string payload)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return Convert.ToBase64String(qrCode.GetGraphic(20));
        }

        private ulong GetLoggedInUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ulong.TryParse(userIdStr, out var result) ? result : 0;
        }

        private bool CheckDeviceLimit(ulong userId)
        {
            return _context.Devices.Count(d => d.UserId == userId) < 5;
        }
    }
}

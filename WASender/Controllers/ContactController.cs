using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using WASender.Services;
using Microsoft.Extensions.Configuration;

namespace WASender.Controllers
{
    public class ContactController : BaseController
    {
        private readonly IContactService _contactService;
        private readonly IConfiguration _configuration;

        public ContactController(IContactService contactService, IGlobalDataService globalDataService, ILogger<ContactController> logger, IConfiguration configuration)
      : base(globalDataService, logger)
        {
            _contactService = contactService;
            _configuration = configuration;
        }


        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync();
            var contactData = await _contactService.GetContactDataAsync();

            foreach (var item in contactData)
            {
                ViewData[item.Key] = item.Value;
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendMail(IFormCollection form)
        {
            try
            {
                string name = form["name"];
                string email = form["email"];
                string phone = form["phone"];
                string subject = form["subject"];
                string message = form["message"];

                string fromEmail = _configuration["SmtpSettings:FromEmail"];
                string fromName = _configuration["SmtpSettings:FromName"];
                string toEmail = _configuration["SmtpSettings:ToEmail"];
                string smtpHost = _configuration["SmtpSettings:Host"];
                int smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
                string smtpUser = _configuration["SmtpSettings:Username"];
                string smtpPass = _configuration["SmtpSettings:Password"];

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = $"<p>Name: {name}</p><p>Email: {email}</p><p>Phone: {phone}</p><p>Message: {message}</p>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                mailMessage.ReplyToList.Add(new MailAddress(email));

                await smtpClient.SendMailAsync(mailMessage);

                TempData["Success"] = "Thanks for contacting us. We will reach out to you soon.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Something went wrong: " + ex.InnerException?.Message ?? ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
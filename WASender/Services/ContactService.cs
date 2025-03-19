using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WASender.Models;

namespace WASender.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetContactDataAsync()
        {
            var contactOption = await _context.Options.FirstOrDefaultAsync(o => o.Key == "contact-page");

            if (contactOption != null)
            {
                var contactData = JsonConvert.DeserializeObject<dynamic>(contactOption.Value);

                return new Dictionary<string, string>
                {
                    ["Address"] = contactData?.address?.ToString() ?? "",
                    ["Country"] = contactData?.country?.ToString() ?? "",
                    ["MapLink"] = contactData?.map_link?.ToString() ?? "",
                    ["Contact1"] = contactData?.contact1?.ToString() ?? "",
                    ["Contact2"] = contactData?.contact2?.ToString() ?? "",
                    ["Email1"] = contactData?.email1?.ToString() ?? "",
                    ["Email2"] = contactData?.email2?.ToString() ?? ""
                };
            }

            return new Dictionary<string, string>();
        }

    }
}
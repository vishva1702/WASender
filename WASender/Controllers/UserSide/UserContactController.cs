using Microsoft.AspNetCore.Mvc;
using WASender.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace WASender.Controllers
{
    [Authorize]
    [Route("UserContact")] // Base route for the controller
    public class UserContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserContactController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: UserContact/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId(); // Get the logged-in user's ID

            // Count all contacts (static + user-added)
            var totalContacts = await _context.Contacts.CountAsync(); // This will count all contacts

            // Count only static contacts (direct database insertions)
            var totalStaticContacts = await _context.Contacts
                .Where(c => c.UserId == null || c.UserId == 0)
                .CountAsync();

            Console.WriteLine("Total Contacts (Static + User): " + totalContacts);
            Console.WriteLine("Total Static Contacts: " + totalStaticContacts);

            // Fetch user's contacts (Ensure it’s never null)
            var contacts = await _context.Contacts
                .Where(c => c.UserId == userId)
                .Include(c => c.Schedulecontacts)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync() ?? new List<Contact>();

            // Fetch active templates
            var templates = await _context.Templates
                .Where(t => t.UserId == userId && t.Status == 1)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Fetch active devices
            var devices = await _context.Devices
                .Where(d => d.UserId == userId && d.Status == 1)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            // Fetch user plan and decode JSON
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            int contactLimit = 0;

            if (user != null && !string.IsNullOrEmpty(user.Plan))
            {
                dynamic plan = JsonConvert.DeserializeObject(user.Plan);
                contactLimit = plan?.contact_limit ?? 0;
            }

            string limit = (contactLimit <= 0) ? totalContacts.ToString("N0") : $"{totalContacts} / {contactLimit}";

            // Fetch user groups
            var groups = await _context.Groups
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            ViewBag.TotalContacts = totalContacts;
            ViewBag.TotalStaticContacts = totalStaticContacts;
            ViewBag.Templates = templates;
            ViewBag.Devices = devices;
            ViewBag.Limit = limit;
            ViewBag.Groups = groups;

            return View(contacts);
        }


        //GET: UserContact/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ulong userId = GetUserId();
            var groups = await _context.Groups
                .Where(g => g.UserId == userId)
                .ToListAsync();

            ViewBag.Groups = groups;
            ViewBag.DebugMessage = "Create View Loaded"; // Debugging
            return View();
        }

        [HttpPost("Store")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(Contact contact, ulong? GroupId)
        {
            Console.WriteLine(" Store method called");

            if (!ModelState.IsValid)
            {
                Console.WriteLine(" ModelState is invalid");
                return RedirectToAction("Create");
            }

            Console.WriteLine($" Contact Name: {contact.Name}");
            Console.WriteLine($" Contact Phone: {contact.Phone}");
            Console.WriteLine($" Group ID: {GroupId}");

            ulong userId = GetUserId();
            contact.UserId = userId;
            contact.CreatedAt = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;

            _context.Contacts.Add(contact);
            int result = await _context.SaveChangesAsync();

            Console.WriteLine($"Rows affected: {result}");

            if (result > 0)
            {
                TempData["Success"] = "New Contact Created Successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create contact.";
            }

            return RedirectToAction("Index");
        }


        // GET: UserContact/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(ulong id)
        {
            ulong userId = GetUserId();
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        
        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Contact contact)
        {
            ulong userId = GetUserId();

            // Debugging log to check if the method is called and ID is received
            Console.WriteLine($"Update method called with ID: {contact.Id}");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return RedirectToAction("Index");
            }

            // Fetch the existing contact using the correct ID
            var existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == contact.Id && c.UserId == userId);

            if (existingContact == null)
            {
                TempData["ErrorMessage"] = "Contact not found.";
                return RedirectToAction("Index");
            }

            // Check if phone number exists for another contact
            bool isPhoneExists = await _context.Contacts
                .AnyAsync(c => c.UserId == userId && c.Phone == contact.Phone && c.Id != contact.Id);

            if (isPhoneExists)
            {
                TempData["ErrorMessage"] = "This contact number is already added.";
                return RedirectToAction("Index");
            }

            // Update the contact details
            existingContact.Name = contact.Name;
            existingContact.Phone = contact.Phone;
            existingContact.UpdatedAt = DateTime.UtcNow;

            _context.Contacts.Update(existingContact);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contact Updated Successfully!";
            return RedirectToAction("Index");
        }



        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(ulong id)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Contact Deleted Successfully!" });
        }

        // Helper method to get logged-in user ID
        private ulong GetUserId()
        {
            return 1; // Replace with actual user authentication logic
        }
    }
}
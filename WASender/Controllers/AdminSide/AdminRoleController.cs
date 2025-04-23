using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WASender.Models;
using WASender.Services;

namespace WASender.Controllers.AdminSide
{
    [Authorize(Roles = "admin,Admin")]
    public class AdminRoleController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminRoleController(IGlobalDataService globalDataService, ILogger<AdminHomeController> logger, ApplicationDbContext context)
            : base(globalDataService, logger)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await LoadGlobalDataAsync(); // ✅ Await the async method

            var roles = await _context.Roles
                .Where(r => r.Id != 1)
                .AsNoTracking()
                .ToListAsync(); // ✅ Use async version

            var rolePermissions = await _context.ModelHasPermissions
                .Include(m => m.Permission)
                .Where(m => m.ModelType == "Role")
                .AsNoTracking()
                .ToListAsync(); // ✅ Use async version

            foreach (var role in roles)
            {
                var permissions = rolePermissions
                    .Where(p => p.ModelId == role.Id)
                    .Select(p => p.Permission)
                    .ToList();

                role.Permissions = new List<Permission>(permissions);
            }

            ViewBag.Roles = roles;

            return View();
        }



        // GET: AdminRole/Create
        public async Task<IActionResult> Create()
        {
            await LoadGlobalDataAsync();    
            var permissionGroups = _context.Permissions
                .GroupBy(p => p.GroupName)
                .ToList()
                .Select(g => new PermissionGroup
                {
                    GroupName = g.Key,
                    Permissions = g.ToList()
                })
                .ToList();

            ViewBag.PermissionGroups = permissionGroups;
            return View();
        }

        // Helper class
        public class PermissionGroup
        {
            public string GroupName { get; set; }
            public List<Permission> Permissions { get; set; }
        }

        // POST: AdminRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string name, List<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Role name is required.";
                return RedirectToAction("Create");
            }

            if (_context.Roles.Any(r => r.Name == name))
            {
                TempData["Error"] = "Role name must be unique.";
                return RedirectToAction("Create");
            }

            // Save Role
            var newRole = new Role
            {
                Name = name,
                GuardName = "web",
                CreatedAt = DateTime.Now
            };

            _context.Roles.Add(newRole);
            _context.SaveChanges();

            // Sync permissions
            if (permissions != null && permissions.Any())
            {
                var selectedPermissions = _context.Permissions
                    .Where(p => permissions.Contains(p.Name))
                    .ToList();

                foreach (var perm in selectedPermissions)
                {
                    _context.ModelHasPermissions.Add(new ModelHasPermission
                    {
                        PermissionId = perm.Id,
                        ModelType = "Role",
                        ModelId = newRole.Id
                    });
                }

                _context.SaveChanges();
            }

            TempData["Success"] = "Role created successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(ulong id)
        {
            var role = _context.Roles.Find(id);
            if (role == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Role not found.",
                    redirect = Url.Action("Index", "AdminRole")
                });
            }

            _context.Roles.Remove(role);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Role Removed",
                redirect = Url.Action("Index", "AdminRole")
            });
        }


    }
}

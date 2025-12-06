using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.Data.Seeders;

namespace VirtualEventTicketing.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        private readonly IServiceProvider _serviceProvider;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger,
            IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList(); // UserManager.Users is already in memory
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: Admin/EditUserRoles/{id}
        public async Task<IActionResult> EditUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserId = id;
            ViewBag.UserEmail = user.Email;

            return View();
        }

        // POST: Admin/EditUserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            
            // Remove all current roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            // Add selected roles
            if (selectedRoles != null && selectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            _logger.LogInformation("Admin {AdminEmail} updated roles for user {UserEmail}", 
                User.Identity?.Name, user.Email);

            TempData["Success"] = $"Roles updated successfully for {user.Email}";
            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/DeleteAttendeeUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendeeUsers()
        {
            try
            {
                var deletedCount = await DatabaseSeeder.DeleteAttendeeUsersAsync(_serviceProvider);
                _logger.LogInformation("Admin {AdminEmail} deleted {Count} attendee user(s)", User.Identity?.Name, deletedCount);
                TempData["Success"] = $"Successfully deleted {deletedCount} attendee user(s). Admin users were preserved.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendee users");
                TempData["Error"] = $"An error occurred while deleting attendee users: {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/DeleteUserByEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email is required.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                var deleted = await DatabaseSeeder.DeleteUserByEmailAsync(_serviceProvider, email);
                if (deleted)
                {
                    _logger.LogInformation("Admin {AdminEmail} deleted user {Email}", User.Identity?.Name, email);
                    TempData["Success"] = $"User {email} has been deleted successfully.";
                }
                else
                {
                    TempData["Error"] = $"User {email} could not be deleted. User may not exist or has Admin role.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Email}", email);
                TempData["Error"] = $"An error occurred while deleting user: {ex.Message}";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}


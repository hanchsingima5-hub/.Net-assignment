using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using StudentManagementSystem.Data;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var model = new System.Collections.Generic.List<Models.AdminUserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new Models.AdminUserViewModel { Id = u.Id, Email = u.Email, Roles = roles });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var currentId = _userManager.GetUserId(User);
            if (string.Equals(currentId, id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "User is already an Admin.";
                return RedirectToAction(nameof(Users));
            }

            var addResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", addResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }

            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
            }

            TempData["SuccessMessage"] = "User promoted to Admin.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var currentId = _userManager.GetUserId(User);
            if (string.Equals(currentId, id, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You cannot remove your own Admin role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "User is not an Admin.";
                return RedirectToAction(nameof(Users));
            }

            var removeResult = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Users));
            }

            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            TempData["SuccessMessage"] = "Admin role removed.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest();

            var currentEmail = User?.Identity?.Name;
            if (string.Equals(currentEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["SuccessMessage"] = "User deleted.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
}

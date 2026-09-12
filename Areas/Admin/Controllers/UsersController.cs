using LMS.Web.Models;
using LMS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin,Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseService _courseService;

        public UsersController(UserManager<ApplicationUser> userManager, ICourseService courseService)
        {
            _userManager = userManager;
            _courseService = courseService;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string? search)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.Email!.ToLower().Contains(term) || 
                                         u.FirstName.ToLower().Contains(term) || 
                                         u.LastName.ToLower().Contains(term));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            ViewBag.Search = search;

            return View(users);
        }

        // GET: /Admin/Users/Details/guid
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var enrollments = await _courseService.GetStudentEnrollmentsAsync(id);

            ViewBag.Roles = roles;
            ViewBag.Enrollments = enrollments;

            return View(user);
        }

        // POST: /Admin/Users/Delete/guid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deleting Super Admin
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Super Admin"))
            {
                TempData["ErrorMessage"] = "Cannot delete a Super Admin user account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

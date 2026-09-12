using LMS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.TotalCourses = await _context.Courses.CountAsync(c => !c.IsDeleted);
            ViewBag.PendingDropRequests = await _context.Enrollments.CountAsync(e => e.DropStatus == "Pending" && !e.IsDeleted);

            return View();
        }
    }
}

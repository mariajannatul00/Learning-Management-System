using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LMS.Web.Models;
using LMS.Web.Data;
using LMS.Web.ViewModels.Home;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace LMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalCourses = await _context.Courses.CountAsync(c => c.IsPublished && !c.IsDeleted);
        var totalStudents = await _context.Users.CountAsync();
        var totalLessons = await _context.Lessons.CountAsync(l => !l.IsDeleted);
        var totalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);

        var featuredCourse = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.IsPublished && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)

            .FirstOrDefaultAsync();

        var viewModel = new HomeIndexViewModel
        {
            TotalCoursesCount = totalCourses,
            TotalStudentsCount = totalStudents,
            TotalLessonsCount = totalLessons,
            TotalCategoriesCount = totalCategories,
            FeaturedCourse = featuredCourse
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Features()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


using LMS.Web.Models;
using LMS.Web.Services.Interfaces;
using LMS.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ICourseService courseService, UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _userManager = userManager;
        }

        // GET: /Student/Dashboard
        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var enrollments = await _courseService.GetStudentEnrollmentsAsync(studentId);

            var viewModel = new StudentDashboardViewModel
            {
                Enrollments = enrollments,
                TotalEnrolledCourses = enrollments.Count(e => e.DropStatus != "Approved"),
                TotalPendingDropRequests = enrollments.Count(e => e.DropStatus == "Pending"),
                TotalCompletedCourses = 0 // Can be computed based on lesson completion
            };

            return View(viewModel);
        }

        // GET: /Student/Dashboard/Learn/5
        public async Task<IActionResult> Learn(int id, int? lessonId = null)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isEnrolled = await _courseService.IsStudentEnrolledAsync(studentId, id);

            if (!isEnrolled)
            {
                TempData["InfoMessage"] = "You must be enrolled in this course to access the content.";
                return RedirectToAction("Details", "Courses", new { area = "", id = id });
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var modules = (await _courseService.GetModulesByCourseIdAsync(id)).ToList();
            var completedLessonIds = (await _courseService.GetCompletedLessonIdsAsync(studentId, id)).ToList();

            var allLessons = modules.SelectMany(m => m.Lessons).ToList();
            var totalLessons = allLessons.Count;
            var completedCount = completedLessonIds.Count;
            double progressPct = totalLessons > 0 ? Math.Round(completedCount * 100.0 / totalLessons, 1) : 0;

            Lesson? selectedLesson = null;
            if (lessonId.HasValue)
            {
                selectedLesson = allLessons.FirstOrDefault(l => l.Id == lessonId.Value);
            }
            if (selectedLesson == null)
            {
                selectedLesson = allLessons.FirstOrDefault();
            }

            var viewModel = new CourseLearnViewModel
            {
                Course = course,
                Modules = modules,
                CompletedLessonIds = completedLessonIds,
                ActiveLesson = selectedLesson,
                TotalLessons = totalLessons,
                CompletedLessonsCount = completedCount,
                ProgressPercentage = progressPct
            };

            return View(viewModel);
        }


        // POST: /Student/Dashboard/ToggleLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLesson(int lessonId, int courseId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _courseService.ToggleLessonProgressAsync(studentId, lessonId);

            return RedirectToAction(nameof(Learn), new { id = courseId, lessonId = lessonId });
        }

        // POST: /Student/Dashboard/RequestDrop
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDrop(int enrollmentId, string reason)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _courseService.RequestCourseDropAsync(studentId, enrollmentId, reason);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using LMS.Web.Models;
using LMS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin,Admin")]
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ICourseService courseService, UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _userManager = userManager;
        }

        // GET: /Admin/Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesForAdminAsync();
            return View(courses);
        }

        // GET: /Admin/Courses/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _courseService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(new Course());
        }

        // POST: /Admin/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            course.InstructorId = currentUserId;

            ModelState.Remove(nameof(Course.InstructorId));
            ModelState.Remove(nameof(Course.Instructor));
            ModelState.Remove(nameof(Course.Category));
            ModelState.Remove(nameof(Course.Enrollments));

            if (!ModelState.IsValid)
            {
                var categories = await _courseService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", course.CategoryId);
                return View(course);
            }

            var success = await _courseService.CreateCourseAsync(course);
            if (success)
            {
                TempData["SuccessMessage"] = "New course created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to create course.";
            var cats = await _courseService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(cats, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /Admin/Courses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var categories = await _courseService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // POST: /Admin/Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return BadRequest();

            ModelState.Remove(nameof(Course.InstructorId));
            ModelState.Remove(nameof(Course.Instructor));
            ModelState.Remove(nameof(Course.Category));
            ModelState.Remove(nameof(Course.Enrollments));

            if (!ModelState.IsValid)
            {
                var categories = await _courseService.GetAllCategoriesAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", course.CategoryId);
                return View(course);
            }

            var success = await _courseService.UpdateCourseAsync(course);
            if (success)
            {
                TempData["SuccessMessage"] = "Course information updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update course.";
            var cats = await _courseService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(cats, "Id", "Name", course.CategoryId);
            return View(course);
        }

        // GET: /Admin/Courses/ManageContent/5
        public async Task<IActionResult> ManageContent(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            var modules = await _courseService.GetModulesByCourseIdAsync(id);
            ViewBag.Modules = modules;

            return View(course);
        }

        // POST: /Admin/Courses/AddModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddModule(int courseId, string title, int orderIndex)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course != null && course.IsEnded)
            {
                TempData["ErrorMessage"] = "❌ Cannot add course materials because the course timeline has ended.";
                return RedirectToAction(nameof(ManageContent), new { id = courseId });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Module title cannot be empty.";
                return RedirectToAction(nameof(ManageContent), new { id = courseId });
            }

            var module = new Module
            {
                CourseId = courseId,
                Title = title.Trim(),
                OrderIndex = orderIndex <= 0 ? 1 : orderIndex
            };

            var success = await _courseService.AddModuleAsync(module);
            if (success)
            {
                TempData["SuccessMessage"] = "New module added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add module. Course timeline may have ended.";
            }

            return RedirectToAction(nameof(ManageContent), new { id = courseId });
        }

        // POST: /Admin/Courses/AddLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(int courseId, int moduleId, string title, string videoUrl, int durationMinutes, string contentSummary, bool isFreePreview, int orderIndex)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course != null && course.IsEnded)
            {
                TempData["ErrorMessage"] = "❌ Cannot upload course materials because the course timeline has ended.";
                return RedirectToAction(nameof(ManageContent), new { id = courseId });
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(videoUrl))
            {
                TempData["ErrorMessage"] = "Lesson title and video URL are required.";
                return RedirectToAction(nameof(ManageContent), new { id = courseId });
            }

            var lesson = new Lesson
            {
                ModuleId = moduleId,
                Title = title.Trim(),
                VideoUrl = videoUrl.Trim(),
                DurationMinutes = durationMinutes <= 0 ? 10 : durationMinutes,
                ContentSummary = contentSummary ?? string.Empty,
                IsFreePreview = isFreePreview,
                OrderIndex = orderIndex <= 0 ? 1 : orderIndex
            };

            var success = await _courseService.AddLessonAsync(lesson);
            if (success)
            {
                TempData["SuccessMessage"] = "New video lecture added to course!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add video lesson. Course timeline may have ended.";
            }

            return RedirectToAction(nameof(ManageContent), new { id = courseId });
        }


        // POST: /Admin/Courses/DeleteLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId, int courseId)
        {
            var success = await _courseService.DeleteLessonAsync(lessonId);
            if (success)
            {
                TempData["SuccessMessage"] = "Lesson video removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove lesson video.";
            }

            return RedirectToAction(nameof(ManageContent), new { id = courseId });
        }

        // POST: /Admin/Courses/DeleteModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteModule(int moduleId, int courseId)
        {
            var success = await _courseService.DeleteModuleAsync(moduleId);
            if (success)
            {
                TempData["SuccessMessage"] = "Module removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove module.";
            }

            return RedirectToAction(nameof(ManageContent), new { id = courseId });
        }

        // POST: /Admin/Courses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _courseService.DeleteCourseAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete course.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


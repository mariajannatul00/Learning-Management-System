using LMS.Web.Models;
using LMS.Web.Services.Interfaces;
using LMS.Web.ViewModels.Course;
using LMS.Web.ViewModels.Payment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.Web.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ISslCommerzService _sslCommerzService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(
            ICourseService courseService,
            ISslCommerzService sslCommerzService,
            UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _sslCommerzService = sslCommerzService;
            _userManager = userManager;
        }

        // GET: /Courses or /Courses/Index
        public async Task<IActionResult> Index(string? search, int? categoryId, string level = "all", string sortBy = "newest")
        {
            var courses = await _courseService.GetPublishedCoursesAsync(search, categoryId, level, sortBy);
            var categories = await _courseService.GetAllCategoriesAsync();

            var enrolledCourseIds = new List<int>();
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var studentEnrollments = await _courseService.GetStudentEnrollmentsAsync(currentUserId);
                enrolledCourseIds = studentEnrollments
                    .Where(e => e.PaymentStatus == "Completed" && e.DropStatus != "Approved")
                    .Select(e => e.CourseId)
                    .ToList();
            }

            var viewModel = new CourseExploreViewModel
            {
                Courses = courses,
                Categories = categories,
                Search = search,
                CategoryId = categoryId,
                Level = level,
                SortBy = sortBy,
                TotalCoursesCount = courses.Count(),
                EnrolledCourseIds = enrolledCourseIds
            };

            return View(viewModel);
        }


        // GET: /Courses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEnrolled = !string.IsNullOrEmpty(currentUserId) && await _courseService.IsStudentEnrolledAsync(currentUserId, id);

            var relatedCourses = (await _courseService.GetPublishedCoursesAsync(categoryId: course.CategoryId))
                .Where(c => c.Id != id)
                .Take(3);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                IsUserEnrolled = isEnrolled,
                RelatedCourses = relatedCourses
            };

            return View(viewModel);
        }

        // POST: /Courses/Buy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int courseId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["InfoMessage"] = "Please sign in or create an account to purchase this course.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Courses", new { id = courseId }) });
            }

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.FindByIdAsync(studentId);
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var initResult = await _courseService.InitiateEnrollmentAsync(studentId, courseId);
            if (!initResult.Success || initResult.Enrollment == null)
            {
                TempData["InfoMessage"] = initResult.Message;
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var enrollment = initResult.Enrollment;

            var successUrl = Url.Action("PaymentSuccess", "Courses", null, Request.Scheme) ?? "";
            var failUrl = Url.Action("PaymentFail", "Courses", null, Request.Scheme) ?? "";
            var cancelUrl = Url.Action("PaymentCancel", "Courses", null, Request.Scheme) ?? "";

            var gatewayUrl = await _sslCommerzService.InitiatePaymentAsync(
                enrollment.TransactionId,
                enrollment.PricePaid,
                course.Title,
                user?.FullName ?? "Student",
                user?.Email ?? "student@example.com",
                successUrl,
                failUrl,
                cancelUrl);

            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                return Redirect(gatewayUrl);
            }

            // Render interactive SSLCommerz Sandbox Gateway page
            var bankTranId = $"BANK-SSL-{System.Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var sandboxViewModel = new SslCommerzSandboxViewModel
            {
                TransactionId = enrollment.TransactionId,
                BankTranId = bankTranId,
                Amount = enrollment.PricePaid,
                CourseTitle = course.Title,
                CourseId = course.Id,
                StudentName = user?.FullName ?? "Student",
                StudentEmail = user?.Email ?? "student@example.com",
                StoreId = "lms6a7f32ae9cbfa"
            };

            return View("SslCommerzSandbox", sandboxViewModel);
        }



        // POST/GET: /Courses/PaymentSuccess
        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentSuccess(string? tran_id, string? val_id, string? bank_tran_id)
        {
            tran_id ??= Request.Form["tran_id"].FirstOrDefault() ?? Request.Query["tran_id"].FirstOrDefault();
            bank_tran_id ??= Request.Form["bank_tran_id"].FirstOrDefault() ?? Request.Query["bank_tran_id"].FirstOrDefault();
            val_id ??= Request.Form["val_id"].FirstOrDefault() ?? Request.Query["val_id"].FirstOrDefault();

            if (string.IsNullOrEmpty(bank_tran_id) && !string.IsNullOrEmpty(val_id))
            {
                bank_tran_id = val_id;
            }

            if (string.IsNullOrEmpty(bank_tran_id))
            {
                bank_tran_id = $"BANK-SSL-{System.Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            }

            if (!string.IsNullOrEmpty(tran_id))
            {
                await _courseService.CompleteEnrollmentAsync(tran_id, bank_tran_id);
                TempData["SuccessMessage"] = "Payment completed successfully via SSLCommerz Payment Gateway Sandbox!";
                return RedirectToAction("Index", "Dashboard", new { area = "Student" });
            }

            TempData["ErrorMessage"] = "Payment validation failed.";
            return RedirectToAction(nameof(Index));
        }


        // POST/GET: /Courses/PaymentFail
        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult PaymentFail(string? tran_id)
        {
            TempData["ErrorMessage"] = "Payment failed through SSLCommerz sandbox gateway.";
            return RedirectToAction(nameof(Index));
        }

        // POST/GET: /Courses/PaymentCancel
        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult PaymentCancel(string? tran_id)
        {
            TempData["InfoMessage"] = "Payment process was cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }
}


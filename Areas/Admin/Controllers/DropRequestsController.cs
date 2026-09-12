using LMS.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Super Admin,Admin")]
    public class DropRequestsController : Controller
    {
        private readonly ICourseService _courseService;

        public DropRequestsController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: /Admin/DropRequests
        public async Task<IActionResult> Index()
        {
            var pendingRequests = await _courseService.GetPendingDropRequestsAsync();
            var allRequests = await _courseService.GetAllDropRequestsAsync();
            var statementRecords = await _courseService.GetAllEnrollmentsForStatementAsync();

            ViewBag.AllRequests = allRequests;
            ViewBag.StatementRecords = statementRecords;
            return View(pendingRequests);
        }

        // GET: /Admin/DropRequests/BankStatement
        public async Task<IActionResult> BankStatement()
        {
            var statementRecords = await _courseService.GetAllEnrollmentsForStatementAsync();
            return View(statementRecords);
        }

        // POST: /Admin/DropRequests/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int enrollmentId)
        {
            var result = await _courseService.ApproveDropRequestAsync(enrollmentId);
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

        // POST: /Admin/DropRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int enrollmentId)
        {
            var result = await _courseService.RejectDropRequestAsync(enrollmentId);
            if (result.Success)
            {
                TempData["InfoMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


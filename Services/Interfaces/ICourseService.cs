using LMS.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Web.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetPublishedCoursesAsync(string? search = null, int? categoryId = null, string? level = null, string? sortBy = null);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
        Task<bool> IsStudentEnrolledAsync(string studentId, int courseId);
        Task<(bool Success, string Message, Enrollment? Enrollment)> BuyCourseAsync(string studentId, int courseId);
        Task<(bool Success, string Message, Enrollment? Enrollment)> InitiateEnrollmentAsync(string studentId, int courseId);
        Task<bool> CompleteEnrollmentAsync(string transactionId, string bankTranId);

        // Student Content & Drop Request Methods
        Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(string studentId);
        Task<Course?> GetCourseContentAsync(int courseId);
        Task<IEnumerable<int>> GetCompletedLessonIdsAsync(string studentId, int courseId);
        Task<bool> ToggleLessonProgressAsync(string studentId, int lessonId);
        Task<(bool Success, string Message)> RequestCourseDropAsync(string studentId, int enrollmentId, string reason);
        Task<bool> AddModuleAsync(Module module);
        Task<bool> DeleteModuleAsync(int moduleId);
        Task<bool> AddLessonAsync(Lesson lesson);
        Task<bool> DeleteLessonAsync(int lessonId);
        Task<IEnumerable<Module>> GetModulesByCourseIdAsync(int courseId);

        // Admin Course CRUD & Drop Request Methods
        Task<IEnumerable<Course>> GetAllCoursesForAdminAsync();
        Task<bool> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<IEnumerable<Enrollment>> GetPendingDropRequestsAsync();
        Task<IEnumerable<Enrollment>> GetAllDropRequestsAsync();
        Task<IEnumerable<Enrollment>> GetAllEnrollmentsForStatementAsync();
        Task<(bool Success, string Message)> ApproveDropRequestAsync(int enrollmentId);
        Task<(bool Success, string Message)> RejectDropRequestAsync(int enrollmentId);
    }
}


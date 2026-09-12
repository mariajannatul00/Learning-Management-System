using LMS.Web.Models;
using System.Collections.Generic;

namespace LMS.Web.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public IEnumerable<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public int TotalEnrolledCourses { get; set; }
        public int TotalCompletedCourses { get; set; }
        public int TotalPendingDropRequests { get; set; }
    }
}

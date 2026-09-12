using LMS.Web.Models;
using System.Collections.Generic;

namespace LMS.Web.ViewModels.Course
{
    public class CourseDetailsViewModel
    {
        public LMS.Web.Models.Course Course { get; set; } = default!;
        public bool IsUserEnrolled { get; set; } = false;
        public IEnumerable<LMS.Web.Models.Course> RelatedCourses { get; set; } = new List<LMS.Web.Models.Course>();
    }
}

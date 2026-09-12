using LMS.Web.Models;
using System.Collections.Generic;

namespace LMS.Web.ViewModels.Course
{
    public class CourseExploreViewModel
    {
        public IEnumerable<LMS.Web.Models.Course> Courses { get; set; } = new List<LMS.Web.Models.Course>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string Level { get; set; } = "all";
        public string SortBy { get; set; } = "newest";

        public int TotalCoursesCount { get; set; }
        public List<int> EnrolledCourseIds { get; set; } = new List<int>();
    }
}


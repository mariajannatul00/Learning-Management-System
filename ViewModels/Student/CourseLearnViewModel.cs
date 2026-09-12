using LMS.Web.Models;
using System.Collections.Generic;

namespace LMS.Web.ViewModels.Student
{
    public class CourseLearnViewModel
    {
        public LMS.Web.Models.Course Course { get; set; } = default!;
        public IEnumerable<Module> Modules { get; set; } = new List<Module>();
        public IEnumerable<int> CompletedLessonIds { get; set; } = new List<int>();
        public Lesson? ActiveLesson { get; set; }
        public double ProgressPercentage { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessonsCount { get; set; }
    }
}

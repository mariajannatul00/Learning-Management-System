namespace LMS.Web.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public int TotalCoursesCount { get; set; }
        public int TotalStudentsCount { get; set; }
        public int TotalLessonsCount { get; set; }
        public int TotalCategoriesCount { get; set; }
        public LMS.Web.Models.Course? FeaturedCourse { get; set; }
    }
}

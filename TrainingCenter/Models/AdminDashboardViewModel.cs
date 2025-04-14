namespace TrainingCenter.Models
{
    public class AdminDashboardViewModel
    {
        public Admin Admin { get; set; }
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
        public int EnrollmentCount { get; set; }
        public int OpenCourses { get; set; }
    }
}
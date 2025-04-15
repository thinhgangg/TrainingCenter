using System.Collections.Generic;

namespace TrainingCenter.Models
{
    public class DashboardViewModel
    {
        public Student Student { get; set; }
        public List<CourseWithEnrollment> EnrolledCourses { get; set; }
        public List<CourseWithEnrollment> OpenCourses { get; set; }
    }

    public class CourseWithEnrollment
    {
        public Course Course { get; set; }
        public int EnrolledCount { get; set; }
    }
}   
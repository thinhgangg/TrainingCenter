using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace TrainingCenter.Models
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<TrainingCenterContext>
    {
        protected override void Seed(TrainingCenterContext context)
        {
            var courses = new List<Course>
            {
                new Course { CourseName = "Lập trình C#", Instructor = "Nguyễn Văn A", StartDate = new DateTime(2024, 5, 10), Fee = 2000000, MaxStudents = 20 },
                new Course { CourseName = "Cơ sở dữ liệu", Instructor = "Trần Thị B", StartDate = new DateTime(2024, 6, 5), Fee = 1500000, MaxStudents = 30 }
            };
            courses.ForEach(c => context.Courses.Add(c));
            context.SaveChanges();

            var students = new List<Student>
            {
                new Student { FullName = "Lê Văn Hùng", Dob = new DateTime(2000, 1, 15), Email = "hunglv@example.com", Username = "hunglv", Password = "123456" },
                new Student { FullName = "Nguyễn Thị Mai", Dob = new DateTime(1999, 12, 3), Email = "main@example.com", Username = "main", Password = "123456" }
            };
            students.ForEach(s => context.Students.Add(s));
            context.SaveChanges();

            var enrollments = new List<Enrollment>
            {
                new Enrollment { StudentId = students[0].StudentId, CourseId = courses[0].CourseId, RegisterDate = DateTime.Now },
                new Enrollment { StudentId = students[1].StudentId, CourseId = courses[1].CourseId, RegisterDate = DateTime.Now }
            };
            enrollments.ForEach(e => context.Enrollments.Add(e));
            context.SaveChanges();
        }
    }
}

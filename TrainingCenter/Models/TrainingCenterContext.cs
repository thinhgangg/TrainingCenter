using System.Data.Entity;

namespace TrainingCenter.Models
{
    public class TrainingCenterContext : DbContext
    {
        public TrainingCenterContext()
            : base("name=TrainingCenterContext")
        {
            Database.SetInitializer<TrainingCenterContext>(null);
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Admin> Admins { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingCenter.Models
{
    [Table("enrollments")]
    public class Enrollment
    {
        [Key]
        [Column("enrollment_id")]
        public int EnrollmentId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Display(Name = "Ngày đăng ký")]
        [Column("register_date")]
        public DateTime RegisterDate { get; set; }

        // Navigation properties (optional)
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
    }
}

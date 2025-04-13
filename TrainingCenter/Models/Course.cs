using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingCenter.Models
{
    [Table("courses")]
    public class Course
    {
        [Key]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Display(Name = "Tên khóa học")]
        [Column("course_name")]
        public string CourseName { get; set; }

        [Display(Name = "Giảng viên")]
        [Column("instructor")]
        public string Instructor { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Học phí")]
        [Column("fee")]
        public int Fee { get; set; }

        [Display(Name = "Số lượng tối đa")]
        [Column("max_students")]
        public int? MaxStudents { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}

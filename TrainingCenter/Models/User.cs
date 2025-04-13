using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingCenter.Models
{
    [Table("users")]
    public class User
    {
        public User()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Display(Name = "Họ và tên")]
        [Column("full_name")]
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; }

        [Display(Name = "Ngày sinh")]
        [Column("dob")]
        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Display(Name = "Số điện thoại")]
        [Column("phone")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        [Column("email")]
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; }

        [Display(Name = "Tên người dùng")]
        [Column("username")]
        [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên người dùng không được vượt quá 50 ký tự.")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Column("password")]
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Quyền")]
        [Column("role")]
        [Required]
        public string Role { get; set; } = "user";

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
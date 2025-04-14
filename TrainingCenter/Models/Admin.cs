using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingCenter.Models
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("full_name")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column("email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        [Column("username")]
        [Display(Name = "Tên người dùng")]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Column("password")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace TrainingCenter.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        public string RoleName { get; set; }
    }
}
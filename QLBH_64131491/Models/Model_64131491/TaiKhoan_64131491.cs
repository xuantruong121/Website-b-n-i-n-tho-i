using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("TaiKhoan")]
    public class TaiKhoan_64131491
    {
        [Key]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [Display(Name = "Mật khẩu")]
        [StringLength(50)]
        public string MatKhau { get; set; } = null!;

        [Display(Name = "Quyền Admin")]
        public bool RoleAdmin { get; set; } = false;

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Đã xác thực")]
        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public virtual KhachHang_64131491? KhachHang { get; set; }
        public virtual NhanVien_64131491? NhanVien { get; set; }
    }
} 
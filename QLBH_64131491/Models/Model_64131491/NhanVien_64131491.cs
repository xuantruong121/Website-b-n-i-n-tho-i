using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("NhanViens")]
    public class NhanVien_64131491
    {
        [Key]
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [Display(Name = "Mã nhân viên")]
        [StringLength(50)]
        public string MaNV { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Display(Name = "Quyền sử dụng")]
        [StringLength(50)]
        public string? QuyenSuDung { get; set; }

        // Navigation properties
        [ForeignKey("Email")]
        public virtual TaiKhoan_64131491? TaiKhoan { get; set; }
        public virtual ICollection<Order_64131491> Orders { get; set; } = new List<Order_64131491>();
    }
} 
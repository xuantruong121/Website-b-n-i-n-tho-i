using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("KhachHangs")]
    public class KhachHang_64131491
    {
        [Key]
        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [Display(Name = "Mã khách hàng")]
        [StringLength(50)]
        public string MaKH { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? DiaChi { get; set; }

        // Navigation properties
        [ForeignKey("Email")]
        public virtual TaiKhoan_64131491? TaiKhoan { get; set; }
        public virtual ICollection<Order_64131491> Orders { get; set; } = new List<Order_64131491>();
        public virtual ICollection<ProductReview_64131491> ProductReviews { get; set; } = new List<ProductReview_64131491>();
        public virtual ICollection<Cart_64131491> CartItems { get; set; } = new List<Cart_64131491>();
    }
} 
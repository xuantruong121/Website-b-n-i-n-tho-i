using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("Orders")]
    public class Order_64131491
    {
        [Key]
        [Required(ErrorMessage = "Mã đơn hàng không được để trống")]
        [Display(Name = "Mã đơn hàng")]
        [StringLength(50)]
        public string OrderId { get; set; } = null!;

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [Display(Name = "Mã khách hàng")]
        [StringLength(50)]
        public string MaKH { get; set; } = null!;

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Địa chỉ giao hàng")]
        public string? ShippingAddress { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Đã thanh toán")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Đã giao hàng")]
        public bool IsShipped { get; set; } = false;

        [Display(Name = "Đang xử lý")]
        public bool IsProcessing { get; set; } = false;

        [Display(Name = "Đã hoàn thành")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Đã hủy")]
        public bool IsCancelled { get; set; } = false;

        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? TotalAmount { get; set; }

        [Display(Name = "Giảm giá")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "Thành tiền")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? FinalAmount { get; set; }

        [Display(Name = "Nhân viên xử lý")]
        [StringLength(50)]
        public string? MaNVXuLy { get; set; }

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("MaKH")]
        public virtual KhachHang_64131491? KhachHang { get; set; }

        [ForeignKey("MaNVXuLy")]
        public virtual NhanVien_64131491? NhanVien { get; set; }

        public virtual ICollection<OrderDetail_64131491> OrderDetails { get; set; } = new List<OrderDetail_64131491>();
    }
} 
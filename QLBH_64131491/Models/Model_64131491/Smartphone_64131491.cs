using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("Smartphones")]
    public class Smartphone_64131491
    {
        [Key]
        [Display(Name = "Mã sản phẩm")]
        [StringLength(50)]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Thương hiệu không được để trống")]
        [Display(Name = "Thương hiệu")]
        [StringLength(50)]
        public string Brand { get; set; } = null!;

        [Required(ErrorMessage = "Model không được để trống")]
        [Display(Name = "Model")]
        [StringLength(100)]
        public string Model { get; set; } = null!;

        [Required(ErrorMessage = "Giá không được để trống")]
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        [Display(Name = "Giá khuyến mãi")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? DiscountPrice { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hình ảnh")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Màu sắc")]
        [StringLength(50)]
        public string? Color { get; set; }

        [Display(Name = "Bộ nhớ (GB)")]
        public int? StorageGB { get; set; }

        [Display(Name = "RAM")]
        [StringLength(20)]
        public string? RAM { get; set; }

        [Display(Name = "CPU")]
        [StringLength(100)]
        public string? Processor { get; set; }

        [Display(Name = "Camera")]
        [StringLength(100)]
        public string? Camera { get; set; }

        [Display(Name = "Pin")]
        [StringLength(50)]
        public string? Battery { get; set; }

        [Display(Name = "Màn hình")]
        [StringLength(50)]
        public string? ScreenSize { get; set; }

        [Display(Name = "Hệ điều hành")]
        [StringLength(50)]
        public string? OS { get; set; }

        [Display(Name = "Ngày phát hành")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Đánh giá")]
        [Column(TypeName = "decimal(3,1)")]
        public decimal? Rating { get; set; }

        [Display(Name = "Còn hàng")]
        public bool InStock { get; set; } = true;

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "Sản phẩm nổi bật")]
        public bool Featured { get; set; } = false;

        [Display(Name = "Sản phẩm mới")]
        public bool IsNew { get; set; } = false;

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Loại sản phẩm")]
        [StringLength(50)]
        public string? MaLSP { get; set; }

        // Navigation properties
        [ForeignKey("MaLSP")]
        public virtual LoaiSanPham_64131491? LoaiSanPham { get; set; }
        public virtual ICollection<ProductImage_64131491> ProductImages { get; set; } = new List<ProductImage_64131491>();
        public virtual ICollection<ProductReview_64131491> ProductReviews { get; set; } = new List<ProductReview_64131491>();
        public virtual ICollection<OrderDetail_64131491> OrderDetails { get; set; } = new List<OrderDetail_64131491>();
        public virtual ICollection<Cart_64131491> CartItems { get; set; } = new List<Cart_64131491>();
    }
} 
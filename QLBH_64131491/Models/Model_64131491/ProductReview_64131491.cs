using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("ProductReviews")]
    public class ProductReview_64131491
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [Display(Name = "Mã sản phẩm")]
        [StringLength(50)]
        public string SmartphoneId { get; set; } = null!;

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [Display(Name = "Mã khách hàng")]
        [StringLength(50)]
        public string MaKH { get; set; } = null!;

        [Required(ErrorMessage = "Đánh giá không được để trống")]
        [Display(Name = "Đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; }

        [Display(Name = "Bình luận")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã duyệt")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "Đã ẩn")]
        public bool IsHidden { get; set; } = false;

        // Navigation properties
        [ForeignKey("SmartphoneId")]
        public virtual Smartphone_64131491? Smartphone { get; set; }

        [ForeignKey("MaKH")]
        public virtual KhachHang_64131491? KhachHang { get; set; }
    }
} 
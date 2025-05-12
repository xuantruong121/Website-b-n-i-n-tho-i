using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("Cart")]
    public class Cart_64131491
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        [Display(Name = "Mã khách hàng")]
        [StringLength(50)]
        public string MaKH { get; set; } = null!;

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [Display(Name = "Mã sản phẩm")]
        [StringLength(50)]
        public string SmartphoneId { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Ngày thêm")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("MaKH")]
        public virtual KhachHang_64131491? KhachHang { get; set; }

        [ForeignKey("SmartphoneId")]
        public virtual Smartphone_64131491? Smartphone { get; set; }
    }
} 
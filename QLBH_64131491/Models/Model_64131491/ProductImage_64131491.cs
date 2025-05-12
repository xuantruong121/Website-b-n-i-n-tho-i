using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("ProductImages")]
    public class ProductImage_64131491
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [Display(Name = "Mã sản phẩm")]
        [StringLength(50)]
        public string SmartphoneId { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ảnh không được để trống")]
        [Display(Name = "URL hình ảnh")]
        [StringLength(255)]
        public string ImageUrl { get; set; } = null!;

        [Display(Name = "Hình ảnh chính")]
        public bool IsPrimary { get; set; } = false;

        // Navigation properties
        [ForeignKey("SmartphoneId")]
        public virtual Smartphone_64131491? Smartphone { get; set; }
    }
} 
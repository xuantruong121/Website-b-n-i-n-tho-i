using System.ComponentModel.DataAnnotations;

namespace QLBH_64131491.Models.Model_64131491
{
    public class LoaiSanPham_64131491
    {
        [Key]
        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        [Display(Name = "Mã loại sản phẩm")]
        public string MaLSP { get; set; }

        [Required(ErrorMessage = "Tên loại sản phẩm không được để trống")]
        [Display(Name = "Tên loại sản phẩm")]
        public string TenLSP { get; set; }

        // Navigation property
        public virtual ICollection<Smartphone_64131491> Smartphones { get; set; } = new List<Smartphone_64131491>();
    }
} 
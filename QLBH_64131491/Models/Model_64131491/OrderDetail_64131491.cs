using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_64131491.Models.Model_64131491
{
    [Table("OrderDetails")]
    public class OrderDetail_64131491
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string OrderId { get; set; } = null!;

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SmartphoneId { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Display(Name = "Đơn giá")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Giảm giá")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Discount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order_64131491? Order { get; set; }

        [ForeignKey("SmartphoneId")]
        public virtual Smartphone_64131491? Smartphone { get; set; }
    }
} 
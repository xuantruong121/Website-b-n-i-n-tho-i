using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Models.Model_64131491;

namespace QLBH_64131491.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LoaiSanPham_64131491> LoaiSanPhams { get; set; }
        public DbSet<Smartphone_64131491> Smartphones { get; set; }
        public DbSet<ProductImage_64131491> ProductImages { get; set; }
        public DbSet<ProductReview_64131491> ProductReviews { get; set; }
        public DbSet<KhachHang_64131491> KhachHangs { get; set; }
        public DbSet<TaiKhoan_64131491> TaiKhoans { get; set; }
        public DbSet<Order_64131491> Orders { get; set; }
        public DbSet<OrderDetail_64131491> OrderDetails { get; set; }
        public DbSet<Cart_64131491> Cart { get; set; }
        public DbSet<NhanVien_64131491> NhanViens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure sequences
            modelBuilder.HasSequence<int>("seq_KhachHang")
                .StartsAt(4)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("seq_Order")
                .StartsAt(4)
                .IncrementsBy(1);

            // Configure composite key for OrderDetail
            modelBuilder.Entity<OrderDetail_64131491>()
                .HasKey(od => new { od.OrderId, od.SmartphoneId });

            // Configure unique constraint for Cart
            modelBuilder.Entity<Cart_64131491>()
                .HasIndex(c => new { c.MaKH, c.SmartphoneId })
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<Smartphone_64131491>()
                .HasOne(s => s.LoaiSanPham)
                .WithMany(l => l.Smartphones)
                .HasForeignKey(s => s.MaLSP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KhachHang_64131491>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang_64131491>(k => k.Email)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order_64131491>()
                .HasOne(o => o.KhachHang)
                .WithMany(k => k.Orders)
                .HasForeignKey(o => o.MaKH)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order_64131491>()
                .HasOne(o => o.NhanVien)
                .WithMany(n => n.Orders)
                .HasForeignKey(o => o.MaNVXuLy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes for Smartphones
            modelBuilder.Entity<Smartphone_64131491>()
                .HasIndex(s => s.Brand);

            modelBuilder.Entity<Smartphone_64131491>()
                .HasIndex(s => s.Price);

            modelBuilder.Entity<Smartphone_64131491>()
                .HasIndex(s => s.DiscountPrice);

            modelBuilder.Entity<Smartphone_64131491>()
                .HasIndex(s => s.ReleaseDate);
        }
    }
}

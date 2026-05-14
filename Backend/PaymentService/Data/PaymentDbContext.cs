using PaymentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }
    public DbSet<InventoryHold> InventoryHolds { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigurePayments(modelBuilder);
        ConfigurePaymentRefunds(modelBuilder);
        ConfigureInventoryHolds(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureProductVariants(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("NGUOIDUNG");
            e.HasKey(x => x.MaNguoiDung);
            e.Property(x => x.MaNguoiDung).ValueGeneratedOnAdd();
            e.Property(x => x.HoTen).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.SoDienThoai).HasMaxLength(20).IsRequired();
            e.Property(x => x.MatKhauHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("DONHANG");
            e.HasKey(x => x.MaDonHang);
            e.Property(x => x.MaDonHang).ValueGeneratedOnAdd();
            e.Property(x => x.MaDonHangKinhDoanh).HasMaxLength(50).IsRequired();
            e.Property(x => x.TongThanhToan).HasPrecision(18, 2);
            e.Property(x => x.TrangThaiDonHang).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.TrangThaiThanhToan).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayThanhToanThanhCong).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayHuyDon).HasColumnType("datetime2(0)");
            e.Property(x => x.LyDoHuyDon).HasMaxLength(500);
            e.Property(x => x.LoaiDonHang).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.TienDatCoc).HasPrecision(18, 2);
            e.Property(x => x.SoTienConLai).HasPrecision(18, 2);

            e.HasMany(x => x.Payments)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);

            e.HasMany(x => x.InventoryHolds)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);
        });
    }

    private static void ConfigurePayments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("THANHTOAN");
            e.HasKey(x => x.MaThanhToan);
            e.Property(x => x.MaThanhToan).ValueGeneratedOnAdd();
            e.Property(x => x.MaThanhToanKinhDoanh).HasMaxLength(50).IsRequired();
            e.Property(x => x.SoTien).HasPrecision(18, 2);
            e.Property(x => x.PhuongThuc).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.MaGiaoDich).HasMaxLength(120);
            e.Property(x => x.DaThanhToanLuc).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.LoaiThanhToan).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.SoTienHoan).HasPrecision(18, 2);
            e.Property(x => x.NoiDungChuyenKhoan).HasMaxLength(500);
            e.Property(x => x.MaNganHang).HasMaxLength(50);
            e.Property(x => x.LyDoHuy).HasMaxLength(500);
            e.Property(x => x.NgayHuy).HasColumnType("datetime2(0)");

            e.HasIndex(x => x.MaThanhToanKinhDoanh).IsUnique();

            e.HasMany(x => x.Refunds)
                .WithOne(x => x.Payment)
                .HasForeignKey(x => x.MaThanhToan);
        });
    }

    private static void ConfigurePaymentRefunds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentRefund>(e =>
        {
            e.ToTable("THANHTOAN_HOANTIEN");
            e.HasKey(x => x.MaHoanTien);
            e.Property(x => x.MaHoanTien).ValueGeneratedOnAdd();
            e.Property(x => x.SoTienHoan).HasPrecision(18, 2);
            e.Property(x => x.MaGiaoDichHoanTien).HasMaxLength(120);
            e.Property(x => x.LyDo).HasMaxLength(500);
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");

            e.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.MaDonHang);
        });
    }

    private static void ConfigureInventoryHolds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryHold>(e =>
        {
            e.ToTable("TONKHO_GIUCHO");
            e.HasKey(x => x.MaGiuCho);
            e.Property(x => x.MaGiuCho).ValueGeneratedOnAdd();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.HetHanLuc).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(x => x.GhiChu).HasMaxLength(500);
        });
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("SANPHAM");
            e.HasKey(x => x.MaSanPham);
            e.Property(x => x.SoLuongTon).IsRequired();
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private static void ConfigureProductVariants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("BIENSANPHAM");
            e.HasKey(x => x.MaBienSanPham);
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }
}

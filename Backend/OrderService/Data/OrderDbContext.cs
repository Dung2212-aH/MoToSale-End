using OrderService.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderHistory> OrderHistories { get; set; }
    public DbSet<InventoryHold> InventoryHolds { get; set; }
    public DbSet<OrderVoucher> OrderVouchers { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<VoucherUser> VoucherUsers { get; set; }
    public DbSet<VoucherValidationResult> VoucherValidationResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureUserAddresses(modelBuilder);
        ConfigureProducts(modelBuilder);
        ConfigureProductVariants(modelBuilder);
        ConfigureCarts(modelBuilder);
        ConfigureCartItems(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderItems(modelBuilder);
        ConfigureOrderHistories(modelBuilder);
        ConfigureInventoryHolds(modelBuilder);
        ConfigureOrderVouchers(modelBuilder);
        ConfigureVouchers(modelBuilder);
        ConfigureVoucherUsers(modelBuilder);
        ConfigureStoredProcedureResults(modelBuilder);
    }

    private static void ConfigureVouchers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Voucher>(e =>
        {
            e.ToTable("VOUCHER");
            e.HasKey(x => x.MaVoucher);
            e.Property(x => x.MaVoucher).ValueGeneratedOnAdd();
            e.Property(x => x.MaVoucherCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.LoaiGiamGia).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.GiaTriGiam).HasPrecision(18, 2);
            e.Property(x => x.GiaTriDonToiThieu).HasPrecision(18, 2);
            e.Property(x => x.GiaTriGiamToiDa).HasPrecision(18, 2);
            e.Property(x => x.NgayBatDau).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayKetThuc).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(x => x.MoTa).HasMaxLength(500);
            e.Property(x => x.PhamViApDung).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.ApDungLoaiDonHang).HasMaxLength(50).IsUnicode(false);
        });
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

    private static void ConfigureUserAddresses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAddress>(e =>
        {
            e.ToTable("NGUOIDUNG_DIACHI");
            e.HasKey(x => x.MaDiaChi);
            e.Property(x => x.MaDiaChi).ValueGeneratedOnAdd();
            e.Property(x => x.HoTenNhanHang).HasMaxLength(150).IsRequired();
            e.Property(x => x.SoDienThoaiNhanHang).HasMaxLength(20).IsRequired();
            e.Property(x => x.DiaChiNhanHang).HasMaxLength(255).IsRequired();
            e.Property(x => x.PhuongXa).HasMaxLength(100);
            e.Property(x => x.QuanHuyen).HasMaxLength(100);
            e.Property(x => x.TinhThanh).HasMaxLength(100).IsRequired();
            e.Property(x => x.GhiChu).HasMaxLength(255);
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private static void ConfigureProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("SANPHAM", table =>
            {
                table.HasTrigger("trg_SANPHAM_Validate_HangXe_DongXe");
            });
            e.HasKey(x => x.MaSanPham);
            e.Property(x => x.MaSanPham).ValueGeneratedOnAdd();
            e.Ignore(x => x.MaShowroom);
            e.Property(x => x.MaSanPhamKinhDoanh).HasMaxLength(50).IsRequired();
            e.Property(x => x.TenSanPham).HasMaxLength(255).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(280).IsRequired();
            e.Property(x => x.MoTaNgan).HasMaxLength(500);
            e.Property(x => x.GiaGoc).HasPrecision(18, 2);
            e.Property(x => x.GiaKhuyenMai).HasPrecision(18, 2);
            e.Property(x => x.AnhChinhUrl).HasMaxLength(500);
            e.Property(x => x.TrangThaiSanPham).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(x => x.MaSanPhamKinhDoanh).IsUnique();
            e.HasIndex(x => x.Slug).IsUnique();
        });
    }

    private static void ConfigureProductVariants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("BIENSANPHAM", table =>
            {
                table.HasTrigger("trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM");
            });
            e.HasKey(x => x.MaBienSanPham);
            e.Property(x => x.MaBienSanPham).ValueGeneratedOnAdd();
            e.Property(x => x.TenBienThe).HasMaxLength(180).IsRequired();
            e.Property(x => x.SKU).HasMaxLength(80).IsRequired();
            e.Property(x => x.GiaGhiDe).HasPrecision(18, 2);
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.PhienBan).HasMaxLength(100);
            e.Property(x => x.MauSac).HasMaxLength(80);
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(x => x.SKU).IsUnique();
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.MaSanPham);
        });
    }

    private static void ConfigureCarts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(e =>
        {
            e.ToTable("GIOHANG");
            e.HasKey(x => x.MaGioHang);
            e.Property(x => x.MaGioHang).ValueGeneratedOnAdd();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasMany(x => x.Items)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.MaGioHang);
        });
    }

    private static void ConfigureCartItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("CHITIET_GIOHANG", table =>
            {
                table.HasTrigger("trg_CHITIET_GIOHANG_Validate_MaBienSanPham");
            });
            e.HasKey(x => x.MaChiTietGioHang);
            e.Property(x => x.MaChiTietGioHang).ValueGeneratedOnAdd();
            e.Property(x => x.DonGia).HasPrecision(18, 2);
            e.Property(x => x.ThanhTien)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("CONVERT([decimal](18,2),[DonGia]*[SoLuong])", stored: true);
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.MaSanPham);

            e.HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.MaBienSanPham);
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
            e.Property(x => x.HoTenNhanHang).HasMaxLength(150).IsRequired();
            e.Property(x => x.SoDienThoaiNhanHang).HasMaxLength(20).IsRequired();
            e.Property(x => x.EmailNhanHang).HasMaxLength(255);
            e.Property(x => x.DiaChiNhanHang).HasMaxLength(255).IsRequired();
            e.Property(x => x.TongTienHang).HasPrecision(18, 2);
            e.Property(x => x.TienGiam).HasPrecision(18, 2);
            e.Property(x => x.PhiVanChuyen).HasPrecision(18, 2);
            e.Property(x => x.TongThanhToan).HasPrecision(18, 2);
            e.Property(x => x.TrangThaiDonHang).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.TrangThaiThanhToan).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.GhiChu).HasMaxLength(1000);
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayThanhToanThanhCong).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayHuyDon).HasColumnType("datetime2(0)");
            e.Property(x => x.LyDoHuyDon).HasMaxLength(500);
            e.Property(x => x.PhuongThucNhanHang).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.TrangThaiVanChuyen).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.LoaiDonHang).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.TienDatCoc).HasPrecision(18, 2);
            e.Property(x => x.SoTienConLai).HasPrecision(18, 2);
            e.Property(x => x.NgayHenNhanXe).HasColumnType("datetime2(0)");
            e.Property(x => x.GhiChuGiaoNhan).HasMaxLength(500);

            e.HasIndex(x => x.MaDonHangKinhDoanh).IsUnique();

            e.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);

            e.HasMany(x => x.InventoryHolds)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);

            e.HasMany(x => x.Vouchers)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);

            e.HasMany(x => x.Histories)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.MaDonHang);
        });
    }

    private static void ConfigureOrderHistories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderHistory>(e =>
        {
            e.ToTable("DONHANG_LICHSU_TRANGTHAI");
            e.HasKey(x => x.MaLichSuDonHang);
            e.Property(x => x.MaLichSuDonHang).ValueGeneratedOnAdd();
            e.Property(x => x.LoaiSuKien).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(x => x.GiaTriCu).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.GiaTriMoi).HasMaxLength(50).IsUnicode(false);
            e.Property(x => x.GhiChu).HasMaxLength(500);
            e.Property(x => x.ThoiGian).HasColumnType("datetime2(0)");

            e.HasOne(x => x.Order)
                .WithMany(x => x.Histories)
                .HasForeignKey(x => x.MaDonHang);
        });
    }

    private static void ConfigureOrderItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("CHITIET_DONHANG", table =>
            {
                table.HasTrigger("trg_CHITIET_DONHANG_Validate_MaBienSanPham");
            });
            e.HasKey(x => x.MaChiTietDonHang);
            e.Property(x => x.MaChiTietDonHang).ValueGeneratedOnAdd();
            e.Property(x => x.TenSanPhamSnapshot).HasMaxLength(255).IsRequired();
            e.Property(x => x.SKUSnapshot).HasMaxLength(80);
            e.Property(x => x.DonGia).HasPrecision(18, 2);
            e.Property(x => x.ThanhTien)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("CONVERT([decimal](18,2),[DonGia]*[SoLuong])", stored: true);

            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.MaSanPham);

            e.HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.MaBienSanPham);
        });
    }

    private static void ConfigureInventoryHolds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryHold>(e =>
        {
            e.ToTable("TONKHO_GIUCHO", table =>
            {
                table.HasTrigger("trg_TONKHO_GIUCHO_Validate_MaBienSanPham");
            });
            e.HasKey(x => x.MaGiuCho);
            e.Property(x => x.MaGiuCho).ValueGeneratedOnAdd();
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.HetHanLuc).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(x => x.GhiChu).HasMaxLength(500);
        });
    }

    private static void ConfigureOrderVouchers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderVoucher>(e =>
        {
            e.ToTable("DONHANG_VOUCHER");
            e.HasKey(x => new { x.MaDonHang, x.MaVoucher });
            e.Property(x => x.MaVoucherCodeSnapshot).HasMaxLength(50).IsRequired();
            e.Property(x => x.SoTienGiam).HasPrecision(18, 2);
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");
            e.Property(x => x.LoaiGiamGiaSnapshot).HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.GiaTriGiamSnapshot).HasPrecision(18, 2);
        });
    }

    private static void ConfigureVoucherUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoucherUser>(e =>
        {
            e.ToTable("VOUCHER_NGUOIDUNG");
            e.HasKey(x => x.MaVoucherNguoiDung);
            e.Property(x => x.MaVoucherNguoiDung).ValueGeneratedOnAdd();
            e.Property(x => x.MaVoucherCodeSnapshot).HasMaxLength(50).IsRequired();
            e.Property(x => x.LoaiGiamGiaSnapshot).HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.GiaTriGiamSnapshot).HasPrecision(18, 2);
            e.Property(x => x.SoTienGiam).HasPrecision(18, 2);
            e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(x => x.NgaySuDung).HasColumnType("datetime2(0)");
            e.Property(x => x.NgayTao).HasColumnType("datetime2(0)");

            e.HasOne(x => x.Voucher)
                .WithMany()
                .HasForeignKey(x => x.MaVoucher);
        });
    }

    private static void ConfigureStoredProcedureResults(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VoucherValidationResult>(e =>
        {
            e.HasNoKey();
            e.ToView(null);
        });
    }
}

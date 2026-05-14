using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ContactRequest> ContactRequests { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<PartCompatibility> PartCompatibilities { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Showroom> Showrooms { get; set; }
    public DbSet<VehicleModel> VehicleModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBrand(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureContactRequest(modelBuilder);
        ConfigureFaq(modelBuilder);
        ConfigureFavorite(modelBuilder);
        ConfigurePartCompatibility(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureProductImage(modelBuilder);
        ConfigureProductReview(modelBuilder);
        ConfigureProductVariant(modelBuilder);
        ConfigureShowroom(modelBuilder);
        ConfigureVehicleModel(modelBuilder);
    }

    private void ConfigureBrand(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(e =>
        {
            e.ToTable("HANGXE");
            e.HasKey(e => e.MaHangXe);
            e.Property(e => e.MaHangXe).ValueGeneratedOnAdd();
            e.Property(e => e.TenHang).HasMaxLength(100).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(150).IsRequired();
            e.Property(e => e.LogoUrl).HasMaxLength(500);
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.TenHang).IsUnique();
            e.HasIndex(e => e.Slug).IsUnique();
        });
    }

    private void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("DANHMUC");
            e.HasKey(e => e.MaDanhMuc);
            e.Property(e => e.MaDanhMuc).ValueGeneratedOnAdd();
            e.Property(e => e.TenDanhMuc).HasMaxLength(150).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(180).IsRequired();
            e.Property(e => e.MoTa).HasMaxLength(500);
            e.Property(e => e.ThuTuHienThi).IsRequired();
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.Slug).IsUnique();
        });
    }

    private void ConfigureContactRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContactRequest>(e =>
        {
            e.ToTable("LIENHE_YEUCAU");
            e.HasKey(e => e.MaLienHe);
            e.Property(e => e.MaLienHe).ValueGeneratedOnAdd();
            e.Property(e => e.HoTen).HasMaxLength(150).IsRequired();
            e.Property(e => e.SoDienThoai).HasMaxLength(20).IsRequired();
            e.Property(e => e.Email).HasMaxLength(255);
            e.Property(e => e.TieuDe).HasMaxLength(255);
            e.Property(e => e.NoiDung).IsRequired();
            e.Property(e => e.LoaiYeuCau).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.DaXuLyLuc).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigureFaq(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Faq>(e =>
        {
            e.ToTable("FAQ");
            e.HasKey(e => e.MaFAQ);
            e.Property(e => e.MaFAQ).ValueGeneratedOnAdd();
            e.Property(e => e.CauHoi).HasMaxLength(500).IsRequired();
            e.Property(e => e.CauTraLoi).IsRequired();
            e.Property(e => e.DanhMuc).HasMaxLength(100);
            e.Property(e => e.ThuTuHienThi).IsRequired();
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigureFavorite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorite>(e =>
        {
            e.ToTable("YEUTHICH");
            e.HasKey(e => new { e.MaNguoiDung, e.MaSanPham });
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigurePartCompatibility(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartCompatibility>(e =>
        {
            e.ToTable("PHUTUNG_TUONGTHICH");
            e.HasKey(e => e.MaTuongThich);
            e.Property(e => e.MaTuongThich).ValueGeneratedOnAdd();
            e.Property(e => e.GhiChu).HasMaxLength(500);
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigurePost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(e =>
        {
            e.ToTable("BAIVIET");
            e.HasKey(e => e.MaBaiViet);
            e.Property(e => e.MaBaiViet).ValueGeneratedOnAdd();
            e.Property(e => e.TieuDe).HasMaxLength(255).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(280).IsRequired();
            e.Property(e => e.TomTat).HasMaxLength(500);
            e.Property(e => e.NoiDung).IsRequired();
            e.Property(e => e.AnhDaiDienUrl).HasMaxLength(500);
            e.Property(e => e.DanhMuc).HasMaxLength(100);
            e.Property(e => e.XuatBanLuc).HasColumnType("datetime2(0)");
            e.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.Slug).IsUnique();
        });
    }

    private void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("SANPHAM");
            e.HasKey(e => e.MaSanPham);
            e.Property(e => e.MaSanPham).ValueGeneratedOnAdd();
            e.Property(e => e.MaSanPhamKinhDoanh).HasMaxLength(50).IsRequired();
            e.Property(e => e.TenSanPham).HasMaxLength(255).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(280).IsRequired();
            e.Property(e => e.LoaiSanPham).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.MoTaNgan).HasMaxLength(500);
            e.Property(e => e.GiaGoc).HasPrecision(18, 2).IsRequired();
            e.Property(e => e.GiaKhuyenMai).HasPrecision(18, 2);
            e.Property(e => e.AnhChinhUrl).HasMaxLength(500);
            e.Property(e => e.TrangThaiSanPham).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.MaSanPhamKinhDoanh).IsUnique();
            e.HasIndex(e => e.Slug).IsUnique();
        });
    }

    private void ConfigureProductImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.ToTable("ANHSANPHAM");
            e.HasKey(e => e.MaAnhSanPham);
            e.Property(e => e.MaAnhSanPham).ValueGeneratedOnAdd();
            e.Property(e => e.UrlAnh).HasMaxLength(500).IsRequired();
            e.Property(e => e.AltText).HasMaxLength(255);
            e.Property(e => e.LaAnhChinh).IsRequired();
            e.Property(e => e.ThuTuHienThi).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");

            e.HasIndex(e => new { e.MaSanPham, e.ThuTuHienThi });
        });
    }

    private void ConfigureProductReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReview>(e =>
        {
            e.ToTable("DANHGIASANPHAM");
            e.HasKey(e => e.MaDanhGia);
            e.Property(e => e.MaDanhGia).ValueGeneratedOnAdd();
            e.Property(e => e.Diem).HasColumnType("tinyint").IsRequired();
            e.Property(e => e.TieuDe).HasMaxLength(255);
            e.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigureProductVariant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("BIENSANPHAM");
            e.HasKey(e => e.MaBienSanPham);
            e.Property(e => e.MaBienSanPham).ValueGeneratedOnAdd();
            e.Property(e => e.TenBienThe).HasMaxLength(180).IsRequired();
            e.Property(e => e.SKU).HasMaxLength(80).IsRequired();
            e.Property(e => e.GiaGhiDe).HasPrecision(18, 2);
            e.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.PhienBan).HasMaxLength(100);
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");
            e.Property(e => e.MauSac).HasMaxLength(80);

            e.HasIndex(e => e.SKU).IsUnique();
        });
    }

    private void ConfigureShowroom(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Showroom>(e =>
        {
            e.ToTable("SHOWROOM");
            e.HasKey(e => e.MaShowroom);
            e.Property(e => e.MaShowroom).ValueGeneratedOnAdd();
            e.Property(e => e.TenShowroom).HasMaxLength(180).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(220).IsRequired();
            e.Property(e => e.DiaChi).HasMaxLength(255).IsRequired();
            e.Property(e => e.SoDienThoai).HasMaxLength(20);
            e.Property(e => e.Email).HasMaxLength(255);
            e.Property(e => e.GioMoCua).HasMaxLength(255);
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.Slug).IsUnique();
        });
    }

    private void ConfigureVehicleModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleModel>(e =>
        {
            e.ToTable("DONGXE");
            e.HasKey(e => e.MaDongXe);
            e.Property(e => e.MaDongXe).ValueGeneratedOnAdd();
            e.Property(e => e.TenDongXe).HasMaxLength(120).IsRequired();
            e.Property(e => e.Slug).HasMaxLength(160).IsRequired();
            e.Property(e => e.DangHoatDong).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => new { e.MaHangXe, e.TenDongXe }).IsUnique();
            e.HasIndex(e => e.Slug).IsUnique();
        });
    }
}

using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureUserAddresses(modelBuilder);
        ConfigureRoles(modelBuilder);
        ConfigureUserRoles(modelBuilder);
    }

    private void ConfigureUserAddresses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAddress>(e =>
        {
            e.ToTable("NGUOIDUNG_DIACHI");
            e.HasKey(e => e.MaDiaChi);
            e.Property(e => e.MaDiaChi).ValueGeneratedOnAdd();
            e.Property(e => e.HoTenNhanHang).HasMaxLength(150).IsRequired();
            e.Property(e => e.SoDienThoaiNhanHang).HasMaxLength(20).IsRequired();
            e.Property(e => e.DiaChiNhanHang).HasMaxLength(255).IsRequired();
            e.Property(e => e.PhuongXa).HasMaxLength(100);
            e.Property(e => e.QuanHuyen).HasMaxLength(100);
            e.Property(e => e.TinhThanh).HasMaxLength(100).IsRequired();
            e.Property(e => e.GhiChu).HasMaxLength(255);
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");
        });
    }

    private void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("NGUOIDUNG");
            e.HasKey(e => e.Id);
            e.Property(e => e.Id).HasColumnName("MaNguoiDung");
            e.Property(e => e.HoTen).HasMaxLength(150).IsRequired();
            e.Property(e => e.Email).HasMaxLength(255).IsRequired();
            e.Property(e => e.SoDienThoai).HasMaxLength(20).IsRequired();
            e.Property(e => e.MatKhau).HasColumnName("MatKhauHash").HasMaxLength(500).IsRequired();
            e.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).IsRequired();
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");
            e.Property(e => e.NgayCapNhat).HasColumnType("datetime2(0)");

            e.HasIndex(e => e.Email).IsUnique();
            e.HasIndex(e => e.SoDienThoai).IsUnique();
        });
    }

    private void ConfigureRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("VAITRO");
            e.HasKey(e => e.Id);
            e.Property(e => e.Id).HasColumnName("MaVaiTro").HasColumnType("tinyint");
            e.Property(e => e.TenVaiTro).HasMaxLength(30).IsUnicode(false).IsRequired();
            e.Property(e => e.MoTa).HasMaxLength(255);

            e.HasIndex(e => e.TenVaiTro).IsUnique();
        });
    }

    private void ConfigureUserRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("NGUOIDUNG_VAITRO");
            e.HasKey(e => new { e.UserId, e.RoleId });
            e.Property(e => e.UserId).HasColumnName("MaNguoiDung");
            e.Property(e => e.RoleId).HasColumnName("MaVaiTro").HasColumnType("tinyint");
            e.Property(e => e.NgayTao).HasColumnType("datetime2(0)");

            e.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });
    }
}

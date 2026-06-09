using CatalogService.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services;

/// <summary>
/// Tao cac bang phu tro (khong quan ly bang EF migration) neu chua ton tai.
/// Theo dung quy uoc cua AuditLogService.EnsureTableAsync.
/// </summary>
public static class CatalogSchema
{
    public static async Task EnsureBannerTableAsync(CatalogDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.BANNER_TRANGCHU', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.BANNER_TRANGCHU (
                    MaBanner INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    ViTri VARCHAR(40) NOT NULL,
                    TieuDe NVARCHAR(255) NULL,
                    UrlAnh NVARCHAR(500) NOT NULL,
                    LienKet NVARCHAR(500) NULL,
                    ThuTuHienThi INT NOT NULL DEFAULT 0,
                    DangHoatDong BIT NOT NULL DEFAULT 1,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_BANNER_TRANGCHU_ViTri ON dbo.BANNER_TRANGCHU (ViTri, ThuTuHienThi);
            END;
            """);
    }

    public static async Task EnsureRelatedTableAsync(CatalogDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.SANPHAM_LIENQUAN', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.SANPHAM_LIENQUAN (
                    MaLienQuan INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaSanPham INT NOT NULL,
                    MaSanPhamLienQuan INT NOT NULL,
                    LoaiLienQuan VARCHAR(20) NOT NULL DEFAULT 'Accessory',
                    GhiChu NVARCHAR(500) NULL,
                    ThuTuHienThi INT NOT NULL DEFAULT 0,
                    DangHoatDong BIT NOT NULL DEFAULT 1,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL,
                    CONSTRAINT UX_SANPHAM_LIENQUAN UNIQUE (MaSanPham, MaSanPhamLienQuan)
                );
            END;
            """);
    }
}

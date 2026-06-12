USE [ShowroomDB]
GO
/****** Object:  Table [dbo].[BIENSANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BIENSANPHAM](
	[MaBienSanPham] [int] IDENTITY(1,1) NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[TenBienThe] [nvarchar](180) NOT NULL,
	[SKU] [nvarchar](80) NOT NULL,
	[GiaGhiDe] [decimal](18, 2) NULL,
	[SoLuongTon] [int] NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[PhienBan] [nvarchar](100) NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
	[MauSac] [nvarchar](80) NULL,
 CONSTRAINT [PK_BIENSANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaBienSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ANHSANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ANHSANPHAM](
	[MaAnhSanPham] [int] IDENTITY(1,1) NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[UrlAnh] [nvarchar](500) NOT NULL,
	[AltText] [nvarchar](255) NULL,
	[LaAnhChinh] [bit] NOT NULL,
	[ThuTuHienThi] [int] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[MaBienSanPham] [int] NULL,
 CONSTRAINT [PK_ANHSANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaAnhSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SANPHAM](
	[MaSanPham] [int] IDENTITY(1,1) NOT NULL,
	[MaSanPhamKinhDoanh] [nvarchar](50) NOT NULL,
	[TenSanPham] [nvarchar](255) NOT NULL,
	[Slug] [nvarchar](280) NOT NULL,
	[MaDanhMuc] [int] NOT NULL,
	[MaHangXe] [int] NULL,
	[MaDongXe] [int] NULL,
	[MoTaNgan] [nvarchar](500) NULL,
	[MoTa] [nvarchar](max) NULL,
	[GiaGoc] [decimal](18, 2) NOT NULL,
	[GiaKhuyenMai] [decimal](18, 2) NULL,
	[SoLuongTon] [int] NOT NULL,
	[AnhChinhUrl] [nvarchar](500) NULL,
	[DangHoatDong] [bit] NOT NULL,
	[TrangThaiSanPham] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
	[LoaiSanPham] [varchar](20) NOT NULL,
 CONSTRAINT [PK_SANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_SANPHAM_BIENTHE_ANH]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    FIX 10: Dọn lỗi còn sót sau khi xóa ANHSANPHAM.MauSac và ANHSANPHAM.PhienBan

    Vấn đề:
    - Bảng ANHSANPHAM hiện đã không còn cột MauSac, PhienBan.
    - Nhưng view dbo.v_SANPHAM_BIENTHE_ANH vẫn còn tham chiếu:
        a.MauSac
        a.PhienBan
    - Khi query view này sẽ lỗi Invalid column name 'MauSac' / 'PhienBan'.

    Cách sửa:
    - Tạo lại view v_SANPHAM_BIENTHE_ANH.
    - Chỉ lấy ảnh theo MaBienSanPham.
    - Nếu biến thể chưa có ảnh riêng, fallback sang ảnh chung của sản phẩm.
*/

CREATE   VIEW [dbo].[v_SANPHAM_BIENTHE_ANH]
AS
SELECT
    sp.MaSanPham,
    sp.TenSanPham,
    sp.Slug,
    sp.LoaiSanPham,

    bt.MaBienSanPham,
    bt.TenBienThe,
    bt.SKU,
    bt.PhienBan,
    bt.MauSac,

    GiaBan = COALESCE(bt.GiaGhiDe, sp.GiaKhuyenMai, sp.GiaGoc),
    bt.SoLuongTon,

    MaAnhSanPham = a.MaAnhSanPham,
    UrlAnh = COALESCE(a.UrlAnh, sp.AnhChinhUrl),
    AltText = COALESCE(a.AltText, sp.TenSanPham),
    LaAnhChinh = ISNULL(a.LaAnhChinh, CONVERT(bit, 0)),
    ThuTuHienThi = ISNULL(a.ThuTuHienThi, 0)
FROM dbo.SANPHAM sp
INNER JOIN dbo.BIENSANPHAM bt
    ON bt.MaSanPham = sp.MaSanPham
OUTER APPLY
(
    SELECT TOP (1)
        a.MaAnhSanPham,
        a.UrlAnh,
        a.AltText,
        a.LaAnhChinh,
        a.ThuTuHienThi
    FROM dbo.ANHSANPHAM a
    WHERE a.MaSanPham = sp.MaSanPham
      AND (
            a.MaBienSanPham = bt.MaBienSanPham
            OR a.MaBienSanPham IS NULL
          )
    ORDER BY
        CASE WHEN a.MaBienSanPham = bt.MaBienSanPham THEN 0 ELSE 1 END,
        CASE WHEN a.LaAnhChinh = 1 THEN 0 ELSE 1 END,
        a.ThuTuHienThi,
        a.MaAnhSanPham
) a;

GO
/****** Object:  Table [dbo].[PHUTUNG_TUONGTHICH]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PHUTUNG_TUONGTHICH](
	[MaTuongThich] [int] IDENTITY(1,1) NOT NULL,
	[MaPhuTung] [int] NOT NULL,
	[MaHangXe] [int] NULL,
	[MaDongXe] [int] NULL,
	[NamTu] [smallint] NULL,
	[NamDen] [smallint] NULL,
	[ApDungTatCaXe] [bit] NOT NULL,
	[GhiChu] [nvarchar](500) NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PHUTUNG_TUONGTHICH] PRIMARY KEY CLUSTERED 
(
	[MaTuongThich] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HANGXE]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HANGXE](
	[MaHangXe] [int] IDENTITY(1,1) NOT NULL,
	[TenHang] [nvarchar](100) NOT NULL,
	[Slug] [nvarchar](150) NOT NULL,
	[LogoUrl] [nvarchar](500) NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_HANGXE] PRIMARY KEY CLUSTERED 
(
	[MaHangXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DONGXE]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DONGXE](
	[MaDongXe] [int] IDENTITY(1,1) NOT NULL,
	[MaHangXe] [int] NOT NULL,
	[TenDongXe] [nvarchar](120) NOT NULL,
	[Slug] [nvarchar](160) NOT NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_DONGXE] PRIMARY KEY CLUSTERED 
(
	[MaDongXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_PHUTUNG_TUONGTHICH]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 5) View de backend/frontend lay phu tung kem ten hang/dong xe */
CREATE   VIEW [dbo].[v_PHUTUNG_TUONGTHICH]
AS
SELECT
    ptt.MaTuongThich,
    ptt.MaPhuTung,
    sp.TenSanPham AS TenPhuTung,
    sp.Slug AS SlugPhuTung,
    sp.AnhChinhUrl,
    sp.GiaGoc,
    sp.GiaKhuyenMai,
    sp.SoLuongTon,
    sp.DangHoatDong AS PhuTungDangHoatDong,
    ptt.MaHangXe,
    hx.TenHang,
    ptt.MaDongXe,
    dx.TenDongXe,
    ptt.NamTu,
    ptt.NamDen,
    ptt.ApDungTatCaXe,
    ptt.GhiChu,
    ptt.DangHoatDong,
    ptt.NgayTao,
    ptt.NgayCapNhat
FROM dbo.PHUTUNG_TUONGTHICH ptt
JOIN dbo.SANPHAM sp ON sp.MaSanPham = ptt.MaPhuTung
LEFT JOIN dbo.HANGXE hx ON hx.MaHangXe = ptt.MaHangXe
LEFT JOIN dbo.DONGXE dx ON dx.MaDongXe = ptt.MaDongXe;

GO
/****** Object:  View [dbo].[v_SANPHAM_TONKHO_KIEMTRA]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------------------------------------
-- 4) View kiểm tra tồn kho sản phẩm và biến thể
--------------------------------------------------------------------------------

CREATE   VIEW [dbo].[v_SANPHAM_TONKHO_KIEMTRA]
AS
SELECT
    sp.MaSanPham,
    sp.TenSanPham,
    sp.LoaiSanPham,
    sp.SoLuongTon AS SoLuongTon_TrongSANPHAM,
    COUNT(bsp.MaBienSanPham) AS SoBienThe,
    ISNULL(SUM(ISNULL(bsp.SoLuongTon, 0)), 0) AS TongSoLuongTon_BienThe,
    CASE
        WHEN COUNT(bsp.MaBienSanPham) = 0 THEN N'Không có biến thể - dùng tồn kho SANPHAM'
        WHEN sp.SoLuongTon = ISNULL(SUM(ISNULL(bsp.SoLuongTon, 0)), 0) THEN N'Đã đồng bộ'
        ELSE N'Lệch tồn kho'
    END AS TrangThaiKiemTra
FROM dbo.SANPHAM sp
LEFT JOIN dbo.BIENSANPHAM bsp
    ON bsp.MaSanPham = sp.MaSanPham
GROUP BY
    sp.MaSanPham,
    sp.TenSanPham,
    sp.LoaiSanPham,
    sp.SoLuongTon;

GO
/****** Object:  View [dbo].[v_ANHSANPHAM_THEO_BIENTHE]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------------------------------------
-- 2) Tạo/Sửa view ảnh theo biến thể để lấy MauSac/PhienBan từ BIENSANPHAM
--------------------------------------------------------------------------------

CREATE   VIEW [dbo].[v_ANHSANPHAM_THEO_BIENTHE]
AS
SELECT
    a.MaAnhSanPham,
    a.MaSanPham,
    a.MaBienSanPham,
    sp.TenSanPham,
    bsp.PhienBan,
    bsp.MauSac,
    bsp.SKU,
    a.UrlAnh,
    a.AltText,
    a.LaAnhChinh,
    a.ThuTuHienThi,
    a.NgayTao
FROM dbo.ANHSANPHAM a
INNER JOIN dbo.SANPHAM sp
    ON sp.MaSanPham = a.MaSanPham
LEFT JOIN dbo.BIENSANPHAM bsp
    ON bsp.MaBienSanPham = a.MaBienSanPham;

GO
/****** Object:  Table [dbo].[TONKHO_GIUCHO]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TONKHO_GIUCHO](
	[MaGiuCho] [int] IDENTITY(1,1) NOT NULL,
	[MaDonHang] [int] NOT NULL,
	[MaChiTietDonHang] [int] NULL,
	[MaSanPham] [int] NOT NULL,
	[MaBienSanPham] [int] NULL,
	[SoLuong] [int] NOT NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[HetHanLuc] [datetime2](0) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
	[GhiChu] [nvarchar](500) NULL,
 CONSTRAINT [PK_TONKHO_GIUCHO] PRIMARY KEY CLUSTERED 
(
	[MaGiuCho] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_TONKHO_KHADUNG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

----------------------------------------------------------------------
-- 3) View tính tồn kho khả dụng theo sản phẩm / biến thể
--    Lưu ý: giữ chỗ hết hạn tự động không bị tính vào tồn kho đang giữ.
----------------------------------------------------------------------
CREATE   VIEW [dbo].[v_TONKHO_KHADUNG]
AS
    SELECT
        sp.MaSanPham,
        CAST(NULL AS INT) AS MaBienSanPham,
        sp.TenSanPham,
        CAST(NULL AS NVARCHAR(180)) AS TenBienThe,
        sp.SoLuongTon AS TonKhoThucTe,
        ISNULL(gc.SoLuongDangGiu, 0) AS SoLuongDangGiu,
        sp.SoLuongTon - ISNULL(gc.SoLuongDangGiu, 0) AS TonKhoKhaDung
    FROM dbo.SANPHAM sp
    OUTER APPLY
    (
        SELECT SUM(g.SoLuong) AS SoLuongDangGiu
        FROM dbo.TONKHO_GIUCHO g
        WHERE g.MaSanPham = sp.MaSanPham
          AND g.MaBienSanPham IS NULL
          AND g.TrangThai = 'Active'
          AND g.HetHanLuc > SYSDATETIME()
    ) gc

    UNION ALL

    SELECT
        sp.MaSanPham,
        bt.MaBienSanPham,
        sp.TenSanPham,
        bt.TenBienThe,
        ISNULL(bt.SoLuongTon, 0) AS TonKhoThucTe,
        ISNULL(gc.SoLuongDangGiu, 0) AS SoLuongDangGiu,
        ISNULL(bt.SoLuongTon, 0) - ISNULL(gc.SoLuongDangGiu, 0) AS TonKhoKhaDung
    FROM dbo.BIENSANPHAM bt
    INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = bt.MaSanPham
    OUTER APPLY
    (
        SELECT SUM(g.SoLuong) AS SoLuongDangGiu
        FROM dbo.TONKHO_GIUCHO g
        WHERE g.MaSanPham = bt.MaSanPham
          AND g.MaBienSanPham = bt.MaBienSanPham
          AND g.TrangThai = 'Active'
          AND g.HetHanLuc > SYSDATETIME()
    ) gc;

GO
/****** Object:  Table [dbo].[VOUCHER_NGUOIDUNG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VOUCHER_NGUOIDUNG](
	[MaVoucherNguoiDung] [int] IDENTITY(1,1) NOT NULL,
	[MaVoucher] [int] NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[MaDonHang] [int] NULL,
	[MaVoucherCodeSnapshot] [nvarchar](50) NOT NULL,
	[LoaiGiamGiaSnapshot] [varchar](20) NULL,
	[GiaTriGiamSnapshot] [decimal](18, 2) NULL,
	[SoTienGiam] [decimal](18, 2) NOT NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgaySuDung] [datetime2](0) NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_VOUCHER_NGUOIDUNG] PRIMARY KEY CLUSTERED 
(
	[MaVoucherNguoiDung] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_VOUCHER_USER_USAGE]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 8. View lich su user da dung voucher */
CREATE   VIEW [dbo].[v_VOUCHER_USER_USAGE]
AS
SELECT
    MaNguoiDung,
    MaVoucher,
    COUNT(*) AS SoLanDaDungHopLe
FROM dbo.VOUCHER_NGUOIDUNG
WHERE TrangThai = 'Used'
GROUP BY MaNguoiDung, MaVoucher;

GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BAIVIET]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BAIVIET](
	[MaBaiViet] [int] IDENTITY(1,1) NOT NULL,
	[TieuDe] [nvarchar](255) NOT NULL,
	[Slug] [nvarchar](280) NOT NULL,
	[TomTat] [nvarchar](500) NULL,
	[NoiDung] [nvarchar](max) NOT NULL,
	[AnhDaiDienUrl] [nvarchar](500) NULL,
	[DanhMuc] [nvarchar](100) NULL,
	[MaTacGia] [int] NULL,
	[XuatBanLuc] [datetime2](0) NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_BAIVIET] PRIMARY KEY CLUSTERED 
(
	[MaBaiViet] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CHITIET_DONHANG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CHITIET_DONHANG](
	[MaChiTietDonHang] [int] IDENTITY(1,1) NOT NULL,
	[MaDonHang] [int] NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[MaBienSanPham] [int] NULL,
	[TenSanPhamSnapshot] [nvarchar](255) NOT NULL,
	[SKUSnapshot] [nvarchar](80) NULL,
	[DonGia] [decimal](18, 2) NOT NULL,
	[SoLuong] [int] NOT NULL,
	[ThanhTien]  AS (CONVERT([decimal](18,2),[DonGia]*[SoLuong])) PERSISTED,
 CONSTRAINT [PK_CHITIET_DONHANG] PRIMARY KEY CLUSTERED 
(
	[MaChiTietDonHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CHITIET_GIOHANG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CHITIET_GIOHANG](
	[MaChiTietGioHang] [int] IDENTITY(1,1) NOT NULL,
	[MaGioHang] [int] NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[MaBienSanPham] [int] NULL,
	[SoLuong] [int] NOT NULL,
	[DonGia] [decimal](18, 2) NOT NULL,
	[ThanhTien]  AS (CONVERT([decimal](18,2),[DonGia]*[SoLuong])) PERSISTED,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_CHITIET_GIOHANG] PRIMARY KEY CLUSTERED 
(
	[MaChiTietGioHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DANHGIASANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DANHGIASANPHAM](
	[MaDanhGia] [int] IDENTITY(1,1) NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[MaDonHang] [int] NULL,
	[Diem] [tinyint] NOT NULL,
	[TieuDe] [nvarchar](255) NULL,
	[NoiDung] [nvarchar](max) NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[HinhAnhUrl] [nvarchar](max) NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_DANHGIASANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaDanhGia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DANHMUC]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DANHMUC](
	[MaDanhMuc] [int] IDENTITY(1,1) NOT NULL,
	[MaDanhMucCha] [int] NULL,
	[TenDanhMuc] [nvarchar](150) NOT NULL,
	[Slug] [nvarchar](180) NOT NULL,
	[MoTa] [nvarchar](500) NULL,
	[ThuTuHienThi] [int] NOT NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_DANHMUC] PRIMARY KEY CLUSTERED 
(
	[MaDanhMuc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DONHANG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DONHANG](
	[MaDonHang] [int] IDENTITY(1,1) NOT NULL,
	[MaDonHangKinhDoanh] [nvarchar](50) NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[HoTenNhanHang] [nvarchar](150) NOT NULL,
	[SoDienThoaiNhanHang] [nvarchar](20) NOT NULL,
	[EmailNhanHang] [nvarchar](255) NULL,
	[DiaChiNhanHang] [nvarchar](255) NOT NULL,
	[TongTienHang] [decimal](18, 2) NOT NULL,
	[TienGiam] [decimal](18, 2) NOT NULL,
	[PhiVanChuyen] [decimal](18, 2) NOT NULL,
	[TongThanhToan] [decimal](18, 2) NOT NULL,
	[TrangThaiDonHang] [varchar](20) NOT NULL,
	[TrangThaiThanhToan] [varchar](20) NOT NULL,
	[GhiChu] [nvarchar](1000) NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
	[NgayThanhToanThanhCong] [datetime2](0) NULL,
	[NgayHuyDon] [datetime2](0) NULL,
	[LyDoHuyDon] [nvarchar](500) NULL,
	[MaGioHang] [int] NULL,
	[PhuongThucNhanHang] [varchar](30) NOT NULL,
	[TrangThaiVanChuyen] [varchar](30) NOT NULL,
	[LoaiDonHang] [varchar](20) NOT NULL,
	[TienDatCoc] [decimal](18, 2) NOT NULL,
	[SoTienConLai] [decimal](18, 2) NOT NULL,
	[NgayHenNhanXe] [datetime2](0) NULL,
	[GhiChuGiaoNhan] [nvarchar](500) NULL,
 CONSTRAINT [PK_DONHANG] PRIMARY KEY CLUSTERED 
(
	[MaDonHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DONHANG_VOUCHER]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DONHANG_VOUCHER](
	[MaDonHang] [int] NOT NULL,
	[MaVoucher] [int] NOT NULL,
	[MaVoucherCodeSnapshot] [nvarchar](50) NOT NULL,
	[SoTienGiam] [decimal](18, 2) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[LoaiGiamGiaSnapshot] [varchar](20) NULL,
	[GiaTriGiamSnapshot] [decimal](18, 2) NULL,
 CONSTRAINT [PK_DONHANG_VOUCHER] PRIMARY KEY CLUSTERED 
(
	[MaDonHang] ASC,
	[MaVoucher] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FAQ]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FAQ](
	[MaFAQ] [int] IDENTITY(1,1) NOT NULL,
	[CauHoi] [nvarchar](500) NOT NULL,
	[CauTraLoi] [nvarchar](max) NOT NULL,
	[DanhMuc] [nvarchar](100) NULL,
	[ThuTuHienThi] [int] NOT NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_FAQ] PRIMARY KEY CLUSTERED 
(
	[MaFAQ] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GIOHANG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GIOHANG](
	[MaGioHang] [int] IDENTITY(1,1) NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_GIOHANG] PRIMARY KEY CLUSTERED 
(
	[MaGioHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LIENHE_YEUCAU]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LIENHE_YEUCAU](
	[MaLienHe] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](150) NOT NULL,
	[SoDienThoai] [nvarchar](20) NOT NULL,
	[Email] [nvarchar](255) NULL,
	[TieuDe] [nvarchar](255) NULL,
	[NoiDung] [nvarchar](max) NOT NULL,
	[LoaiYeuCau] [varchar](30) NOT NULL,
	[MaSanPham] [int] NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[DaXuLyLuc] [datetime2](0) NULL,
	[MaNguoiXuLy] [int] NULL,
 CONSTRAINT [PK_LIENHE_YEUCAU] PRIMARY KEY CLUSTERED 
(
	[MaLienHe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NGUOIDUNG]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NGUOIDUNG](
	[MaNguoiDung] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](150) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[SoDienThoai] [nvarchar](20) NOT NULL,
	[MatKhauHash] [nvarchar](500) NOT NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_NGUOIDUNG] PRIMARY KEY CLUSTERED 
(
	[MaNguoiDung] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NGUOIDUNG_DIACHI]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NGUOIDUNG_DIACHI](
	[MaDiaChi] [int] IDENTITY(1,1) NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[HoTenNhanHang] [nvarchar](150) NOT NULL,
	[SoDienThoaiNhanHang] [nvarchar](20) NOT NULL,
	[DiaChiNhanHang] [nvarchar](255) NOT NULL,
	[PhuongXa] [nvarchar](100) NULL,
	[QuanHuyen] [nvarchar](100) NULL,
	[TinhThanh] [nvarchar](100) NOT NULL,
	[GhiChu] [nvarchar](255) NULL,
	[LaMacDinh] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MaDiaChi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NGUOIDUNG_VAITRO]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NGUOIDUNG_VAITRO](
	[MaNguoiDung] [int] NOT NULL,
	[MaVaiTro] [tinyint] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_NGUOIDUNG_VAITRO] PRIMARY KEY CLUSTERED 
(
	[MaNguoiDung] ASC,
	[MaVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[THANHTOAN]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[THANHTOAN](
	[MaThanhToan] [int] IDENTITY(1,1) NOT NULL,
	[MaThanhToanKinhDoanh] [nvarchar](50) NOT NULL,
	[MaDonHang] [int] NOT NULL,
	[SoTien] [decimal](18, 2) NOT NULL,
	[PhuongThuc] [varchar](30) NOT NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[MaGiaoDich] [nvarchar](120) NULL,
	[DaThanhToanLuc] [datetime2](0) NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[LoaiThanhToan] [varchar](30) NOT NULL,
	[NoiDungChuyenKhoan] [nvarchar](500) NULL,
	[MaNganHang] [nvarchar](50) NULL,
	[LyDoHuy] [nvarchar](500) NULL,
	[NgayHuy] [datetime2](0) NULL,
	[ResponseRaw] [nvarchar](max) NULL,
 CONSTRAINT [PK_THANHTOAN] PRIMARY KEY CLUSTERED 
(
	[MaThanhToan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VAITRO]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VAITRO](
	[MaVaiTro] [tinyint] IDENTITY(1,1) NOT NULL,
	[TenVaiTro] [varchar](30) NOT NULL,
	[MoTa] [nvarchar](255) NULL,
 CONSTRAINT [PK_VAITRO] PRIMARY KEY CLUSTERED 
(
	[MaVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VOUCHER](
	[MaVoucher] [int] IDENTITY(1,1) NOT NULL,
	[MaVoucherCode] [nvarchar](50) NOT NULL,
	[LoaiGiamGia] [varchar](20) NOT NULL,
	[GiaTriGiam] [decimal](18, 2) NOT NULL,
	[GiaTriDonToiThieu] [decimal](18, 2) NOT NULL,
	[GiaTriGiamToiDa] [decimal](18, 2) NULL,
	[NgayBatDau] [datetime2](0) NOT NULL,
	[NgayKetThuc] [datetime2](0) NOT NULL,
	[GioiHanSuDung] [int] NULL,
	[SoLanDaDung] [int] NOT NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[MoTa] [nvarchar](500) NULL,
	[SoLanToiDaMoiNguoiDung] [int] NOT NULL,
	[PhamViApDung] [varchar](20) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
	[ApDungLoaiDonHang] [varchar](50) NULL,
 CONSTRAINT [PK_VOUCHER] PRIMARY KEY CLUSTERED 
(
	[MaVoucher] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER_DANHMUC]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VOUCHER_DANHMUC](
	[MaVoucher] [int] NOT NULL,
	[MaDanhMuc] [int] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_VOUCHER_DANHMUC] PRIMARY KEY CLUSTERED 
(
	[MaVoucher] ASC,
	[MaDanhMuc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER_HANGXE]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VOUCHER_HANGXE](
	[MaVoucher] [int] NOT NULL,
	[MaHangXe] [int] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_VOUCHER_HANGXE] PRIMARY KEY CLUSTERED 
(
	[MaVoucher] ASC,
	[MaHangXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER_SANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VOUCHER_SANPHAM](
	[MaVoucher] [int] NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_VOUCHER_SANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaVoucher] ASC,
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[YEUTHICH]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[YEUTHICH](
	[MaNguoiDung] [int] NOT NULL,
	[MaSanPham] [int] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_YEUTHICH] PRIMARY KEY CLUSTERED 
(
	[MaNguoiDung] ASC,
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260518085245_AddPaymentTables', N'8.0.22')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260518090449_RemovePaymentRefundTable', N'8.0.22')
GO
SET IDENTITY_INSERT [dbo].[ANHSANPHAM] ON 

INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (1, 101, N'https://product.hstatic.net/200000560101/product/20220524-air-blade-160-chi-tiet-xe-tieu-chuan-xanh-xam_dab7adeec6b34187967667d6199a6de5.png', N'Honda Air Blade 160 Xanh Đen', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1001)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (2, 101, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT-6eag1uRVdUxAQvGWzYyt8AR1UKeXJ9LyNw&s', N'Honda Air Blade 160 Đỏ Đen', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1002)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (3, 101, N'https://product.hstatic.net/200000387527/product/160_bac_xanh_den_3cf4282ef40f4934be7f0d7b378ee325_large.jpg', N'Honda Air Blade 160 Đen Bạc', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1003)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (4, 101, N'https://img.tinxe.vn/crop/730x410/2020/12/04/XForF7yt/tc-125-honda-air-blade-4-b9b0.png', N'Honda Air Blade 160 Trắng Đen', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1004)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (5, 101, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQL2H2VYNPD_9gZglpc9SOxv4ahPgpPx9qiTg&s', N'Honda Air Blade 160 Xanh Ghi', 0, 5, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1005)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (6, 101, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQX6RO5vXdLVF9lyiOwX__0cPT2jpFhKX9-1Q&s', N'Honda Air Blade 160 Đen Vàng', 0, 6, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1006)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (7, 102, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRk1bqcmPMG13Xu2wetmJVwsaPYC95k0y5OAQ&s', N'Honda Air Blade 125 Xanh Đen', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1007)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (8, 102, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR4zcDp7yJmnedGzkvMN-DjZyPMXZXQ70cJ0A&s', N'Honda Air Blade 125 Đỏ Đen', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1008)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (9, 102, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQS2nNwdhnqPY-cAwcZ9QkwrEtLXCmz26aFwg&s', N'Honda Air Blade 125 Đen Nhám', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1009)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (10, 102, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTh_5vvBj_Ns1KFuseFzkGj7K-B9k837l3Sig&s', N'Honda Air Blade 125 Xám Đen', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1010)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (11, 103, N'https://cdn.showroom.vn/images/honda-lead-trang.jpg', N'Honda LEAD Trắng', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1011)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (12, 103, N'https://cdn.showroom.vn/images/honda-lead-do.jpg', N'Honda LEAD Đỏ', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1012)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (13, 103, N'https://cdn.showroom.vn/images/honda-lead-xanh-dam.jpg', N'Honda LEAD Xanh Đậm', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1013)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (14, 103, N'https://cdn.showroom.vn/images/honda-lead-bac.jpg', N'Honda LEAD Bạc', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1014)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (15, 103, N'https://cdn.showroom.vn/images/honda-lead-den-nham.jpg', N'Honda LEAD Đen Nhám', 0, 5, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1015)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (16, 104, N'https://cdn.showroom.vn/images/wave-alpha-trang.jpg', N'Honda Wave Alpha Trắng', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1016)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (17, 104, N'https://cdn.showroom.vn/images/wave-alpha-den-bac.jpg', N'Honda Wave Alpha Đen Bạc', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1017)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (18, 104, N'https://cdn.showroom.vn/images/wave-alpha-xanh-dam.jpg', N'Honda Wave Alpha Xanh Đậm', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1018)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (19, 104, N'https://cdn.showroom.vn/images/wave-alpha-den-nham.jpg', N'Honda Wave Alpha Đen Nhám', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1019)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (20, 105, N'https://cdn.showroom.vn/images/exciter-155-den-bong.jpg', N'Yamaha Exciter 155 Đen Bóng', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1020)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (21, 105, N'https://cdn.showroom.vn/images/exciter-155-do-den.jpg', N'Yamaha Exciter 155 Đỏ Đen', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1021)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (22, 105, N'https://cdn.showroom.vn/images/exciter-155-xanh-gp.jpg', N'Yamaha Exciter 155 Xanh GP', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1022)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (23, 105, N'https://cdn.showroom.vn/images/exciter-155-vang-den.jpg', N'Yamaha Exciter 155 Vàng Đen', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1023)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (24, 106, N'https://cdn.showroom.vn/images/freego-s-trang.jpg', N'Yamaha FreeGo S Trắng', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1024)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (25, 106, N'https://cdn.showroom.vn/images/freego-s-xanh-den.jpg', N'Yamaha FreeGo S Xanh Đen', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1025)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (26, 106, N'https://cdn.showroom.vn/images/freego-s-do-den.jpg', N'Yamaha FreeGo S Đỏ Đen', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1026)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (27, 106, N'https://cdn.showroom.vn/images/freego-s-xam-den.jpg', N'Yamaha FreeGo S Xám Đen', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1027)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (28, 108, N'https://cdn.showroom.vn/images/evo200-trang-ngoc-trai.jpg', N'VinFast Evo200 Trắng Ngọc Trai', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1028)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (29, 108, N'https://cdn.showroom.vn/images/evo200-do-tuoi.jpg', N'VinFast Evo200 Đỏ Tươi', 0, 2, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1029)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (30, 108, N'https://cdn.showroom.vn/images/evo200-xanh-tim.jpg', N'VinFast Evo200 Xanh Tím', 0, 3, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1030)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (31, 108, N'https://cdn.showroom.vn/images/evo200-vang-cam.jpg', N'VinFast Evo200 Vàng Cam', 0, 4, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1031)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (32, 112, N'https://cdn.showroom.vn/images/michelin-pilot-street.jpg', N'Lốp Michelin Pilot Street 2', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1032)
INSERT [dbo].[ANHSANPHAM] ([MaAnhSanPham], [MaSanPham], [UrlAnh], [AltText], [LaAnhChinh], [ThuTuHienThi], [NgayTao], [MaBienSanPham]) VALUES (33, 113, N'https://cdn.showroom.vn/images/motul-300v-xanh-la.jpg', N'Dầu nhớt Motul 300V', 1, 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), 1034)
SET IDENTITY_INSERT [dbo].[ANHSANPHAM] OFF
GO
SET IDENTITY_INSERT [dbo].[BAIVIET] ON 

INSERT [dbo].[BAIVIET] ([MaBaiViet], [TieuDe], [Slug], [TomTat], [NoiDung], [AnhDaiDienUrl], [DanhMuc], [MaTacGia], [XuatBanLuc], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (1, N'Kinh nghiệm chọn xe tay ga đô thị', N'kinh-nghiem-chon-xe-tay-ga-do-thi', N'Các tiêu chí nên xem trước khi chọn xe tay ga: cốp, phanh, chiều cao yên, mức tiêu hao nhiên liệu.', N'Khi chọn xe tay ga đô thị, khách hàng nên ưu tiên tư thế ngồi, độ cao yên, dung tích cốp, phanh ABS, mức tiêu hao nhiên liệu và chi phí bảo dưỡng. Nhu cầu đi phố nhẹ nhàng phù hợp nhóm 110-125cc; nhu cầu vận hành mạnh hơn phù hợp nhóm 155-160cc.', N'https://cdn.honda.com.vn/motorbike-strong-points/November2025/hG09n8OzFHezbDWnOvZS.png', N'Tư vấn mua xe', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), N'Published', CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[BAIVIET] OFF
GO
SET IDENTITY_INSERT [dbo].[BIENSANPHAM] ON 

INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1001, 101, N'Air Blade 160 Tiêu Chuẩn - Xanh Đen', N'AB160-TC-XANHDEN', CAST(56690000.00 AS Decimal(18, 2)), 15, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1002, 101, N'Air Blade 160 Tiêu Chuẩn - Đỏ Đen', N'AB160-TC-DODEN', CAST(56690000.00 AS Decimal(18, 2)), 10, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1003, 101, N'Air Blade 160 Tiêu Chuẩn - Đen Bạc', N'AB160-TC-DENBAC', CAST(56690000.00 AS Decimal(18, 2)), 20, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Bạc')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1004, 101, N'Air Blade 160 Tiêu Chuẩn - Trắng Đen', N'AB160-TC-TRANGDEN', CAST(56690000.00 AS Decimal(18, 2)), 8, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Trắng Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1005, 101, N'Air Blade 160 Đặc Biệt - Xanh Ghi', N'AB160-DB-XANHGHI', CAST(57890000.00 AS Decimal(18, 2)), 5, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Ghi')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1006, 101, N'Air Blade 160 Đặc Biệt - Đen Vàng', N'AB160-DB-DENVANG', CAST(57890000.00 AS Decimal(18, 2)), 12, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Vàng')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1007, 102, N'Air Blade 125 Tiêu Chuẩn - Xanh Đen', N'AB125-TC-XANHDEN', CAST(42090000.00 AS Decimal(18, 2)), 25, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1008, 102, N'Air Blade 125 Tiêu Chuẩn - Đỏ Đen', N'AB125-TC-DODEN', CAST(42090000.00 AS Decimal(18, 2)), 18, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1009, 102, N'Air Blade 125 Đặc Biệt - Đen Nhám', N'AB125-DB-DENNHAM', CAST(43290000.00 AS Decimal(18, 2)), 30, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Nhám')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1010, 102, N'Air Blade 125 Thể Thao - Xám Đen', N'AB125-TT-XAMDEN', CAST(43790000.00 AS Decimal(18, 2)), 15, N'Available', N'Thể thao', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xám Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1011, 103, N'LEAD Tiêu Chuẩn - Trắng', N'LEAD-TC-TRANG', CAST(39590000.00 AS Decimal(18, 2)), 20, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Trắng')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1012, 103, N'LEAD Tiêu Chuẩn - Đỏ', N'LEAD-TC-DO', CAST(39590000.00 AS Decimal(18, 2)), 15, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1013, 103, N'LEAD Cao Cấp - Xanh Đậm', N'LEAD-CC-XANHDAM', CAST(41790000.00 AS Decimal(18, 2)), 10, N'Available', N'Cao cấp', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Đậm')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1014, 103, N'LEAD Cao Cấp - Bạc', N'LEAD-CC-BAC', CAST(41790000.00 AS Decimal(18, 2)), 8, N'Available', N'Cao cấp', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Bạc')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1015, 103, N'LEAD Đặc Biệt - Đen Nhám', N'LEAD-DB-DENNHAM', CAST(42790000.00 AS Decimal(18, 2)), 12, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Nhám')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1016, 104, N'Wave Alpha Tiêu Chuẩn - Trắng', N'WAVE-TC-TRANG', CAST(18190000.00 AS Decimal(18, 2)), 40, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Trắng')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1017, 104, N'Wave Alpha Tiêu Chuẩn - Đen Bạc', N'WAVE-TC-DENBAC', CAST(18190000.00 AS Decimal(18, 2)), 35, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Bạc')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1018, 104, N'Wave Alpha Tiêu Chuẩn - Xanh Đậm', N'WAVE-TC-XANHDAM', CAST(18190000.00 AS Decimal(18, 2)), 25, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Đậm')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1019, 104, N'Wave Alpha Đặc Biệt - Đen Nhám', N'WAVE-DB-DENNHAM', CAST(18790000.00 AS Decimal(18, 2)), 50, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Nhám')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1020, 105, N'Exciter 155 Tiêu Chuẩn - Đen Bóng', N'EX155-TC-DEN', CAST(48000000.00 AS Decimal(18, 2)), 10, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen Bóng')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1021, 105, N'Exciter 155 Tiêu Chuẩn - Đỏ Đen', N'EX155-TC-DODEN', CAST(48000000.00 AS Decimal(18, 2)), 12, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1022, 105, N'Exciter 155 Cao Cấp - Xanh GP', N'EX155-CC-XANHGP', CAST(51000000.00 AS Decimal(18, 2)), 15, N'Available', N'Cao cấp', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh GP')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1023, 105, N'Exciter 155 Giới Hạn - Vàng Đen', N'EX155-GH-VANGDEN', CAST(52000000.00 AS Decimal(18, 2)), 5, N'Available', N'Giới hạn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Vàng Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1024, 106, N'FreeGo Tiêu Chuẩn - Trắng', N'FREEGO-TC-TRANG', CAST(30100000.00 AS Decimal(18, 2)), 10, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Trắng')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1025, 106, N'FreeGo Đặc Biệt - Xanh Đen', N'FREEGO-DB-XANHDEN', CAST(34000000.00 AS Decimal(18, 2)), 12, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1026, 106, N'FreeGo Đặc Biệt - Đỏ Đen', N'FREEGO-DB-DODEN', CAST(34000000.00 AS Decimal(18, 2)), 10, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1027, 106, N'FreeGo Đặc Biệt - Xám Đen', N'FREEGO-DB-XAMDEN', CAST(34000000.00 AS Decimal(18, 2)), 8, N'Available', N'Đặc biệt', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xám Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1028, 108, N'Evo200 - Trắng Ngọc Trai', N'EVO-TRANG', CAST(22000000.00 AS Decimal(18, 2)), 30, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Trắng Ngọc Trai')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1029, 108, N'Evo200 - Đỏ Tươi', N'EVO-DO', CAST(22000000.00 AS Decimal(18, 2)), 20, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đỏ Tươi')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1030, 108, N'Evo200 - Xanh Tím', N'EVO-XANHTIM', CAST(22000000.00 AS Decimal(18, 2)), 15, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Tím')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1031, 108, N'Evo200 - Vàng Cam', N'EVO-VANGCAM', CAST(22000000.00 AS Decimal(18, 2)), 10, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Vàng Cam')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1032, 112, N'Lốp Michelin 90/80-17', N'MICH-908017', CAST(1200000.00 AS Decimal(18, 2)), 100, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1033, 112, N'Lốp Michelin 120/70-17', N'MICH-1207017', CAST(1400000.00 AS Decimal(18, 2)), 80, N'Available', N'Tiêu chuẩn', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Đen')
INSERT [dbo].[BIENSANPHAM] ([MaBienSanPham], [MaSanPham], [TenBienThe], [SKU], [GiaGhiDe], [SoLuongTon], [TrangThai], [PhienBan], [NgayTao], [NgayCapNhat], [MauSac]) VALUES (1034, 113, N'Nhớt Motul 300V 1L', N'MOTUL-300V-1L', CAST(450000.00 AS Decimal(18, 2)), 200, N'Available', N'1 Lít', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'Xanh Lá')
SET IDENTITY_INSERT [dbo].[BIENSANPHAM] OFF
GO
SET IDENTITY_INSERT [dbo].[CHITIET_DONHANG] ON 

INSERT [dbo].[CHITIET_DONHANG] ([MaChiTietDonHang], [MaDonHang], [MaSanPham], [MaBienSanPham], [TenSanPhamSnapshot], [SKUSnapshot], [DonGia], [SoLuong]) VALUES (27, 69, 101, 1001, N'Honda Air Blade 160', N'AB160-TC-XANHDEN', CAST(56690000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[CHITIET_DONHANG] ([MaChiTietDonHang], [MaDonHang], [MaSanPham], [MaBienSanPham], [TenSanPhamSnapshot], [SKUSnapshot], [DonGia], [SoLuong]) VALUES (28, 70, 101, 1001, N'Honda Air Blade 160', N'AB160-TC-XANHDEN', CAST(56690000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[CHITIET_DONHANG] ([MaChiTietDonHang], [MaDonHang], [MaSanPham], [MaBienSanPham], [TenSanPhamSnapshot], [SKUSnapshot], [DonGia], [SoLuong]) VALUES (29, 71, 103, 1013, N'Honda LEAD ABS', N'LEAD-CC-XANHDAM', CAST(41790000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[CHITIET_DONHANG] ([MaChiTietDonHang], [MaDonHang], [MaSanPham], [MaBienSanPham], [TenSanPhamSnapshot], [SKUSnapshot], [DonGia], [SoLuong]) VALUES (30, 71, 103, 1011, N'Honda LEAD ABS', N'LEAD-TC-TRANG', CAST(39590000.00 AS Decimal(18, 2)), 1)
SET IDENTITY_INSERT [dbo].[CHITIET_DONHANG] OFF
GO
SET IDENTITY_INSERT [dbo].[CHITIET_GIOHANG] ON 

INSERT [dbo].[CHITIET_GIOHANG] ([MaChiTietGioHang], [MaGioHang], [MaSanPham], [MaBienSanPham], [SoLuong], [DonGia], [NgayTao], [NgayCapNhat]) VALUES (23, 25, 101, 1001, 2, CAST(56690000.00 AS Decimal(18, 2)), CAST(N'2026-05-11T08:25:11.0000000' AS DateTime2), CAST(N'2026-05-14T08:31:31.0000000' AS DateTime2))
INSERT [dbo].[CHITIET_GIOHANG] ([MaChiTietGioHang], [MaGioHang], [MaSanPham], [MaBienSanPham], [SoLuong], [DonGia], [NgayTao], [NgayCapNhat]) VALUES (24, 26, 101, 1001, 1, CAST(56690000.00 AS Decimal(18, 2)), CAST(N'2026-05-14T10:10:28.0000000' AS DateTime2), CAST(N'2026-05-14T10:28:56.0000000' AS DateTime2))
INSERT [dbo].[CHITIET_GIOHANG] ([MaChiTietGioHang], [MaGioHang], [MaSanPham], [MaBienSanPham], [SoLuong], [DonGia], [NgayTao], [NgayCapNhat]) VALUES (25, 27, 103, 1013, 1, CAST(41790000.00 AS Decimal(18, 2)), CAST(N'2026-05-15T08:46:56.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2))
INSERT [dbo].[CHITIET_GIOHANG] ([MaChiTietGioHang], [MaGioHang], [MaSanPham], [MaBienSanPham], [SoLuong], [DonGia], [NgayTao], [NgayCapNhat]) VALUES (26, 27, 103, 1011, 1, CAST(39590000.00 AS Decimal(18, 2)), CAST(N'2026-05-15T08:50:13.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[CHITIET_GIOHANG] OFF
GO
SET IDENTITY_INSERT [dbo].[DANHGIASANPHAM] ON 

INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (1, 101, 4, NULL, 5, N'Xe chạy cực bốc', N'Động cơ 160cc chạy rất bốc, màu xanh đen nhám nhìn rất sang trọng. Tuyệt vời!', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (2, 101, 5, NULL, 4, N'Thiết kế đẹp', N'Xe đẹp nhưng phuộc hơi cứng khi đi đường xóc.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (3, 102, 6, NULL, 5, N'Tiết kiệm xăng', N'Bản 125 chạy êm ái, màu xám đen rất sạch sẽ và hợp nhãn.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (4, 103, 7, NULL, 5, N'Cốp xe quá đã', N'LEAD muôn năm với cái cốp khổng lồ, bản màu trắng nhìn nữ tính và đẹp.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (5, 103, 8, NULL, 4, N'Tốt trong tầm giá', N'Xe nhẹ dễ dắt, phanh ABS an toàn. Đáng tiền.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (6, 104, 9, NULL, 5, N'Bền bỉ, rẻ', N'Màu xanh đậm rất đẹp. Wave Alpha luôn là lựa chọn quốc dân.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (7, 105, 10, NULL, 5, N'Vua côn tay', N'Exciter 155 VVA kéo ga vọt, thiết kế đuôi xe vuốt nhọn thể thao. Rất ưng màu xanh GP.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (8, 105, 4, NULL, 4, N'Xe mạnh nhưng yên cứng', N'Vận hành ngon nhưng yên zin đi xa hơi ê mông.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (9, 106, 5, NULL, 5, N'Ngon bổ rẻ', N'Xe ga có ABS mà giá quá tốt, màu đỏ đen cực kỳ nổi bật.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (10, 108, 6, NULL, 5, N'VinFast đi mượt', N'Xe điện Evo200 đi cực êm, màu vàng tươi sành điệu, rất phù hợp đi chợ.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (11, 110, 7, NULL, 5, N'Công nghệ ngập tràn', N'Vento S chạy bốc không kém xe xăng, công nghệ thông minh. Màu cam đỉnh chóp.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (12, 112, 8, NULL, 5, N'Lốp bám đường', N'Thay lốp Michelin xong vào cua tự tin hẳn.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (13, 113, 9, NULL, 5, N'Nhớt xịn', N'Motul 300V thì không có gì để chê, mát máy và bốc xe.', N'Approved', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (14, 101, 10, NULL, 5, N'Màu đỏ đen cá tính', N'Mới múc em AB đỏ đen, xe khỏe, đèn LED sáng rõ.', N'Pending', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
INSERT [dbo].[DANHGIASANPHAM] ([MaDanhGia], [MaSanPham], [MaNguoiDung], [MaDonHang], [Diem], [TieuDe], [NoiDung], [TrangThai], [NgayTao], [HinhAnhUrl], [NgayCapNhat]) VALUES (15, 109, 4, NULL, 4, N'Dáng đẹp như xe Ý', N'Klara S đi chơi rất hợp, cốp vừa phải, nước sơn bóng bẩy.', N'Pending', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), NULL, CAST(N'2026-05-18T15:52:09.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[DANHGIASANPHAM] OFF
GO
SET IDENTITY_INSERT [dbo].[DANHMUC] ON 

INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (2, 12, N'Xe tay ga', N'xe-tay-ga', N'Xe tay ga phổ thông và cao cấp.', 2, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (3, 12, N'Xe số', N'xe-so', N'Xe số bền bỉ, tiết kiệm nhiên liệu.', 3, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (4, 12, N'Xe côn tay', N'xe-con-tay', N'Xe côn tay thể thao.', 4, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (5, 12, N'Xe máy điện', N'xe-dien', N'Xe máy điện đô thị.', 5, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (7, 15, N'Dầu nhớt', N'dau-nhot', N'Dầu nhớt và dung dịch bảo dưỡng.', 11, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (8, 15, N'Lốp xe', N'lop-xe', N'Lốp xe máy, ruột xe, van lốp.', 12, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (9, 15, N'Phanh xe', N'phanh-xe', N'Má phanh, bố thắng, dầu phanh.', 13, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (10, 15, N'Lọc gió', N'loc-gio', N'Lọc gió động cơ.', 14, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (11, 15, N'Phụ kiện', N'phu-kien', N'Mũ bảo hiểm, gương, phụ kiện đi xe.', 15, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (12, NULL, N'Xe máy', N'xe-may', NULL, 1, 1, CAST(N'2026-05-03T16:37:49.0000000' AS DateTime2), CAST(N'2026-05-03T16:37:49.0000000' AS DateTime2))
INSERT [dbo].[DANHMUC] ([MaDanhMuc], [MaDanhMucCha], [TenDanhMuc], [Slug], [MoTa], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (15, NULL, N'Phụ tùng', N'phu-tung', NULL, 2, 1, CAST(N'2026-05-03T16:39:28.0000000' AS DateTime2), CAST(N'2026-05-03T16:39:28.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[DANHMUC] OFF
GO
SET IDENTITY_INSERT [dbo].[DONGXE] ON 

INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (1, 1, N'Air Blade 160/125', N'air-blade-160-125', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (2, 1, N'LEAD ABS', N'lead-abs', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (3, 1, N'Wave Alpha 110', N'wave-alpha-110', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (4, 2, N'Exciter 155 VVA', N'exciter-155', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (5, 2, N'FreeGo', N'freego', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (6, 2, N'Latte', N'latte', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (7, 3, N'Evo200', N'evo200', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (8, 3, N'Klara S', N'klara-s', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (9, 3, N'Vento S', N'vento-s', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[DONGXE] ([MaDongXe], [MaHangXe], [TenDongXe], [Slug], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (10, 3, N'Theon S', N'theon-s', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[DONGXE] OFF
GO
SET IDENTITY_INSERT [dbo].[DONHANG] ON 

INSERT [dbo].[DONHANG] ([MaDonHang], [MaDonHangKinhDoanh], [MaNguoiDung], [HoTenNhanHang], [SoDienThoaiNhanHang], [EmailNhanHang], [DiaChiNhanHang], [TongTienHang], [TienGiam], [PhiVanChuyen], [TongThanhToan], [TrangThaiDonHang], [TrangThaiThanhToan], [GhiChu], [NgayTao], [NgayCapNhat], [NgayThanhToanThanhCong], [NgayHuyDon], [LyDoHuyDon], [MaGioHang], [PhuongThucNhanHang], [TrangThaiVanChuyen], [LoaiDonHang], [TienDatCoc], [SoTienConLai], [NgayHenNhanXe], [GhiChuGiaoNhan]) VALUES (69, N'ORD202605140831305b9bb0f', 15, N'phạm tiến dũng', N'0392757286', N'phamtiendung2k5hc@gmail.com', N'236 Hoàng Quốc Việt, phường Nghĩa Đô, TP Hà Nội, Hoàng Quốc Việt, Hà Nội', CAST(113380000.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(113230000.00 AS Decimal(18, 2)), N'Cancelled', N'Unpaid', NULL, CAST(N'2026-05-14T08:31:31.0000000' AS DateTime2), CAST(N'2026-05-14T08:32:20.0000000' AS DateTime2), NULL, CAST(N'2026-05-14T08:32:20.0000000' AS DateTime2), N'Khach hang huy don', 25, N'Delivery', N'Cancelled', N'FullPayment', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[DONHANG] ([MaDonHang], [MaDonHangKinhDoanh], [MaNguoiDung], [HoTenNhanHang], [SoDienThoaiNhanHang], [EmailNhanHang], [DiaChiNhanHang], [TongTienHang], [TienGiam], [PhiVanChuyen], [TongThanhToan], [TrangThaiDonHang], [TrangThaiThanhToan], [GhiChu], [NgayTao], [NgayCapNhat], [NgayThanhToanThanhCong], [NgayHuyDon], [LyDoHuyDon], [MaGioHang], [PhuongThucNhanHang], [TrangThaiVanChuyen], [LoaiDonHang], [TienDatCoc], [SoTienConLai], [NgayHenNhanXe], [GhiChuGiaoNhan]) VALUES (70, N'ORD202605141028561f95951', 15, N'phạm tiến dũng', N'0392757286', N'phamtiendung2k5hc@gmail.com', N'236 Hoàng Quốc Việt, phường Nghĩa Đô, TP Hà Nội, Hoàng Quốc Việt, Hà Nội', CAST(56690000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(56690000.00 AS Decimal(18, 2)), N'Cancelled', N'Unpaid', N'fucking bit', CAST(N'2026-05-14T10:28:56.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), NULL, CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), N'Het thoi gian thanh toan', 26, N'Delivery', N'NotShipped', N'FullPayment', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[DONHANG] ([MaDonHang], [MaDonHangKinhDoanh], [MaNguoiDung], [HoTenNhanHang], [SoDienThoaiNhanHang], [EmailNhanHang], [DiaChiNhanHang], [TongTienHang], [TienGiam], [PhiVanChuyen], [TongThanhToan], [TrangThaiDonHang], [TrangThaiThanhToan], [GhiChu], [NgayTao], [NgayCapNhat], [NgayThanhToanThanhCong], [NgayHuyDon], [LyDoHuyDon], [MaGioHang], [PhuongThucNhanHang], [TrangThaiVanChuyen], [LoaiDonHang], [TienDatCoc], [SoTienConLai], [NgayHenNhanXe], [GhiChuGiaoNhan]) VALUES (71, N'ORD20260519092222270278b', 15, N'phạm tiến dũng', N'0392757286', N'phamtiendung2k5hc@gmail.com', N'236 Hoàng Quốc Việt, phường Nghĩa Đô, TP Hà Nội, Hoàng Quốc Việt, Hà Nội', CAST(81380000.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(81330000.00 AS Decimal(18, 2)), N'AwaitingPayment', N'Unpaid', NULL, CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), NULL, NULL, NULL, 27, N'Delivery', N'NotShipped', N'FullPayment', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
SET IDENTITY_INSERT [dbo].[DONHANG] OFF
GO
INSERT [dbo].[DONHANG_VOUCHER] ([MaDonHang], [MaVoucher], [MaVoucherCodeSnapshot], [SoTienGiam], [NgayTao], [LoaiGiamGiaSnapshot], [GiaTriGiamSnapshot]) VALUES (69, 19, N'SALE5', CAST(150000.00 AS Decimal(18, 2)), CAST(N'2026-05-14T15:31:31.0000000' AS DateTime2), N'Percent', CAST(5.00 AS Decimal(18, 2)))
INSERT [dbo].[DONHANG_VOUCHER] ([MaDonHang], [MaVoucher], [MaVoucherCodeSnapshot], [SoTienGiam], [NgayTao], [LoaiGiamGiaSnapshot], [GiaTriGiamSnapshot]) VALUES (71, 14, N'NHOTXE10', CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-05-19T16:22:23.0000000' AS DateTime2), N'Percent', CAST(10.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[FAQ] ON 

INSERT [dbo].[FAQ] ([MaFAQ], [CauHoi], [CauTraLoi], [DanhMuc], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (1, N'Làm sao biết màu xe còn hàng?', N'Trang chi tiết sản phẩm hiển thị tồn kho theo từng biến thể màu. Khách hàng nên liên hệ showroom để xác nhận trước khi đến xem xe.', N'Sản phẩm', 1, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[FAQ] ([MaFAQ], [CauHoi], [CauTraLoi], [DanhMuc], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (2, N'Giá xe đã bao gồm đăng ký biển số chưa?', N'Giá niêm yết là giá xe tại showroom, chưa bao gồm lệ phí trước bạ, biển số và bảo hiểm nếu không ghi rõ.', N'Thanh toán', 2, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[FAQ] ([MaFAQ], [CauHoi], [CauTraLoi], [DanhMuc], [ThuTuHienThi], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (3, N'Có thể đặt màu chưa có sẵn không?', N'Có. Showroom sẽ ghi nhận nhu cầu và báo thời gian nhập hàng dự kiến theo từng mẫu xe.', N'Sản phẩm', 3, 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[FAQ] OFF
GO
SET IDENTITY_INSERT [dbo].[GIOHANG] ON 

INSERT [dbo].[GIOHANG] ([MaGioHang], [MaNguoiDung], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (11, 13, N'Active', CAST(N'2026-05-05T09:08:24.0000000' AS DateTime2), CAST(N'2026-05-05T09:08:24.0000000' AS DateTime2))
INSERT [dbo].[GIOHANG] ([MaGioHang], [MaNguoiDung], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (25, 15, N'CheckedOut', CAST(N'2026-05-11T08:25:11.0000000' AS DateTime2), CAST(N'2026-05-14T08:31:31.0000000' AS DateTime2))
INSERT [dbo].[GIOHANG] ([MaGioHang], [MaNguoiDung], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (26, 15, N'CheckedOut', CAST(N'2026-05-14T10:10:28.0000000' AS DateTime2), CAST(N'2026-05-14T10:28:56.0000000' AS DateTime2))
INSERT [dbo].[GIOHANG] ([MaGioHang], [MaNguoiDung], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (27, 15, N'CheckedOut', CAST(N'2026-05-15T08:46:56.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[GIOHANG] OFF
GO
SET IDENTITY_INSERT [dbo].[HANGXE] ON 

INSERT [dbo].[HANGXE] ([MaHangXe], [TenHang], [Slug], [LogoUrl], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (1, N'Honda', N'honda', N'https://www.honda.com.vn/images/logo.svg', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[HANGXE] ([MaHangXe], [TenHang], [Slug], [LogoUrl], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (2, N'Yamaha', N'yamaha', N'https://yamaha-motor.com.vn/wp/wp-content/themes/yamaha/assets/img/share/logo.png', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[HANGXE] ([MaHangXe], [TenHang], [Slug], [LogoUrl], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (3, N'VinFast', N'vinfast', N'https://storage.googleapis.com/vinfast-data-01/VinFast-logo.png', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[HANGXE] ([MaHangXe], [TenHang], [Slug], [LogoUrl], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (4, N'Michelin', N'michelin', N'https://www.michelin.com.vn/themes/custom/michelin_theme/images/michelin-logo.svg', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
INSERT [dbo].[HANGXE] ([MaHangXe], [TenHang], [Slug], [LogoUrl], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (5, N'Motul', N'motul', N'https://www.motul.com/assets/logo_motul-0a705b8a63d311b1f0a8c540f95f15e7.png', 1, CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2), CAST(N'2026-04-25T14:13:37.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[HANGXE] OFF
GO
SET IDENTITY_INSERT [dbo].[NGUOIDUNG] ON 

INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (1, N'Quản trị viên hệ thống', N'admin@showroom.local', N'0901000001', N'DEMO_HASH_Password@123_Admin', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (2, N'Nguyễn Minh Quân', N'quan.staff@showroom.local', N'0901000002', N'DEMO_HASH_Password@123_Staff01', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (3, N'Trần Hoài An', N'an.staff@showroom.local', N'0901000003', N'DEMO_HASH_Password@123_Staff02', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (4, N'Lê Văn Hưng', N'hung.le@example.com', N'0912000001', N'DEMO_HASH_Password@123_Customer01', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (5, N'Phạm Thu Hà', N'ha.pham@example.com', N'0912000002', N'DEMO_HASH_Password@123_Customer02', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (6, N'Nguyễn Đức Long', N'long.nguyen@example.com', N'0912000003', N'DEMO_HASH_Password@123_Customer03', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (7, N'Trần Bảo Ngọc', N'ngoc.tran@example.com', N'0912000004', N'DEMO_HASH_Password@123_Customer04', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (8, N'Hoàng Anh Tú', N'tu.hoang@example.com', N'0912000005', N'DEMO_HASH_Password@123_Customer05', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (9, N'Đặng Minh Châu', N'chau.dang@example.com', N'0912000006', N'DEMO_HASH_Password@123_Customer06', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (10, N'Bùi Khánh Linh', N'linh.bui@example.com', N'0912000007', N'DEMO_HASH_Password@123_Customer07', N'Active', CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2), CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (12, N'Auto Showroom Admin', N'admin@autoshowroom.vn', N'0900000001', N'HM66BGyKiwB01N9FlqT0tA==:TJiRh+SFuFvVT+37bTKTUPQfkftE3aJ7ny6Dr9G+ou4=', N'Active', CAST(N'2026-04-25T12:39:06.0000000' AS DateTime2), CAST(N'2026-04-25T12:39:07.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (13, N'Phạm Tiến Dũng', N'phamtiendung2k5hc@gmail.com', N'0392757286', N'tAlwo8QnrwYRT+Xxx828Tw==:qBASt/mJhqOadxMlUxyROKfmRyJ73sfcgNUp+8mibMA=', N'Active', CAST(N'2026-04-25T15:01:25.0000000' AS DateTime2), CAST(N'2026-04-25T15:01:25.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (14, N'Test User', N'testuser1@example.com', N'0123456789', N'TbBG4Jkhcf+oO++c1ol+5A==:suqwvLrl0fBE+I+MAIfl7hPaaBXWYODSEl4UJAqjqdI=', N'Active', CAST(N'2026-04-25T16:13:57.0000000' AS DateTime2), CAST(N'2026-04-25T16:13:57.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (15, N'Phạm Dũng', N'dung123@gmail.com', N'0392757287', N'PBKDF2$100000$LMMzgQzjB1tkmxd79IAtNw==$aU8Dvqmp/Snt45hc2F62d67m60rm5c3w95aaoXHuEv0=', N'Active', CAST(N'2026-05-11T08:02:07.0000000' AS DateTime2), CAST(N'2026-05-12T08:24:17.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG] ([MaNguoiDung], [HoTen], [Email], [SoDienThoai], [MatKhauHash], [TrangThai], [NgayTao], [NgayCapNhat]) VALUES (21, N'admin', N'admin123@gmail.com', N'0234567891', N'PBKDF2$100000$LMMzgQzjB1tkmxd79IAtNw==$aU8Dvqmp/Snt45hc2F62d67m60rm5c3w95aaoXHuEv0=', N'Active', CAST(N'2026-05-14T09:46:19.0000000' AS DateTime2), CAST(N'2026-05-14T09:46:19.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[NGUOIDUNG] OFF
GO
SET IDENTITY_INSERT [dbo].[NGUOIDUNG_DIACHI] ON 

INSERT [dbo].[NGUOIDUNG_DIACHI] ([MaDiaChi], [MaNguoiDung], [HoTenNhanHang], [SoDienThoaiNhanHang], [DiaChiNhanHang], [PhuongXa], [QuanHuyen], [TinhThanh], [GhiChu], [LaMacDinh], [NgayTao], [NgayCapNhat]) VALUES (1, 13, N'Phạm Tiến Dũng', N'0392757286', N'236 Hoàng Quốc Việt, Cổ Nhuế 1, phường Nghĩa Đô, TP Hà Nội', N'phường Nghĩa Đô', NULL, N'Hà Nội', N'giao cẩn thận', 1, CAST(N'2026-05-04T10:08:47.0000000' AS DateTime2), CAST(N'2026-05-04T10:08:47.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[NGUOIDUNG_DIACHI] OFF
GO
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (1, 5, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (2, 7, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (3, 7, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (4, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (5, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (6, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (7, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (8, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (9, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (10, 6, CAST(N'2026-04-24T10:23:09.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (13, 6, CAST(N'2026-05-04T10:02:59.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (14, 6, CAST(N'2026-04-25T16:13:57.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (15, 6, CAST(N'2026-05-11T08:02:07.0000000' AS DateTime2))
INSERT [dbo].[NGUOIDUNG_VAITRO] ([MaNguoiDung], [MaVaiTro], [NgayTao]) VALUES (21, 5, CAST(N'2026-05-14T09:47:01.0000000' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[PHUTUNG_TUONGTHICH] ON 

INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (7, 112, 1, 1, 2020, 2026, 0, N'Dành cho mâm 14 inch Air Blade', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (8, 112, 1, 2, NULL, NULL, 0, N'Tương thích LEAD', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (9, 112, 2, 4, 2019, 2026, 0, N'Lốp trước Exciter', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (10, 112, 2, 5, NULL, NULL, 0, N'FreeGo lắp vừa', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (11, 113, 1, NULL, NULL, NULL, 1, N'Dùng tốt cho tất cả xe số, tay ga Honda', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (12, 113, 2, NULL, NULL, NULL, 1, N'Dùng tốt cho tất cả xe Yamaha', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (13, 114, 1, 1, 2022, 2026, 0, N'Air Blade 160/125 ABS', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (14, 114, 1, 2, 2020, 2026, 0, N'LEAD ABS', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (15, 114, 2, 4, NULL, NULL, 0, N'Exciter 155 VVA', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (16, 114, 3, 9, NULL, NULL, 0, N'Vento S', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (17, 115, 1, 1, NULL, NULL, 0, N'Bugi đánh lửa cho Air Blade', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (18, 115, 1, 3, NULL, NULL, 0, N'Wave Alpha', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (19, 115, 2, 4, NULL, NULL, 0, N'Exciter 155', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
INSERT [dbo].[PHUTUNG_TUONGTHICH] ([MaTuongThich], [MaPhuTung], [MaHangXe], [MaDongXe], [NamTu], [NamDen], [ApDungTatCaXe], [GhiChu], [DangHoatDong], [NgayTao], [NgayCapNhat]) VALUES (20, 115, 2, 6, NULL, NULL, 0, N'Latte', 1, CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[PHUTUNG_TUONGTHICH] OFF
GO
SET IDENTITY_INSERT [dbo].[SANPHAM] ON 

INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (101, N'SP_AB160', N'Honda Air Blade 160', N'honda-air-blade-160', 2, 1, 1, N'Xe tay ga thể thao mạnh mẽ', NULL, CAST(56690000.00 AS Decimal(18, 2)), CAST(55990000.00 AS Decimal(18, 2)), 70, N'https://hethongxemayhuanlaihuong.com/wp-content/uploads/2024/03/64-1.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (102, N'SP_AB125', N'Honda Air Blade 125', N'honda-air-blade-125', 2, 1, 1, N'Xe tay ga phổ thông bán chạy', NULL, CAST(42090000.00 AS Decimal(18, 2)), NULL, 88, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPduxcVFdSBQZJFiN_JYhgkKj7bTc7FnRSsg&s', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (103, N'SP_LEAD', N'Honda LEAD ABS', N'honda-lead-abs', 2, 1, 2, N'Cốp siêu rộng, tiện ích cao', NULL, CAST(39590000.00 AS Decimal(18, 2)), CAST(38990000.00 AS Decimal(18, 2)), 65, N'https://cdn.honda.com.vn/motorbike-versions/Image360/November2025/1762149017/6.png', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (104, N'SP_WAVE', N'Honda Wave Alpha 110', N'honda-wave-alpha-110', 3, 1, 3, N'Xe số quốc dân bền bỉ', NULL, CAST(18190000.00 AS Decimal(18, 2)), NULL, 150, N'https://cdn.showroom.vn/images/wave-alpha-trang.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (105, N'SP_EX155', N'Yamaha Exciter 155 VVA', N'yamaha-exciter-155-vva', 4, 2, 4, N'Vua côn tay đường phố', NULL, CAST(48000000.00 AS Decimal(18, 2)), CAST(47000000.00 AS Decimal(18, 2)), 42, N'https://cdn.showroom.vn/images/exciter-155-den-bong.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (106, N'SP_FREEGO', N'Yamaha FreeGo S', N'yamaha-freego-s', 2, 2, 5, N'Tay ga thể thao giá rẻ', NULL, CAST(33800000.00 AS Decimal(18, 2)), NULL, 40, N'https://cdn.showroom.vn/images/freego-s-trang.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (107, N'SP_LATTE', N'Yamaha Latte', N'yamaha-latte', 2, 2, 6, N'Tay ga nữ tính thanh lịch', NULL, CAST(38000000.00 AS Decimal(18, 2)), NULL, 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (108, N'SP_EVO200', N'VinFast Evo200', N'vinfast-evo200', 5, 3, 7, N'Xe điện đô thị nhỏ gọn', NULL, CAST(22000000.00 AS Decimal(18, 2)), CAST(20500000.00 AS Decimal(18, 2)), 75, N'https://cdn.showroom.vn/images/evo200-trang-ngoc-trai.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (109, N'SP_KLARAS', N'VinFast Klara S', N'vinfast-klara-s', 5, 3, 8, N'Kiểu dáng Ý sang trọng', NULL, CAST(39900000.00 AS Decimal(18, 2)), CAST(38000000.00 AS Decimal(18, 2)), 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (110, N'SP_VENTO', N'VinFast Vento S', N'vinfast-vento-s', 5, 3, 9, N'Hiệu năng cao, công nghệ thông minh', NULL, CAST(56000000.00 AS Decimal(18, 2)), CAST(54000000.00 AS Decimal(18, 2)), 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (111, N'SP_THEON', N'VinFast Theon S', N'vinfast-theon-s', 5, 3, 10, N'Xe máy điện Flagship', NULL, CAST(69900000.00 AS Decimal(18, 2)), CAST(68000000.00 AS Decimal(18, 2)), 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (112, N'PT_MICHELIN', N'Lốp Michelin Pilot Street 2', N'lop-michelin-pilot-street-2', 8, 4, NULL, N'Bám đường tốt', NULL, CAST(1200000.00 AS Decimal(18, 2)), NULL, 180, N'https://cdn.showroom.vn/images/michelin-pilot-street.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (113, N'PT_MOTUL', N'Dầu nhớt Motul 300V', N'dau-nhot-motul-300v', 7, 5, NULL, N'Bảo vệ động cơ tối đa', NULL, CAST(450000.00 AS Decimal(18, 2)), CAST(420000.00 AS Decimal(18, 2)), 200, N'https://cdn.showroom.vn/images/motul-300v-xanh-la.jpg', 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T09:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (114, N'PT_PHANH', N'Má phanh Elig', N'ma-phanh-elig', 9, NULL, NULL, N'Phanh êm, an toàn', NULL, CAST(150000.00 AS Decimal(18, 2)), NULL, 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
INSERT [dbo].[SANPHAM] ([MaSanPham], [MaSanPhamKinhDoanh], [TenSanPham], [Slug], [MaDanhMuc], [MaHangXe], [MaDongXe], [MoTaNgan], [MoTa], [GiaGoc], [GiaKhuyenMai], [SoLuongTon], [AnhChinhUrl], [DangHoatDong], [TrangThaiSanPham], [NgayTao], [NgayCapNhat], [LoaiSanPham]) VALUES (115, N'PT_BUGI', N'Bugi NGK Iridium', N'bugi-ngk-iridium', 11, NULL, NULL, N'Đánh lửa mạnh, tiết kiệm xăng', NULL, CAST(250000.00 AS Decimal(18, 2)), CAST(230000.00 AS Decimal(18, 2)), 0, NULL, 1, N'Available', CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), CAST(N'2026-05-05T16:07:47.0000000' AS DateTime2), N'XeMay')
SET IDENTITY_INSERT [dbo].[SANPHAM] OFF
GO
GO
SET IDENTITY_INSERT [dbo].[TONKHO_GIUCHO] ON 

INSERT [dbo].[TONKHO_GIUCHO] ([MaGiuCho], [MaDonHang], [MaChiTietDonHang], [MaSanPham], [MaBienSanPham], [SoLuong], [TrangThai], [HetHanLuc], [NgayTao], [NgayCapNhat], [GhiChu]) VALUES (17, 69, 27, 101, 1001, 2, N'Cancelled', CAST(N'2026-05-14T08:46:31.0000000' AS DateTime2), CAST(N'2026-05-14T08:31:31.0000000' AS DateTime2), CAST(N'2026-05-14T08:32:20.0000000' AS DateTime2), N'Giu ton kho khi tao don hang | Huy don, nha giu cho')
INSERT [dbo].[TONKHO_GIUCHO] ([MaGiuCho], [MaDonHang], [MaChiTietDonHang], [MaSanPham], [MaBienSanPham], [SoLuong], [TrangThai], [HetHanLuc], [NgayTao], [NgayCapNhat], [GhiChu]) VALUES (18, 70, 28, 101, 1001, 1, N'Expired', CAST(N'2026-05-14T10:43:56.0000000' AS DateTime2), CAST(N'2026-05-14T10:28:56.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), N'Giu ton kho khi tao don hang | Tu dong het han giu cho')
INSERT [dbo].[TONKHO_GIUCHO] ([MaGiuCho], [MaDonHang], [MaChiTietDonHang], [MaSanPham], [MaBienSanPham], [SoLuong], [TrangThai], [HetHanLuc], [NgayTao], [NgayCapNhat], [GhiChu]) VALUES (19, 71, 29, 103, 1013, 1, N'Active', CAST(N'2026-05-19T09:37:22.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), N'Giu ton kho khi tao don hang')
INSERT [dbo].[TONKHO_GIUCHO] ([MaGiuCho], [MaDonHang], [MaChiTietDonHang], [MaSanPham], [MaBienSanPham], [SoLuong], [TrangThai], [HetHanLuc], [NgayTao], [NgayCapNhat], [GhiChu]) VALUES (20, 71, 30, 103, 1011, 1, N'Active', CAST(N'2026-05-19T09:37:22.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), CAST(N'2026-05-19T09:22:22.0000000' AS DateTime2), N'Giu ton kho khi tao don hang')
SET IDENTITY_INSERT [dbo].[TONKHO_GIUCHO] OFF
GO
SET IDENTITY_INSERT [dbo].[VAITRO] ON 

INSERT [dbo].[VAITRO] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (5, N'Admin', N'Quản trị hệ thống')
INSERT [dbo].[VAITRO] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (6, N'Customer', N'Khách hàng')
INSERT [dbo].[VAITRO] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES (7, N'Staff', N'Nhân viên showroom')
SET IDENTITY_INSERT [dbo].[VAITRO] OFF
GO
SET IDENTITY_INSERT [dbo].[VOUCHER] ON 

INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (5, N'BIKE1TR', N'Amount', CAST(1000000.00 AS Decimal(18, 2)), CAST(50000000.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 100, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 1.000.000đ cho xe máy cao cấp', 1, N'Product', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (6, N'BIKE300K', N'Amount', CAST(300000.00 AS Decimal(18, 2)), CAST(20000000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 300.000đ cho xe máy', 1, N'Product', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (7, N'BIKE500K', N'Amount', CAST(500000.00 AS Decimal(18, 2)), CAST(30000000.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 500.000đ cho xe máy chọn lọc', 1, N'Product', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (8, N'FLASH100K', N'Amount', CAST(100000.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-04-01T00:00:00.0000000' AS DateTime2), CAST(N'2026-12-31T00:00:00.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Flash sale giảm 100.000đ', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (9, N'FLASH200K', N'Amount', CAST(200000.00 AS Decimal(18, 2)), CAST(3000000.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(N'2026-04-01T00:00:00.0000000' AS DateTime2), CAST(N'2026-12-31T00:00:00.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Flash sale giảm 200.000đ', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (10, N'FREESHIP30', N'FreeShipping', CAST(30000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(30000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 1000, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Hỗ trợ phí vận chuyển 30.000đ', 3, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (11, N'FREESHIP50', N'FreeShipping', CAST(50000.00 AS Decimal(18, 2)), CAST(800000.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 800, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Hỗ trợ phí vận chuyển 50.000đ', 2, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (12, N'HONDA300', N'Amount', CAST(300000.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi cho sản phẩm thuộc hãng Honda', 1, N'Brand', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (13, N'MUBAOH10', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(80000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 700, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 10% mũ bảo hiểm', 2, N'Category', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (14, N'NHOTXE10', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 800, 1, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 10% dầu nhớt/phụ kiện bảo dưỡng', 2, N'Category', CAST(N'2026-05-19T16:22:23.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (15, N'PHUTUNG20', N'Percent', CAST(20.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 600, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 20% phụ tùng', 2, N'Category', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (16, N'PHUTUNG50K', N'Amount', CAST(50000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 800, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 50.000đ phụ tùng', 2, N'Category', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (17, N'SALE10', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(2000000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 800, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 10% toàn bộ đơn hàng', 2, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (18, N'SALE15', N'Percent', CAST(15.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(600000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 500, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 15% cho đơn giá trị cao', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (19, N'SALE5', N'Percent', CAST(5.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 1000, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 5% toàn bộ đơn hàng', 2, N'All', CAST(N'2026-05-14T15:32:20.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (20, N'STUDENT150K', N'Amount', CAST(150000.00 AS Decimal(18, 2)), CAST(2000000.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 500, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi học sinh sinh viên', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (21, N'SUMMER2026', N'Percent', CAST(8.00 AS Decimal(18, 2)), CAST(1500000.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(N'2026-05-01T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-31T00:00:00.0000000' AS DateTime2), 500, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi mùa hè 2026', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (22, N'SUZUKI250', N'Amount', CAST(250000.00 AS Decimal(18, 2)), CAST(12000000.00 AS Decimal(18, 2)), CAST(250000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 250, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi cho sản phẩm thuộc hãng Suzuki', 1, N'Brand', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (23, N'SYM200', N'Amount', CAST(200000.00 AS Decimal(18, 2)), CAST(10000000.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 250, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi cho sản phẩm thuộc hãng SYM', 1, N'Brand', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (24, N'TET2027', N'Percent', CAST(7.00 AS Decimal(18, 2)), CAST(1500000.00 AS Decimal(18, 2)), CAST(250000.00 AS Decimal(18, 2)), CAST(N'2026-12-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-02-28T00:00:00.0000000' AS DateTime2), 500, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi dịp Tết 2027', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (25, N'VIP500K', N'Amount', CAST(500000.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 150, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi khách hàng VIP', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (26, N'WELCOME100', N'Amount', CAST(100000.00 AS Decimal(18, 2)), CAST(1500000.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 100.000đ cho đơn đầu tiên', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (27, N'WELCOME50', N'Amount', CAST(50000.00 AS Decimal(18, 2)), CAST(500000.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 500, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Giảm 50.000đ cho khách hàng mới', 1, N'All', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (28, N'YAMAHA300', N'Amount', CAST(300000.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(N'2026-01-01T00:00:00.0000000' AS DateTime2), CAST(N'2027-12-31T00:00:00.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), N'Ưu đãi cho sản phẩm thuộc hãng Yamaha', 1, N'Brand', CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2), NULL)
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (29, N'FULLPAY1TR', N'Amount', CAST(1000000.00 AS Decimal(18, 2)), CAST(25000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 1 triệu cho đơn thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (30, N'FULLPAY2TR', N'Amount', CAST(2000000.00 AS Decimal(18, 2)), CAST(30000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 150, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 2 triệu khi thanh toán 100% giá trị đơn hàng', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (31, N'FULLPAY3TR', N'Amount', CAST(3000000.00 AS Decimal(18, 2)), CAST(50000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 100, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 3 triệu cho khách thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (32, N'FULLPAY5TR', N'Amount', CAST(5000000.00 AS Decimal(18, 2)), CAST(80000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-06-27T16:13:10.0000000' AS DateTime2), 50, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 5 triệu cho đơn full payment giá trị cao', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (33, N'FULLPAY3P', N'Percent', CAST(3.00 AS Decimal(18, 2)), CAST(30000000.00 AS Decimal(18, 2)), CAST(2000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 3% tối đa 2 triệu khi thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (34, N'FULLPAY5P', N'Percent', CAST(5.00 AS Decimal(18, 2)), CAST(40000000.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 120, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 5% tối đa 5 triệu cho đơn thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (35, N'FULLPAY7P', N'Percent', CAST(7.00 AS Decimal(18, 2)), CAST(70000000.00 AS Decimal(18, 2)), CAST(7000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-06-27T16:13:10.0000000' AS DateTime2), 70, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 7% tối đa 7 triệu cho đơn full payment', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (36, N'FULLVIP10', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(100000000.00 AS Decimal(18, 2)), CAST(10000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-05-27T16:13:10.0000000' AS DateTime2), 30, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 10% tối đa 10 triệu cho khách thanh toán toàn bộ đơn lớn', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (37, N'PAYNOW500K', N'Amount', CAST(500000.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-10-27T16:13:10.0000000' AS DateTime2), 300, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Ưu đãi thanh toán toàn bộ online - giảm 500 nghìn', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (38, N'PAYNOW800K', N'Amount', CAST(800000.00 AS Decimal(18, 2)), CAST(20000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-10-27T16:13:10.0000000' AS DateTime2), 250, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 800 nghìn cho khách trả đủ ngay', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (39, N'FULLHONDA2', N'Amount', CAST(2000000.00 AS Decimal(18, 2)), CAST(35000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 80, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Voucher thanh toán toàn bộ cho xe Honda', 1, N'Brand', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (40, N'FULLYAMAHA2', N'Amount', CAST(2000000.00 AS Decimal(18, 2)), CAST(30000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 80, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Voucher thanh toán toàn bộ cho xe Yamaha', 1, N'Brand', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (41, N'FULLVF5TR', N'Amount', CAST(5000000.00 AS Decimal(18, 2)), CAST(60000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 60, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Voucher thanh toán toàn bộ cho xe điện VinFast', 1, N'Brand', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (42, N'FULLSUV3TR', N'Amount', CAST(3000000.00 AS Decimal(18, 2)), CAST(70000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-06-27T16:13:10.0000000' AS DateTime2), 50, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 3 triệu cho dòng xe giá trị cao khi thanh toán toàn bộ', 1, N'Category', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (43, N'FULLBIKE1TR', N'Amount', CAST(1000000.00 AS Decimal(18, 2)), CAST(20000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-08-27T16:13:10.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm 1 triệu cho xe máy khi thanh toán toàn bộ', 1, N'Category', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (44, N'FULLNEW2026', N'Percent', CAST(6.00 AS Decimal(18, 2)), CAST(40000000.00 AS Decimal(18, 2)), CAST(6000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-07-27T16:13:10.0000000' AS DateTime2), 100, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Ưu đãi đầu năm 2026 cho khách thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (45, N'FULLFAST', N'Amount', CAST(1500000.00 AS Decimal(18, 2)), CAST(25000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-06-27T16:13:10.0000000' AS DateTime2), 150, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Giảm nhanh 1.5 triệu khi thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (46, N'FULLONLINE', N'Amount', CAST(1200000.00 AS Decimal(18, 2)), CAST(20000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-09-27T16:13:10.0000000' AS DateTime2), 200, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Ưu đãi riêng cho thanh toán toàn bộ online', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (47, N'FULLPREMIUM', N'Percent', CAST(8.00 AS Decimal(18, 2)), CAST(90000000.00 AS Decimal(18, 2)), CAST(8000000.00 AS Decimal(18, 2)), CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-06-27T16:13:10.0000000' AS DateTime2), 40, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Voucher premium cho khách thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
INSERT [dbo].[VOUCHER] ([MaVoucher], [MaVoucherCode], [LoaiGiamGia], [GiaTriGiam], [GiaTriDonToiThieu], [GiaTriGiamToiDa], [NgayBatDau], [NgayKetThuc], [GioiHanSuDung], [SoLanDaDung], [DangHoatDong], [NgayTao], [MoTa], [SoLanToiDaMoiNguoiDung], [PhamViApDung], [NgayCapNhat], [ApDungLoaiDonHang]) VALUES (48, N'FULLLOYAL', N'Amount', CAST(2500000.00 AS Decimal(18, 2)), CAST(45000000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), CAST(N'2026-08-27T16:13:10.0000000' AS DateTime2), 100, 0, 1, CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'Ưu đãi khách hàng thân thiết thanh toán toàn bộ', 1, N'All', CAST(N'2026-04-27T16:13:10.0000000' AS DateTime2), N'FullPayment')
SET IDENTITY_INSERT [dbo].[VOUCHER] OFF
GO
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 7, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 8, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 9, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 10, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (13, 11, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 7, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 8, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_DANHMUC] ([MaVoucher], [MaDanhMuc], [NgayTao]) VALUES (14, 9, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
GO
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (12, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (12, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (12, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (12, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (12, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (22, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (22, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (22, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (22, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (22, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (23, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (23, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (23, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (23, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (23, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (28, 1, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (28, 2, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (28, 3, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (28, 4, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_HANGXE] ([MaVoucher], [MaHangXe], [NgayTao]) VALUES (28, 5, CAST(N'2026-04-27T15:40:33.0000000' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[VOUCHER_NGUOIDUNG] ON 

INSERT [dbo].[VOUCHER_NGUOIDUNG] ([MaVoucherNguoiDung], [MaVoucher], [MaNguoiDung], [MaDonHang], [MaVoucherCodeSnapshot], [LoaiGiamGiaSnapshot], [GiaTriGiamSnapshot], [SoTienGiam], [TrangThai], [NgaySuDung], [NgayTao]) VALUES (21, 19, 15, 69, N'SALE5', N'Percent', CAST(5.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), N'Cancelled', CAST(N'2026-05-14T15:31:31.0000000' AS DateTime2), CAST(N'2026-05-14T15:31:31.0000000' AS DateTime2))
INSERT [dbo].[VOUCHER_NGUOIDUNG] ([MaVoucherNguoiDung], [MaVoucher], [MaNguoiDung], [MaDonHang], [MaVoucherCodeSnapshot], [LoaiGiamGiaSnapshot], [GiaTriGiamSnapshot], [SoTienGiam], [TrangThai], [NgaySuDung], [NgayTao]) VALUES (22, 14, 15, 71, N'NHOTXE10', N'Percent', CAST(10.00 AS Decimal(18, 2)), CAST(50000.00 AS Decimal(18, 2)), N'Used', CAST(N'2026-05-19T16:22:23.0000000' AS DateTime2), CAST(N'2026-05-19T16:22:23.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[VOUCHER_NGUOIDUNG] OFF
GO
/****** Object:  Index [IX_ANHSANPHAM_Product]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_ANHSANPHAM_Product] ON [dbo].[ANHSANPHAM]
(
	[MaSanPham] ASC,
	[ThuTuHienThi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ANHSANPHAM_OneMainImage_PerVariant]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ANHSANPHAM_OneMainImage_PerVariant] ON [dbo].[ANHSANPHAM]
(
	[MaBienSanPham] ASC
)
WHERE ([MaBienSanPham] IS NOT NULL AND [LaAnhChinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ANHSANPHAM_Primary]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ANHSANPHAM_Primary] ON [dbo].[ANHSANPHAM]
(
	[MaSanPham] ASC
)
WHERE ([MaBienSanPham] IS NULL AND [LaAnhChinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_BAIVIET_Slug]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[BAIVIET] ADD  CONSTRAINT [UQ_BAIVIET_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BAIVIET_Category]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_BAIVIET_Category] ON [dbo].[BAIVIET]
(
	[DanhMuc] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BAIVIET_Status_Published]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_BAIVIET_Status_Published] ON [dbo].[BAIVIET]
(
	[TrangThai] ASC,
	[XuatBanLuc] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_BIENSANPHAM_SKU]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[BIENSANPHAM] ADD  CONSTRAINT [UQ_BIENSANPHAM_SKU] UNIQUE NONCLUSTERED 
(
	[SKU] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BIENSANPHAM_MaSanPham_MauSac]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_BIENSANPHAM_MaSanPham_MauSac] ON [dbo].[BIENSANPHAM]
(
	[MaSanPham] ASC,
	[MauSac] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BIENSANPHAM_Product]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_BIENSANPHAM_Product] ON [dbo].[BIENSANPHAM]
(
	[MaSanPham] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CHITIET_DONHANG_Product]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_CHITIET_DONHANG_Product] ON [dbo].[CHITIET_DONHANG]
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_CHITIET_GIOHANG_NoVariant]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CHITIET_GIOHANG_NoVariant] ON [dbo].[CHITIET_GIOHANG]
(
	[MaGioHang] ASC,
	[MaSanPham] ASC
)
WHERE ([MaBienSanPham] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_CHITIET_GIOHANG_WithVariant]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CHITIET_GIOHANG_WithVariant] ON [dbo].[CHITIET_GIOHANG]
(
	[MaGioHang] ASC,
	[MaSanPham] ASC,
	[MaBienSanPham] ASC
)
WHERE ([MaBienSanPham] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DANHGIA_Product_Status]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DANHGIA_Product_Status] ON [dbo].[DANHGIASANPHAM]
(
	[MaSanPham] ASC,
	[TrangThai] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_DANHGIA_User_Product]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_DANHGIA_User_Product] ON [dbo].[DANHGIASANPHAM]
(
	[MaNguoiDung] ASC,
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_DANHMUC_Slug]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[DANHMUC] ADD  CONSTRAINT [UQ_DANHMUC_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DANHMUC_Parent]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DANHMUC_Parent] ON [dbo].[DANHMUC]
(
	[MaDanhMucCha] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_DONGXE_Hang_Ten]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[DONGXE] ADD  CONSTRAINT [UQ_DONGXE_Hang_Ten] UNIQUE NONCLUSTERED 
(
	[MaHangXe] ASC,
	[TenDongXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_DONGXE_Slug]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[DONGXE] ADD  CONSTRAINT [UQ_DONGXE_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONGXE_Hang]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONGXE_Hang] ON [dbo].[DONGXE]
(
	[MaHangXe] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_DONHANG_Code]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [UQ_DONHANG_Code] UNIQUE NONCLUSTERED 
(
	[MaDonHangKinhDoanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_DatCoc]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_DatCoc] ON [dbo].[DONHANG]
(
	[LoaiDonHang] ASC,
	[TrangThaiThanhToan] ASC,
	[SoTienConLai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_GiaoNhan]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_GiaoNhan] ON [dbo].[DONHANG]
(
	[PhuongThucNhanHang] ASC,
	[TrangThaiVanChuyen] ASC,
	[NgayHenNhanXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONHANG_MaGioHang]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_MaGioHang] ON [dbo].[DONHANG]
(
	[MaGioHang] ASC
)
WHERE ([MaGioHang] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_Status_Date]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_Status_Date] ON [dbo].[DONHANG]
(
	[TrangThaiDonHang] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONHANG_User_Date]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_User_Date] ON [dbo].[DONHANG]
(
	[MaNguoiDung] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_DONHANG_MaGioHang_NotNull]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_DONHANG_MaGioHang_NotNull] ON [dbo].[DONHANG]
(
	[MaGioHang] ASC
)
WHERE ([MaGioHang] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FAQ_Category_Active]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_FAQ_Category_Active] ON [dbo].[FAQ]
(
	[DanhMuc] ASC,
	[DangHoatDong] ASC,
	[ThuTuHienThi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_GIOHANG_User_Active]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_GIOHANG_User_Active] ON [dbo].[GIOHANG]
(
	[MaNguoiDung] ASC
)
WHERE ([TrangThai]='Active')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_HANGXE_Slug]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[HANGXE] ADD  CONSTRAINT [UQ_HANGXE_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_HANGXE_TenHang]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[HANGXE] ADD  CONSTRAINT [UQ_HANGXE_TenHang] UNIQUE NONCLUSTERED 
(
	[TenHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LIENHE_Product]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_LIENHE_Product] ON [dbo].[LIENHE_YEUCAU]
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_LIENHE_Status_Date]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_LIENHE_Status_Date] ON [dbo].[LIENHE_YEUCAU]
(
	[TrangThai] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_NGUOIDUNG_Email]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[NGUOIDUNG] ADD  CONSTRAINT [UQ_NGUOIDUNG_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_NGUOIDUNG_SoDienThoai]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_NGUOIDUNG_SoDienThoai] ON [dbo].[NGUOIDUNG]
(
	[SoDienThoai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_NGUOIDUNG_DIACHI_MacDinh]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_NGUOIDUNG_DIACHI_MacDinh] ON [dbo].[NGUOIDUNG_DIACHI]
(
	[MaNguoiDung] ASC
)
WHERE ([LaMacDinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PHUTUNG_TUONGTHICH_LocXe]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_PHUTUNG_TUONGTHICH_LocXe] ON [dbo].[PHUTUNG_TUONGTHICH]
(
	[MaHangXe] ASC,
	[MaDongXe] ASC,
	[NamTu] ASC,
	[NamDen] ASC,
	[DangHoatDong] ASC
)
INCLUDE([MaPhuTung],[ApDungTatCaXe]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PHUTUNG_TUONGTHICH_MaPhuTung]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_PHUTUNG_TUONGTHICH_MaPhuTung] ON [dbo].[PHUTUNG_TUONGTHICH]
(
	[MaPhuTung] ASC,
	[DangHoatDong] ASC
)
INCLUDE([MaHangXe],[MaDongXe],[NamTu],[NamDen],[ApDungTatCaXe]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_SANPHAM_Code]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [UQ_SANPHAM_Code] UNIQUE NONCLUSTERED 
(
	[MaSanPhamKinhDoanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_SANPHAM_Slug]    Script Date: 5/20/2026 1:01:25 PM ******/
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [UQ_SANPHAM_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SANPHAM_Brand_Model]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_SANPHAM_Brand_Model] ON [dbo].[SANPHAM]
(
	[MaHangXe] ASC,
	[MaDongXe] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SANPHAM_Price]    Script Date: 5/20/2026 1:01:25 PM ******/
CREATE NONCLUSTERED INDEX [IX_SANPHAM_Price] ON [dbo].[SANPHAM]
(
	[GiaGoc] ASC,
	[GiaKhuyenMai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
ALTER TABLE [dbo].[THANHTOAN] ADD  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] ADD  CONSTRAINT [DF_TONKHO_GIUCHO_TrangThai]  DEFAULT ('Active') FOR [TrangThai]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] ADD  CONSTRAINT [DF_TONKHO_GIUCHO_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] ADD  CONSTRAINT [DF_TONKHO_GIUCHO_NgayCapNhat]  DEFAULT (sysdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_Min]  DEFAULT ((0)) FOR [GiaTriDonToiThieu]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_Used]  DEFAULT ((0)) FOR [SoLanDaDung]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_SoLanToiDaMoiNguoiDung]  DEFAULT ((1)) FOR [SoLanToiDaMoiNguoiDung]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_PhamViApDung]  DEFAULT ('All') FOR [PhamViApDung]
GO
ALTER TABLE [dbo].[VOUCHER] ADD  CONSTRAINT [DF_VOUCHER_NgayCapNhat]  DEFAULT (sysdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[VOUCHER_DANHMUC] ADD  CONSTRAINT [DF_VOUCHER_DANHMUC_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[VOUCHER_HANGXE] ADD  CONSTRAINT [DF_VOUCHER_HANGXE_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] ADD  CONSTRAINT [DF_VOUCHER_NGUOIDUNG_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[VOUCHER_SANPHAM] ADD  CONSTRAINT [DF_VOUCHER_SANPHAM_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[YEUTHICH] ADD  CONSTRAINT [DF_YEUTHICH_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[ANHSANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_ANHSANPHAM_BIENSANPHAM] FOREIGN KEY([MaBienSanPham])
REFERENCES [dbo].[BIENSANPHAM] ([MaBienSanPham])
GO
ALTER TABLE [dbo].[ANHSANPHAM] CHECK CONSTRAINT [FK_ANHSANPHAM_BIENSANPHAM]
GO
ALTER TABLE [dbo].[ANHSANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_ANHSANPHAM_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ANHSANPHAM] CHECK CONSTRAINT [FK_ANHSANPHAM_SANPHAM]
GO
ALTER TABLE [dbo].[BAIVIET]  WITH NOCHECK ADD  CONSTRAINT [FK_BAIVIET_TACGIA] FOREIGN KEY([MaTacGia])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
GO
ALTER TABLE [dbo].[BAIVIET] CHECK CONSTRAINT [FK_BAIVIET_TACGIA]
GO
ALTER TABLE [dbo].[BIENSANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_BIENSANPHAM_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BIENSANPHAM] CHECK CONSTRAINT [FK_BIENSANPHAM_SANPHAM]
GO
ALTER TABLE [dbo].[CHITIET_DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_DONHANG_BIENSANPHAM] FOREIGN KEY([MaBienSanPham])
REFERENCES [dbo].[BIENSANPHAM] ([MaBienSanPham])
GO
ALTER TABLE [dbo].[CHITIET_DONHANG] CHECK CONSTRAINT [FK_CHITIET_DONHANG_BIENSANPHAM]
GO
ALTER TABLE [dbo].[CHITIET_DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_DONHANG_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CHITIET_DONHANG] CHECK CONSTRAINT [FK_CHITIET_DONHANG_DONHANG]
GO
ALTER TABLE [dbo].[CHITIET_DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_DONHANG_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[CHITIET_DONHANG] CHECK CONSTRAINT [FK_CHITIET_DONHANG_SANPHAM]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_GIOHANG_BIENSANPHAM] FOREIGN KEY([MaBienSanPham])
REFERENCES [dbo].[BIENSANPHAM] ([MaBienSanPham])
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] CHECK CONSTRAINT [FK_CHITIET_GIOHANG_BIENSANPHAM]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_GIOHANG_GIOHANG] FOREIGN KEY([MaGioHang])
REFERENCES [dbo].[GIOHANG] ([MaGioHang])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] CHECK CONSTRAINT [FK_CHITIET_GIOHANG_GIOHANG]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_CHITIET_GIOHANG_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] CHECK CONSTRAINT [FK_CHITIET_GIOHANG_SANPHAM]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_DANHGIA_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] CHECK CONSTRAINT [FK_DANHGIA_DONHANG]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_DANHGIA_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] CHECK CONSTRAINT [FK_DANHGIA_NGUOIDUNG]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_DANHGIA_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] CHECK CONSTRAINT [FK_DANHGIA_SANPHAM]
GO
ALTER TABLE [dbo].[DANHMUC]  WITH NOCHECK ADD  CONSTRAINT [FK_DANHMUC_Parent] FOREIGN KEY([MaDanhMucCha])
REFERENCES [dbo].[DANHMUC] ([MaDanhMuc])
GO
ALTER TABLE [dbo].[DANHMUC] CHECK CONSTRAINT [FK_DANHMUC_Parent]
GO
ALTER TABLE [dbo].[DONGXE]  WITH NOCHECK ADD  CONSTRAINT [FK_DONGXE_HANGXE] FOREIGN KEY([MaHangXe])
REFERENCES [dbo].[HANGXE] ([MaHangXe])
GO
ALTER TABLE [dbo].[DONGXE] CHECK CONSTRAINT [FK_DONGXE_HANGXE]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_DONHANG_GIOHANG] FOREIGN KEY([MaGioHang])
REFERENCES [dbo].[GIOHANG] ([MaGioHang])
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [FK_DONHANG_GIOHANG]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_DONHANG_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [FK_DONHANG_NGUOIDUNG]
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [FK_DONHANG_VOUCHER_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER] CHECK CONSTRAINT [FK_DONHANG_VOUCHER_DONHANG]
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [FK_DONHANG_VOUCHER_VOUCHER] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[VOUCHER] ([MaVoucher])
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER] CHECK CONSTRAINT [FK_DONHANG_VOUCHER_VOUCHER]
GO
ALTER TABLE [dbo].[GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_GIOHANG_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GIOHANG] CHECK CONSTRAINT [FK_GIOHANG_NGUOIDUNG]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [FK_LIENHE_NGUOIXULY] FOREIGN KEY([MaNguoiXuLy])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [FK_LIENHE_NGUOIXULY]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [FK_LIENHE_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [FK_LIENHE_SANPHAM]
GO
ALTER TABLE [dbo].[NGUOIDUNG_VAITRO]  WITH NOCHECK ADD  CONSTRAINT [FK_NGUOIDUNG_VAITRO_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NGUOIDUNG_VAITRO] CHECK CONSTRAINT [FK_NGUOIDUNG_VAITRO_NGUOIDUNG]
GO
ALTER TABLE [dbo].[NGUOIDUNG_VAITRO]  WITH NOCHECK ADD  CONSTRAINT [FK_NGUOIDUNG_VAITRO_VAITRO] FOREIGN KEY([MaVaiTro])
REFERENCES [dbo].[VAITRO] ([MaVaiTro])
GO
ALTER TABLE [dbo].[NGUOIDUNG_VAITRO] CHECK CONSTRAINT [FK_NGUOIDUNG_VAITRO_VAITRO]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [FK_PHUTUNG_TUONGTHICH_DONGXE] FOREIGN KEY([MaDongXe])
REFERENCES [dbo].[DONGXE] ([MaDongXe])
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [FK_PHUTUNG_TUONGTHICH_DONGXE]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [FK_PHUTUNG_TUONGTHICH_HANGXE] FOREIGN KEY([MaHangXe])
REFERENCES [dbo].[HANGXE] ([MaHangXe])
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [FK_PHUTUNG_TUONGTHICH_HANGXE]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [FK_PHUTUNG_TUONGTHICH_SANPHAM] FOREIGN KEY([MaPhuTung])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [FK_PHUTUNG_TUONGTHICH_SANPHAM]
GO
ALTER TABLE [dbo].[SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_SANPHAM_DANHMUC] FOREIGN KEY([MaDanhMuc])
REFERENCES [dbo].[DANHMUC] ([MaDanhMuc])
GO
ALTER TABLE [dbo].[SANPHAM] CHECK CONSTRAINT [FK_SANPHAM_DANHMUC]
GO
ALTER TABLE [dbo].[SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_SANPHAM_DONGXE] FOREIGN KEY([MaDongXe])
REFERENCES [dbo].[DONGXE] ([MaDongXe])
GO
ALTER TABLE [dbo].[SANPHAM] CHECK CONSTRAINT [FK_SANPHAM_DONGXE]
GO
ALTER TABLE [dbo].[SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_SANPHAM_HANGXE] FOREIGN KEY([MaHangXe])
REFERENCES [dbo].[HANGXE] ([MaHangXe])
GO
ALTER TABLE [dbo].[SANPHAM] CHECK CONSTRAINT [FK_SANPHAM_HANGXE]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [FK_THANHTOAN_DONHANG_MaDonHang] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [FK_THANHTOAN_DONHANG_MaDonHang]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [FK_TONKHO_GIUCHO_BIENSANPHAM] FOREIGN KEY([MaBienSanPham])
REFERENCES [dbo].[BIENSANPHAM] ([MaBienSanPham])
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [FK_TONKHO_GIUCHO_BIENSANPHAM]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [FK_TONKHO_GIUCHO_CHITIET_DONHANG] FOREIGN KEY([MaChiTietDonHang])
REFERENCES [dbo].[CHITIET_DONHANG] ([MaChiTietDonHang])
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [FK_TONKHO_GIUCHO_CHITIET_DONHANG]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [FK_TONKHO_GIUCHO_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [FK_TONKHO_GIUCHO_DONHANG]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [FK_TONKHO_GIUCHO_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [FK_TONKHO_GIUCHO_SANPHAM]
GO
ALTER TABLE [dbo].[VOUCHER_DANHMUC]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_DANHMUC_DANHMUC] FOREIGN KEY([MaDanhMuc])
REFERENCES [dbo].[DANHMUC] ([MaDanhMuc])
GO
ALTER TABLE [dbo].[VOUCHER_DANHMUC] CHECK CONSTRAINT [FK_VOUCHER_DANHMUC_DANHMUC]
GO
ALTER TABLE [dbo].[VOUCHER_DANHMUC]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_DANHMUC_VOUCHER] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[VOUCHER] ([MaVoucher])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VOUCHER_DANHMUC] CHECK CONSTRAINT [FK_VOUCHER_DANHMUC_VOUCHER]
GO
ALTER TABLE [dbo].[VOUCHER_HANGXE]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_HANGXE_HANGXE] FOREIGN KEY([MaHangXe])
REFERENCES [dbo].[HANGXE] ([MaHangXe])
GO
ALTER TABLE [dbo].[VOUCHER_HANGXE] CHECK CONSTRAINT [FK_VOUCHER_HANGXE_HANGXE]
GO
ALTER TABLE [dbo].[VOUCHER_HANGXE]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_HANGXE_VOUCHER] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[VOUCHER] ([MaVoucher])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VOUCHER_HANGXE] CHECK CONSTRAINT [FK_VOUCHER_HANGXE_VOUCHER]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG]  WITH CHECK ADD  CONSTRAINT [FK_VOUCHER_NGUOIDUNG_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] CHECK CONSTRAINT [FK_VOUCHER_NGUOIDUNG_DONHANG]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG]  WITH CHECK ADD  CONSTRAINT [FK_VOUCHER_NGUOIDUNG_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] CHECK CONSTRAINT [FK_VOUCHER_NGUOIDUNG_NGUOIDUNG]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG]  WITH CHECK ADD  CONSTRAINT [FK_VOUCHER_NGUOIDUNG_VOUCHER] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[VOUCHER] ([MaVoucher])
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] CHECK CONSTRAINT [FK_VOUCHER_NGUOIDUNG_VOUCHER]
GO
ALTER TABLE [dbo].[VOUCHER_SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_SANPHAM_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
GO
ALTER TABLE [dbo].[VOUCHER_SANPHAM] CHECK CONSTRAINT [FK_VOUCHER_SANPHAM_SANPHAM]
GO
ALTER TABLE [dbo].[VOUCHER_SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_VOUCHER_SANPHAM_VOUCHER] FOREIGN KEY([MaVoucher])
REFERENCES [dbo].[VOUCHER] ([MaVoucher])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VOUCHER_SANPHAM] CHECK CONSTRAINT [FK_VOUCHER_SANPHAM_VOUCHER]
GO
ALTER TABLE [dbo].[YEUTHICH]  WITH NOCHECK ADD  CONSTRAINT [FK_YEUTHICH_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[YEUTHICH] CHECK CONSTRAINT [FK_YEUTHICH_NGUOIDUNG]
GO
ALTER TABLE [dbo].[YEUTHICH]  WITH NOCHECK ADD  CONSTRAINT [FK_YEUTHICH_SANPHAM] FOREIGN KEY([MaSanPham])
REFERENCES [dbo].[SANPHAM] ([MaSanPham])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[YEUTHICH] CHECK CONSTRAINT [FK_YEUTHICH_SANPHAM]
GO
ALTER TABLE [dbo].[BAIVIET]  WITH NOCHECK ADD  CONSTRAINT [CK_BAIVIET_TrangThai] CHECK  (([TrangThai]='Archived' OR [TrangThai]='Published' OR [TrangThai]='Draft'))
GO
ALTER TABLE [dbo].[BAIVIET] CHECK CONSTRAINT [CK_BAIVIET_TrangThai]
GO
ALTER TABLE [dbo].[CHITIET_DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_CHITIET_DONHANG_DonGia] CHECK  (([DonGia]>=(0)))
GO
ALTER TABLE [dbo].[CHITIET_DONHANG] CHECK CONSTRAINT [CK_CHITIET_DONHANG_DonGia]
GO
ALTER TABLE [dbo].[CHITIET_DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_CHITIET_DONHANG_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[CHITIET_DONHANG] CHECK CONSTRAINT [CK_CHITIET_DONHANG_SoLuong]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_CHITIET_GIOHANG_DonGia] CHECK  (([DonGia]>=(0)))
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] CHECK CONSTRAINT [CK_CHITIET_GIOHANG_DonGia]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_CHITIET_GIOHANG_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] CHECK CONSTRAINT [CK_CHITIET_GIOHANG_SoLuong]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM]  WITH NOCHECK ADD  CONSTRAINT [CK_DANHGIA_Diem] CHECK  (([Diem]>=(1) AND [Diem]<=(5)))
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] CHECK CONSTRAINT [CK_DANHGIA_Diem]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM]  WITH NOCHECK ADD  CONSTRAINT [CK_DANHGIA_TrangThai] CHECK  (([TrangThai]='Rejected' OR [TrangThai]='Approved' OR [TrangThai]='Pending'))
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] CHECK CONSTRAINT [CK_DANHGIA_TrangThai]
GO
ALTER TABLE [dbo].[DONHANG]  WITH CHECK ADD  CONSTRAINT [CK_DONHANG_DatCoc] CHECK  (([TienDatCoc]>=(0) AND [SoTienConLai]>=(0) AND [TienDatCoc]<=[TongThanhToan] AND [SoTienConLai]<=[TongThanhToan] AND ([LoaiDonHang]='FullPayment' AND [TienDatCoc]=(0) AND [SoTienConLai]=(0) OR [LoaiDonHang]='Deposit' AND [TienDatCoc]>(0) AND [TienDatCoc]<[TongThanhToan] OR [LoaiDonHang]='Installment' AND [TienDatCoc]>=(0) AND [SoTienConLai]>(0) AND ([TienDatCoc]+[SoTienConLai])<=[TongThanhToan])))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_DatCoc]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_Email] CHECK  (([EmailNhanHang] IS NULL OR [EmailNhanHang] like N'%_@_%._%'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_Email]
GO
ALTER TABLE [dbo].[DONHANG]  WITH CHECK ADD  CONSTRAINT [CK_DONHANG_LoaiDonHang] CHECK  (([LoaiDonHang]='Installment' OR [LoaiDonHang]='Deposit' OR [LoaiDonHang]='FullPayment'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_LoaiDonHang]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_OrderStatus] CHECK  (([TrangThaiDonHang]='Cancelled' OR [TrangThaiDonHang]='Completed' OR [TrangThaiDonHang]='Delivered' OR [TrangThaiDonHang]='Processing' OR [TrangThaiDonHang]='Shipping' OR [TrangThaiDonHang]='Confirmed' OR [TrangThaiDonHang]='AwaitingPayment' OR [TrangThaiDonHang]='Checkout' OR [TrangThaiDonHang]='Pending'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_OrderStatus]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_PaymentStatus] CHECK  (([TrangThaiThanhToan]='Refunded' OR [TrangThaiThanhToan]='Failed' OR [TrangThaiThanhToan]='Paid' OR [TrangThaiThanhToan]='PartiallyPaid' OR [TrangThaiThanhToan]='Unpaid'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_PaymentStatus]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_PhuongThucNhanHang] CHECK  (([PhuongThucNhanHang]='Pickup' OR [PhuongThucNhanHang]='Delivery'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_PhuongThucNhanHang]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_Tien] CHECK  (([TongTienHang]>=(0) AND [TienGiam]>=(0) AND [PhiVanChuyen]>=(0) AND [TongThanhToan]>=(0)))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_Tien]
GO
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_TrangThaiVanChuyen] CHECK  (([TrangThaiVanChuyen]='Cancelled' OR [TrangThaiVanChuyen]='PickedUp' OR [TrangThaiVanChuyen]='PickupReady' OR [TrangThaiVanChuyen]='Delivered' OR [TrangThaiVanChuyen]='Shipping' OR [TrangThaiVanChuyen]='Preparing' OR [TrangThaiVanChuyen]='NotShipped'))
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [CK_DONHANG_TrangThaiVanChuyen]
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_VOUCHER_Discount] CHECK  (([SoTienGiam]>=(0)))
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER] CHECK CONSTRAINT [CK_DONHANG_VOUCHER_Discount]
GO
ALTER TABLE [dbo].[GIOHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_GIOHANG_TrangThai] CHECK  (([TrangThai]='Abandoned' OR [TrangThai]='CheckedOut' OR [TrangThai]='Active'))
GO
ALTER TABLE [dbo].[GIOHANG] CHECK CONSTRAINT [CK_GIOHANG_TrangThai]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [CK_LIENHE_Email] CHECK  (([Email] IS NULL OR [Email] like N'%_@_%._%'))
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [CK_LIENHE_Email]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [CK_LIENHE_Loai] CHECK  (([LoaiYeuCau]='Consultation' OR [LoaiYeuCau]='TestDrive' OR [LoaiYeuCau]='Product' OR [LoaiYeuCau]='General'))
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [CK_LIENHE_Loai]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [CK_LIENHE_TrangThai] CHECK  (([TrangThai]='Spam' OR [TrangThai]='Closed' OR [TrangThai]='Processing' OR [TrangThai]='New'))
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [CK_LIENHE_TrangThai]
GO
ALTER TABLE [dbo].[NGUOIDUNG]  WITH NOCHECK ADD  CONSTRAINT [CK_NGUOIDUNG_Email] CHECK  (([Email] like N'%_@_%._%'))
GO
ALTER TABLE [dbo].[NGUOIDUNG] CHECK CONSTRAINT [CK_NGUOIDUNG_Email]
GO
ALTER TABLE [dbo].[NGUOIDUNG]  WITH NOCHECK ADD  CONSTRAINT [CK_NGUOIDUNG_SoDienThoai] CHECK  ((len([SoDienThoai])>=(9) AND len([SoDienThoai])<=(15) AND NOT [SoDienThoai] like N'%[^0-9+]%'))
GO
ALTER TABLE [dbo].[NGUOIDUNG] CHECK CONSTRAINT [CK_NGUOIDUNG_SoDienThoai]
GO
ALTER TABLE [dbo].[NGUOIDUNG]  WITH NOCHECK ADD  CONSTRAINT [CK_NGUOIDUNG_TrangThai] CHECK  (([TrangThai]='Locked' OR [TrangThai]='Inactive' OR [TrangThai]='Active'))
GO
ALTER TABLE [dbo].[NGUOIDUNG] CHECK CONSTRAINT [CK_NGUOIDUNG_TrangThai]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [CK_PHUTUNG_TUONGTHICH_DongXeCanHangXe] CHECK  (([MaDongXe] IS NULL OR [MaHangXe] IS NOT NULL))
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [CK_PHUTUNG_TUONGTHICH_DongXeCanHangXe]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [CK_PHUTUNG_TUONGTHICH_Nam] CHECK  ((([NamTu] IS NULL OR [NamTu]>=(1900) AND [NamTu]<=(2100)) AND ([NamDen] IS NULL OR [NamDen]>=(1900) AND [NamDen]<=(2100)) AND ([NamTu] IS NULL OR [NamDen] IS NULL OR [NamTu]<=[NamDen])))
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [CK_PHUTUNG_TUONGTHICH_Nam]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH]  WITH NOCHECK ADD  CONSTRAINT [CK_PHUTUNG_TUONGTHICH_PhamVi] CHECK  (([ApDungTatCaXe]=(1) OR [MaHangXe] IS NOT NULL OR [MaDongXe] IS NOT NULL))
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] CHECK CONSTRAINT [CK_PHUTUNG_TUONGTHICH_PhamVi]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [CK_THANHTOAN_LoaiThanhToan] CHECK  (([LoaiThanhToan]='Remaining' OR [LoaiThanhToan]='Deposit' OR [LoaiThanhToan]='Full'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_LoaiThanhToan]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [CK_THANHTOAN_PhuongThuc] CHECK  (([PhuongThuc]='VNPay' OR [PhuongThuc]='Momo' OR [PhuongThuc]='Card' OR [PhuongThuc]='BankTransfer' OR [PhuongThuc]='COD'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_PhuongThuc]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [CK_THANHTOAN_SoTien] CHECK  (([SoTien]>(0)))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_SoTien]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [CK_THANHTOAN_TrangThai] CHECK  (([TrangThai]='Cancelled' OR [TrangThai]='Failed' OR [TrangThai]='Paid' OR [TrangThai]='Pending'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_TrangThai]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [CK_TONKHO_GIUCHO_SoLuong] CHECK  (([SoLuong]>(0)))
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [CK_TONKHO_GIUCHO_SoLuong]
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO]  WITH NOCHECK ADD  CONSTRAINT [CK_TONKHO_GIUCHO_TrangThai] CHECK  (([TrangThai]='Released' OR [TrangThai]='Cancelled' OR [TrangThai]='Expired' OR [TrangThai]='Confirmed' OR [TrangThai]='Active'))
GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] CHECK CONSTRAINT [CK_TONKHO_GIUCHO_TrangThai]
GO
ALTER TABLE [dbo].[VAITRO]  WITH NOCHECK ADD  CONSTRAINT [CK_VAITRO_TenVaiTro] CHECK  (([TenVaiTro]='Staff' OR [TenVaiTro]='Admin' OR [TenVaiTro]='Customer'))
GO
ALTER TABLE [dbo].[VAITRO] CHECK CONSTRAINT [CK_VAITRO_TenVaiTro]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_GiaTri] CHECK  (([GiaTriGiam]>=(0) AND ([LoaiGiamGia]<>'Percent' OR [GiaTriGiam]>=(0) AND [GiaTriGiam]<=(100))))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_GiaTri]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_Loai] CHECK  (([LoaiGiamGia]='FreeShipping' OR [LoaiGiamGia]='Percent' OR [LoaiGiamGia]='Amount'))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_Loai]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_PhamViApDung] CHECK  (([PhamViApDung]='Brand' OR [PhamViApDung]='Product' OR [PhamViApDung]='Category' OR [PhamViApDung]='All'))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_PhamViApDung]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_SoLanToiDaMoiNguoiDung] CHECK  (([SoLanToiDaMoiNguoiDung] IS NULL OR [SoLanToiDaMoiNguoiDung]>(0)))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_SoLanToiDaMoiNguoiDung]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_Time] CHECK  (([NgayKetThuc]>[NgayBatDau]))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_Time]
GO
ALTER TABLE [dbo].[VOUCHER]  WITH NOCHECK ADD  CONSTRAINT [CK_VOUCHER_Usage] CHECK  (([SoLanDaDung]>=(0) AND ([GioiHanSuDung] IS NULL OR [GioiHanSuDung]>=(0))))
GO
ALTER TABLE [dbo].[VOUCHER] CHECK CONSTRAINT [CK_VOUCHER_Usage]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG]  WITH CHECK ADD  CONSTRAINT [CK_VOUCHER_NGUOIDUNG_SoTienGiam] CHECK  (([SoTienGiam]>=(0)))
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] CHECK CONSTRAINT [CK_VOUCHER_NGUOIDUNG_SoTienGiam]
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG]  WITH CHECK ADD  CONSTRAINT [CK_VOUCHER_NGUOIDUNG_TrangThai] CHECK  (([TrangThai]='Expired' OR [TrangThai]='Cancelled' OR [TrangThai]='Used' OR [TrangThai]='Saved'))
GO
ALTER TABLE [dbo].[VOUCHER_NGUOIDUNG] CHECK CONSTRAINT [CK_VOUCHER_NGUOIDUNG_TrangThai]
GO
/****** Object:  StoredProcedure [dbo].[sp_DonHang_BatDauCheckout]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

----------------------------------------------------------------------
-- 5) Bắt đầu checkout: kiểm tra tồn khả dụng và tạo giữ chỗ tạm
--    Backend nên gọi procedure này ngay sau khi tạo DONHANG + CHITIET_DONHANG.
----------------------------------------------------------------------
CREATE   PROCEDURE [dbo].[sp_DonHang_BatDauCheckout]
    @MaDonHang INT,
    @SoPhutGiuCho INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoPhutGiuCho IS NULL OR @SoPhutGiuCho <= 0 SET @SoPhutGiuCho = 15;

    DECLARE @HetHanLuc DATETIME2(0) = DATEADD(MINUTE, @SoPhutGiuCho, SYSDATETIME());

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC dbo.sp_TonKho_DonGiuChoHetHan;

        IF NOT EXISTS (SELECT 1 FROM dbo.DONHANG WITH (UPDLOCK, HOLDLOCK) WHERE MaDonHang = @MaDonHang)
            THROW 51000, N'Không tìm thấy đơn hàng.', 1;

        IF EXISTS (SELECT 1 FROM dbo.TONKHO_GIUCHO WHERE MaDonHang = @MaDonHang AND TrangThai = 'Active' AND HetHanLuc > SYSDATETIME())
            THROW 51001, N'Đơn hàng này đang có giữ chỗ tồn kho còn hiệu lực.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.CHITIET_DONHANG WHERE MaDonHang = @MaDonHang)
            THROW 51002, N'Đơn hàng chưa có chi tiết sản phẩm.', 1;

        -- Khóa các dòng tồn kho liên quan để tránh 2 khách checkout vượt tồn cùng lúc.
        SELECT bt.MaBienSanPham
        FROM dbo.CHITIET_DONHANG ct
        INNER JOIN dbo.BIENSANPHAM bt WITH (UPDLOCK, HOLDLOCK)
            ON bt.MaBienSanPham = ct.MaBienSanPham
        WHERE ct.MaDonHang = @MaDonHang
          AND ct.MaBienSanPham IS NOT NULL;

        SELECT sp.MaSanPham
        FROM dbo.CHITIET_DONHANG ct
        INNER JOIN dbo.SANPHAM sp WITH (UPDLOCK, HOLDLOCK)
            ON sp.MaSanPham = ct.MaSanPham
        WHERE ct.MaDonHang = @MaDonHang;

        -- Kiểm tra biến thể có thuộc đúng sản phẩm không.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.CHITIET_DONHANG ct
            INNER JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = ct.MaBienSanPham
            WHERE ct.MaDonHang = @MaDonHang
              AND ct.MaBienSanPham IS NOT NULL
              AND bt.MaSanPham <> ct.MaSanPham
        )
            THROW 51003, N'Biến thể không thuộc đúng sản phẩm trong chi tiết đơn hàng.', 1;

        -- Kiểm tra tồn kho khả dụng.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.CHITIET_DONHANG ct
            INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = ct.MaSanPham
            LEFT JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = ct.MaBienSanPham
            CROSS APPLY
            (
                SELECT
                    CASE
                        WHEN ct.MaBienSanPham IS NOT NULL THEN ISNULL(bt.SoLuongTon, 0)
                        ELSE sp.SoLuongTon
                    END AS TonKhoThucTe,
                    ISNULL((
                        SELECT SUM(g.SoLuong)
                        FROM dbo.TONKHO_GIUCHO g
                        WHERE g.MaSanPham = ct.MaSanPham
                          AND ((ct.MaBienSanPham IS NULL AND g.MaBienSanPham IS NULL) OR g.MaBienSanPham = ct.MaBienSanPham)
                          AND g.TrangThai = 'Active'
                          AND g.HetHanLuc > SYSDATETIME()
                    ), 0) AS DangGiu
            ) tk
            WHERE ct.MaDonHang = @MaDonHang
              AND ct.SoLuong > (tk.TonKhoThucTe - tk.DangGiu)
        )
            THROW 51004, N'Số lượng tồn kho khả dụng không đủ để checkout.', 1;

        INSERT INTO dbo.TONKHO_GIUCHO
        (
            MaDonHang,
            MaChiTietDonHang,
            MaSanPham,
            MaBienSanPham,
            SoLuong,
            TrangThai,
            HetHanLuc,
            NgayTao,
            NgayCapNhat,
            GhiChu
        )
        SELECT
            ct.MaDonHang,
            ct.MaChiTietDonHang,
            ct.MaSanPham,
            ct.MaBienSanPham,
            ct.SoLuong,
            'Active',
            @HetHanLuc,
            SYSDATETIME(),
            SYSDATETIME(),
            N'Giữ tồn kho khi khách bắt đầu checkout'
        FROM dbo.CHITIET_DONHANG ct
        WHERE ct.MaDonHang = @MaDonHang;

        UPDATE dbo.DONHANG
        SET TrangThaiDonHang = 'AwaitingPayment',
            CheckoutHetHanLuc = @HetHanLuc,
            NgayCapNhat = SYSDATETIME()
        WHERE MaDonHang = @MaDonHang;

        COMMIT TRANSACTION;

        SELECT
            @MaDonHang AS MaDonHang,
            @HetHanLuc AS CheckoutHetHanLuc,
            N'Đã giữ tồn kho tạm thời cho đơn hàng.' AS ThongBao;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_DonHang_HuyVaNhaGiuCho]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

----------------------------------------------------------------------
-- 7) Hủy đơn khi chưa thanh toán: nhả giữ chỗ, không trừ tồn kho
----------------------------------------------------------------------
CREATE   PROCEDURE [dbo].[sp_DonHang_HuyVaNhaGiuCho]
    @MaDonHang INT,
    @LyDoHuyDon NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TONKHO_GIUCHO
        SET TrangThai = 'Cancelled',
            NgayCapNhat = SYSDATETIME(),
            GhiChu = ISNULL(GhiChu + N' | ', N'') + N'Hủy đơn, nhả giữ chỗ'
        WHERE MaDonHang = @MaDonHang
          AND TrangThai = 'Active';

        UPDATE dbo.DONHANG
        SET TrangThaiDonHang = 'Cancelled',
            NgayHuyDon = SYSDATETIME(),
            LyDoHuyDon = ISNULL(@LyDoHuyDon, N'Khách hủy hoặc thanh toán thất bại'),
            NgayCapNhat = SYSDATETIME()
        WHERE MaDonHang = @MaDonHang
          AND TrangThaiDonHang <> 'Confirmed';

        COMMIT TRANSACTION;

        SELECT @MaDonHang AS MaDonHang, N'Đã hủy đơn và nhả giữ chỗ tồn kho.' AS ThongBao;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_PhuTung_LayTheoXe]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 7) Procedure loc phu tung phu hop voi xe */
CREATE   PROCEDURE [dbo].[sp_PhuTung_LayTheoXe]
    @MaHangXe INT = NULL,
    @MaDongXe INT = NULL,
    @NamSanXuat SMALLINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        v.MaPhuTung,
        v.TenPhuTung,
        v.SlugPhuTung,
        v.AnhChinhUrl,
        v.GiaGoc,
        v.GiaKhuyenMai,
        v.SoLuongTon,
        v.MaHangXe,
        v.TenHang,
        v.MaDongXe,
        v.TenDongXe,
        v.NamTu,
        v.NamDen,
        v.ApDungTatCaXe,
        v.GhiChu
    FROM dbo.v_PHUTUNG_TUONGTHICH v
    WHERE v.DangHoatDong = 1
      AND v.PhuTungDangHoatDong = 1
      AND (
            v.ApDungTatCaXe = 1
            OR (@MaDongXe IS NOT NULL AND v.MaDongXe = @MaDongXe)
            OR (@MaHangXe IS NOT NULL AND v.MaHangXe = @MaHangXe AND v.MaDongXe IS NULL)
          )
      AND (@NamSanXuat IS NULL OR v.NamTu IS NULL OR v.NamTu <= @NamSanXuat)
      AND (@NamSanXuat IS NULL OR v.NamDen IS NULL OR v.NamDen >= @NamSanXuat)
    ORDER BY v.TenPhuTung;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_PhuTung_UpsertTuongThich]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 6) Procedure them/cap nhat tuong thich phu tung */
CREATE   PROCEDURE [dbo].[sp_PhuTung_UpsertTuongThich]
    @MaTuongThich INT = NULL,
    @MaPhuTung INT,
    @MaHangXe INT = NULL,
    @MaDongXe INT = NULL,
    @NamTu SMALLINT = NULL,
    @NamDen SMALLINT = NULL,
    @ApDungTatCaXe BIT = 0,
    @GhiChu NVARCHAR(500) = NULL,
    @DangHoatDong BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @ApDungTatCaXe = 0 AND @MaHangXe IS NULL AND @MaDongXe IS NULL
    BEGIN
        RAISERROR(N'Phai chon hang xe, dong xe hoac danh dau ApDungTatCaXe = 1.', 16, 1);
        RETURN;
    END;

    IF @MaDongXe IS NOT NULL AND @MaHangXe IS NULL
    BEGIN
        SELECT @MaHangXe = MaHangXe
        FROM dbo.DONGXE
        WHERE MaDongXe = @MaDongXe;
    END;

    IF @MaTuongThich IS NULL
    BEGIN
        INSERT INTO dbo.PHUTUNG_TUONGTHICH
        (
            MaPhuTung, MaHangXe, MaDongXe, NamTu, NamDen,
            ApDungTatCaXe, GhiChu, DangHoatDong, NgayTao, NgayCapNhat
        )
        VALUES
        (
            @MaPhuTung, @MaHangXe, @MaDongXe, @NamTu, @NamDen,
            @ApDungTatCaXe, @GhiChu, @DangHoatDong, SYSUTCDATETIME(), SYSUTCDATETIME()
        );

        SELECT SCOPE_IDENTITY() AS MaTuongThichMoi;
        RETURN;
    END;

    UPDATE dbo.PHUTUNG_TUONGTHICH
    SET MaPhuTung = @MaPhuTung,
        MaHangXe = @MaHangXe,
        MaDongXe = @MaDongXe,
        NamTu = @NamTu,
        NamDen = @NamDen,
        ApDungTatCaXe = @ApDungTatCaXe,
        GhiChu = @GhiChu,
        DangHoatDong = @DangHoatDong,
        NgayCapNhat = SYSUTCDATETIME()
    WHERE MaTuongThich = @MaTuongThich;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_SANPHAM_DongBoSoLuongTon]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--------------------------------------------------------------------------------
-- 3) Chuẩn hóa tồn kho: BIENSANPHAM là nguồn chính, SANPHAM là tổng đồng bộ
--------------------------------------------------------------------------------

/*
    Procedure đồng bộ tồn kho của 1 sản phẩm:
    - Nếu sản phẩm có biến thể: SANPHAM.SoLuongTon = SUM(BIENSANPHAM.SoLuongTon)
    - Nếu sản phẩm không có biến thể: giữ nguyên SANPHAM.SoLuongTon
*/
CREATE   PROCEDURE [dbo].[sp_SANPHAM_DongBoSoLuongTon]
    @MaSanPham INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.BIENSANPHAM WHERE MaSanPham = @MaSanPham)
    BEGIN
        UPDATE sp
        SET
            SoLuongTon = ISNULL(x.TongTon, 0),
            NgayCapNhat = SYSUTCDATETIME()
        FROM dbo.SANPHAM sp
        OUTER APPLY
        (
            SELECT SUM(ISNULL(bsp.SoLuongTon, 0)) AS TongTon
            FROM dbo.BIENSANPHAM bsp
            WHERE bsp.MaSanPham = sp.MaSanPham
        ) x
        WHERE sp.MaSanPham = @MaSanPham;
    END
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_SANPHAM_DongBoTatCaSoLuongTon]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*
    Procedure đồng bộ tất cả sản phẩm có biến thể.
*/
CREATE   PROCEDURE [dbo].[sp_SANPHAM_DongBoTatCaSoLuongTon]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE sp
    SET
        SoLuongTon = ISNULL(x.TongTon, 0),
        NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.SANPHAM sp
    INNER JOIN
    (
        SELECT
            MaSanPham,
            SUM(ISNULL(SoLuongTon, 0)) AS TongTon
        FROM dbo.BIENSANPHAM
        GROUP BY MaSanPham
    ) x
        ON x.MaSanPham = sp.MaSanPham;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_TonKho_DonGiuChoHetHan]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

----------------------------------------------------------------------
-- 4) Dọn giữ chỗ hết hạn
----------------------------------------------------------------------
CREATE   PROCEDURE [dbo].[sp_TonKho_DonGiuChoHetHan]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TONKHO_GIUCHO
    SET TrangThai = 'Expired',
        NgayCapNhat = SYSDATETIME(),
        GhiChu = ISNULL(GhiChu + N' | ', N'') + N'Tự động hết hạn giữ chỗ'
    WHERE TrangThai = 'Active'
      AND HetHanLuc <= SYSDATETIME();

    UPDATE dh
    SET TrangThaiDonHang = 'Cancelled',
        NgayHuyDon = ISNULL(dh.NgayHuyDon, SYSDATETIME()),
        LyDoHuyDon = ISNULL(dh.LyDoHuyDon, N'Hết thời gian thanh toán'),
        NgayCapNhat = SYSDATETIME()
    FROM dbo.DONHANG dh
    WHERE dh.TrangThaiDonHang IN ('Pending', 'Checkout', 'AwaitingPayment')
      AND dh.CheckoutHetHanLuc IS NOT NULL
      AND dh.CheckoutHetHanLuc <= SYSDATETIME()
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.TONKHO_GIUCHO g
          WHERE g.MaDonHang = dh.MaDonHang
            AND g.TrangThai = 'Active'
            AND g.HetHanLuc > SYSDATETIME()
      );
END

GO
/****** Object:  StoredProcedure [dbo].[sp_Voucher_GhiNhanSuDung]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 10. Ghi nhan voucher sau khi don hang tao thanh cong */
CREATE   PROCEDURE [dbo].[sp_Voucher_GhiNhanSuDung]
    @MaNguoiDung INT,
    @MaDonHang INT,
    @MaVoucherCode NVARCHAR(50),
    @SoTienGiam DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @MaVoucher INT, @LoaiGiamGia VARCHAR(20), @GiaTriGiam DECIMAL(18,2), @CurrentCount INT, @Limit INT;

        SELECT
            @MaVoucher = MaVoucher,
            @LoaiGiamGia = LoaiGiamGia,
            @GiaTriGiam = GiaTriGiam,
            @CurrentCount = SoLanDaDung,
            @Limit = GioiHanSuDung
        FROM dbo.VOUCHER WITH (UPDLOCK, HOLDLOCK)
        WHERE MaVoucherCode = @MaVoucherCode;

        IF @MaVoucher IS NULL
            THROW 51001, N'Ma voucher khong ton tai.', 1;

        IF @Limit IS NOT NULL AND @CurrentCount >= @Limit
            THROW 51002, N'Voucher da het luot su dung.', 1;

        INSERT INTO dbo.VOUCHER_NGUOIDUNG (
            MaVoucher, MaNguoiDung, MaDonHang, MaVoucherCodeSnapshot,
            LoaiGiamGiaSnapshot, GiaTriGiamSnapshot, SoTienGiam, TrangThai, NgaySuDung
        )
        VALUES (
            @MaVoucher, @MaNguoiDung, @MaDonHang, @MaVoucherCode,
            @LoaiGiamGia, @GiaTriGiam, @SoTienGiam, 'Used', SYSDATETIME()
        );

        IF NOT EXISTS (SELECT 1 FROM dbo.DONHANG_VOUCHER WHERE MaDonHang = @MaDonHang AND MaVoucher = @MaVoucher)
        BEGIN
            INSERT INTO dbo.DONHANG_VOUCHER (
                MaDonHang, MaVoucher, MaVoucherCodeSnapshot, SoTienGiam, NgayTao,
                LoaiGiamGiaSnapshot, GiaTriGiamSnapshot
            )
            VALUES (
                @MaDonHang, @MaVoucher, @MaVoucherCode, @SoTienGiam, SYSDATETIME(),
                @LoaiGiamGia, @GiaTriGiam
            );
        END

        UPDATE dbo.VOUCHER
        SET SoLanDaDung = SoLanDaDung + 1,
            NgayCapNhat = SYSDATETIME()
        WHERE MaVoucher = @MaVoucher;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_Voucher_HuySuDungTheoDon]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 11. Huy luot dung voucher neu don hang bi huy */
CREATE   PROCEDURE [dbo].[sp_Voucher_HuySuDungTheoDon]
    @MaDonHang INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        UPDATE dbo.VOUCHER_NGUOIDUNG
        SET TrangThai = 'Cancelled'
        WHERE MaDonHang = @MaDonHang AND TrangThai = 'Used';

        UPDATE v
        SET SoLanDaDung = CASE WHEN v.SoLanDaDung > x.SoLuongHuy THEN v.SoLanDaDung - x.SoLuongHuy ELSE 0 END,
            NgayCapNhat = SYSDATETIME()
        FROM dbo.VOUCHER v
        JOIN (
            SELECT MaVoucher, COUNT(*) AS SoLuongHuy
            FROM dbo.VOUCHER_NGUOIDUNG
            WHERE MaDonHang = @MaDonHang AND TrangThai = 'Cancelled'
            GROUP BY MaVoucher
        ) x ON x.MaVoucher = v.MaVoucher;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_Voucher_KiemTraTruocKhiTaoDon]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 9. Procedure kiem tra voucher truoc khi tao don */
CREATE   PROCEDURE [dbo].[sp_Voucher_KiemTraTruocKhiTaoDon]
    @MaNguoiDung INT,
    @MaGioHang INT,
    @MaVoucherCode NVARCHAR(50),
    @PhiVanChuyen DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @MaVoucher INT,
        @LoaiGiamGia VARCHAR(20),
        @GiaTriGiam DECIMAL(18,2),
        @GiaTriDonToiThieu DECIMAL(18,2),
        @GiaTriGiamToiDa DECIMAL(18,2),
        @NgayBatDau DATETIME2(0),
        @NgayKetThuc DATETIME2(0),
        @GioiHanSuDung INT,
        @SoLanDaDung INT,
        @SoLanToiDaMoiNguoiDung INT,
        @PhamViApDung VARCHAR(20),
        @DangHoatDong BIT,
        @TongTienHang DECIMAL(18,2),
        @TongTienHopLe DECIMAL(18,2),
        @SoTienGiam DECIMAL(18,2) = 0,
        @LyDo NVARCHAR(255) = NULL;

    SELECT
        @MaVoucher = MaVoucher,
        @LoaiGiamGia = LoaiGiamGia,
        @GiaTriGiam = GiaTriGiam,
        @GiaTriDonToiThieu = GiaTriDonToiThieu,
        @GiaTriGiamToiDa = GiaTriGiamToiDa,
        @NgayBatDau = NgayBatDau,
        @NgayKetThuc = NgayKetThuc,
        @GioiHanSuDung = GioiHanSuDung,
        @SoLanDaDung = SoLanDaDung,
        @SoLanToiDaMoiNguoiDung = SoLanToiDaMoiNguoiDung,
        @PhamViApDung = PhamViApDung,
        @DangHoatDong = DangHoatDong
    FROM dbo.VOUCHER
    WHERE MaVoucherCode = @MaVoucherCode;

    IF @MaVoucher IS NULL
        SET @LyDo = N'Ma voucher khong ton tai';
    ELSE IF @DangHoatDong = 0
        SET @LyDo = N'Voucher da tat';
    ELSE IF SYSDATETIME() < @NgayBatDau OR SYSDATETIME() > @NgayKetThuc
        SET @LyDo = N'Voucher khong nam trong thoi gian ap dung';
    ELSE IF @GioiHanSuDung IS NOT NULL AND @SoLanDaDung >= @GioiHanSuDung
        SET @LyDo = N'Voucher da het luot su dung';
    ELSE IF (SELECT COUNT(*) FROM dbo.VOUCHER_NGUOIDUNG WHERE MaNguoiDung = @MaNguoiDung AND MaVoucher = @MaVoucher AND TrangThai = 'Used') >= ISNULL(@SoLanToiDaMoiNguoiDung, 1)
        SET @LyDo = N'Nguoi dung da dung voucher qua so lan cho phep';

    SELECT @TongTienHang = ISNULL(SUM(ThanhTien), 0)
    FROM dbo.CHITIET_GIOHANG
    WHERE MaGioHang = @MaGioHang;

    IF @LyDo IS NULL AND @TongTienHang <= 0
        SET @LyDo = N'Gio hang trong hoac khong hop le';

    IF @LyDo IS NULL AND @TongTienHang < @GiaTriDonToiThieu
        SET @LyDo = N'Gia tri don hang chua dat muc toi thieu';

    IF @LyDo IS NULL
    BEGIN
        SELECT @TongTienHopLe = ISNULL(SUM(ct.ThanhTien), 0)
        FROM dbo.CHITIET_GIOHANG ct
        JOIN dbo.SANPHAM sp ON sp.MaSanPham = ct.MaSanPham
        WHERE ct.MaGioHang = @MaGioHang
          AND (
                @PhamViApDung = 'All'
                OR (@PhamViApDung = 'Category' AND EXISTS (
                    SELECT 1 FROM dbo.VOUCHER_DANHMUC x
                    WHERE x.MaVoucher = @MaVoucher AND x.MaDanhMuc = sp.MaDanhMuc
                ))
                OR (@PhamViApDung = 'Product' AND EXISTS (
                    SELECT 1 FROM dbo.VOUCHER_SANPHAM x
                    WHERE x.MaVoucher = @MaVoucher AND x.MaSanPham = sp.MaSanPham
                ))
                OR (@PhamViApDung = 'Brand' AND EXISTS (
                    SELECT 1 FROM dbo.VOUCHER_HANGXE x
                    WHERE x.MaVoucher = @MaVoucher AND x.MaHangXe = sp.MaHangXe
                ))
          );

        IF @TongTienHopLe <= 0
            SET @LyDo = N'Voucher khong ap dung cho san pham trong gio hang';
    END

    IF @LyDo IS NULL
    BEGIN
        IF @LoaiGiamGia = 'Amount'
            SET @SoTienGiam = IIF(@GiaTriGiam > @TongTienHopLe, @TongTienHopLe, @GiaTriGiam);
        ELSE IF @LoaiGiamGia = 'Percent'
        BEGIN
            SET @SoTienGiam = ROUND(@TongTienHopLe * @GiaTriGiam / 100, 0);
            IF @GiaTriGiamToiDa IS NOT NULL AND @SoTienGiam > @GiaTriGiamToiDa
                SET @SoTienGiam = @GiaTriGiamToiDa;
        END
        ELSE IF @LoaiGiamGia = 'FreeShipping'
        BEGIN
            SET @SoTienGiam = ISNULL(@PhiVanChuyen, 0);
            IF @GiaTriGiamToiDa IS NOT NULL AND @SoTienGiam > @GiaTriGiamToiDa
                SET @SoTienGiam = @GiaTriGiamToiDa;
        END
    END

    SELECT
        IIF(@LyDo IS NULL, CAST(1 AS BIT), CAST(0 AS BIT)) AS HopLe,
        @LyDo AS LyDoKhongHopLe,
        @MaVoucher AS MaVoucher,
        @MaVoucherCode AS MaVoucherCode,
        @LoaiGiamGia AS LoaiGiamGia,
        @PhamViApDung AS PhamViApDung,
        ISNULL(@TongTienHang, 0) AS TongTienHang,
        ISNULL(@TongTienHopLe, 0) AS TongTienHopLe,
        ISNULL(@SoTienGiam, 0) AS SoTienGiam;
END

GO
/****** Object:  Trigger [dbo].[trg_ANHSANPHAM_Validate_MaBienSanPham]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Trigger đảm bảo ảnh gắn MaBienSanPham đúng với MaSanPham của sản phẩm.
CREATE   TRIGGER [dbo].[trg_ANHSANPHAM_Validate_MaBienSanPham]
ON [dbo].[ANHSANPHAM]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = i.MaBienSanPham
        WHERE i.MaBienSanPham IS NOT NULL
          AND bt.MaSanPham <> i.MaSanPham
    )
    BEGIN
        THROW 53001, N'MaBienSanPham của ảnh không thuộc đúng MaSanPham.', 1;
    END
END

GO
ALTER TABLE [dbo].[ANHSANPHAM] ENABLE TRIGGER [trg_ANHSANPHAM_Validate_MaBienSanPham]
GO
/****** Object:  Trigger [dbo].[trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM]
ON [dbo].[BIENSANPHAM]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ChangedProducts AS
    (
        SELECT MaSanPham FROM inserted
        UNION
        SELECT MaSanPham FROM deleted
    )
    UPDATE sp
    SET
        SoLuongTon = ISNULL(x.TongTon, 0),
        NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.SANPHAM sp
    INNER JOIN ChangedProducts cp
        ON cp.MaSanPham = sp.MaSanPham
    OUTER APPLY
    (
        SELECT SUM(ISNULL(bsp.SoLuongTon, 0)) AS TongTon
        FROM dbo.BIENSANPHAM bsp
        WHERE bsp.MaSanPham = sp.MaSanPham
    ) x
    WHERE EXISTS
    (
        SELECT 1
        FROM dbo.BIENSANPHAM bsp2
        WHERE bsp2.MaSanPham = sp.MaSanPham
    );
END;

GO
ALTER TABLE [dbo].[BIENSANPHAM] ENABLE TRIGGER [trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM]
GO
/****** Object:  Trigger [dbo].[trg_CHITIET_DONHANG_Validate_MaBienSanPham]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_CHITIET_DONHANG_Validate_MaBienSanPham]
ON [dbo].[CHITIET_DONHANG]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.BIENSANPHAM bsp
            ON bsp.MaBienSanPham = i.MaBienSanPham
        WHERE i.MaBienSanPham IS NOT NULL
          AND bsp.MaSanPham <> i.MaSanPham
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: CHITIET_DONHANG.MaBienSanPham không thuộc đúng CHITIET_DONHANG.MaSanPham.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;

GO
ALTER TABLE [dbo].[CHITIET_DONHANG] ENABLE TRIGGER [trg_CHITIET_DONHANG_Validate_MaBienSanPham]
GO
/****** Object:  Trigger [dbo].[trg_CHITIET_GIOHANG_Validate_MaBienSanPham]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_CHITIET_GIOHANG_Validate_MaBienSanPham]
ON [dbo].[CHITIET_GIOHANG]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.BIENSANPHAM bsp
            ON bsp.MaBienSanPham = i.MaBienSanPham
        WHERE i.MaBienSanPham IS NOT NULL
          AND bsp.MaSanPham <> i.MaSanPham
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: CHITIET_GIOHANG.MaBienSanPham không thuộc đúng CHITIET_GIOHANG.MaSanPham.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;

GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] ENABLE TRIGGER [trg_CHITIET_GIOHANG_Validate_MaBienSanPham]
GO
/****** Object:  Trigger [dbo].[trg_PHUTUNG_TUONGTHICH_Validate]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_PHUTUNG_TUONGTHICH_Validate]
ON [dbo].[PHUTUNG_TUONGTHICH]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    /*
        2.1. Chặn MaPhuTung trỏ vào sản phẩm không phải phụ tùng/phụ kiện.

        Các giá trị được coi là phụ tùng/phụ kiện:
        - 'Part'
        - 'Accessory'
        - 'SparePart'
        - 'PhuTung'
        - 'PhuKien'
        - 'PhuTungXeMay'
        - 'PhụTùng'
        - 'PhụKiện'

        Nếu database của bạn đang dùng giá trị khác cho LoaiSanPham,
        hãy bổ sung giá trị đó vào danh sách bên dưới.
    */
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.SANPHAM sp
            ON sp.MaSanPham = i.MaPhuTung
        WHERE sp.LoaiSanPham NOT IN
        (
            'Part',
            'Accessory',
            'SparePart',
            'PhuTung',
            'PhuKien',
            'PhuTungXeMay',
            N'PhụTùng',
            N'PhụKiện'
        )
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: PHUTUNG_TUONGTHICH.MaPhuTung phải trỏ tới sản phẩm loại phụ tùng/phụ kiện, không được trỏ tới xe máy.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    /*
        2.2. Chặn lỗi:
        PHUTUNG_TUONGTHICH.MaHangXe = Honda
        PHUTUNG_TUONGTHICH.MaDongXe = Exciter, trong khi Exciter thuộc Yamaha
    */
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.DONGXE dx
            ON dx.MaDongXe = i.MaDongXe
        WHERE i.MaDongXe IS NOT NULL
          AND i.MaHangXe IS NOT NULL
          AND dx.MaHangXe <> i.MaHangXe
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: PHUTUNG_TUONGTHICH.MaDongXe không thuộc đúng PHUTUNG_TUONGTHICH.MaHangXe.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    /*
        2.3. Giữ lại logic cũ: cập nhật NgayCapNhat khi thêm/sửa tương thích.
    */
    UPDATE ptt
    SET NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.PHUTUNG_TUONGTHICH ptt
    INNER JOIN inserted i
        ON i.MaTuongThich = ptt.MaTuongThich;
END;

GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] ENABLE TRIGGER [trg_PHUTUNG_TUONGTHICH_Validate]
GO
/****** Object:  Trigger [dbo].[trg_SANPHAM_Validate_HangXe_DongXe]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_SANPHAM_Validate_HangXe_DongXe]
ON [dbo].[SANPHAM]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    /*
        Chặn lỗi:
        SANPHAM.MaHangXe = Honda
        SANPHAM.MaDongXe = Exciter, trong khi Exciter thuộc Yamaha
    */
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.DONGXE dx
            ON dx.MaDongXe = i.MaDongXe
        WHERE i.MaDongXe IS NOT NULL
          AND i.MaHangXe IS NOT NULL
          AND dx.MaHangXe <> i.MaHangXe
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: SANPHAM.MaDongXe không thuộc đúng SANPHAM.MaHangXe.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;

GO
ALTER TABLE [dbo].[SANPHAM] ENABLE TRIGGER [trg_SANPHAM_Validate_HangXe_DongXe]
GO
/****** Object:  Trigger [dbo].[trg_TONKHO_GIUCHO_Validate_MaBienSanPham]    Script Date: 5/20/2026 1:01:25 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_TONKHO_GIUCHO_Validate_MaBienSanPham]
ON [dbo].[TONKHO_GIUCHO]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.BIENSANPHAM bsp
            ON bsp.MaBienSanPham = i.MaBienSanPham
        WHERE i.MaBienSanPham IS NOT NULL
          AND bsp.MaSanPham <> i.MaSanPham
    )
    BEGIN
        RAISERROR (
            N'Lỗi dữ liệu: TONKHO_GIUCHO.MaBienSanPham không thuộc đúng TONKHO_GIUCHO.MaSanPham.',
            16,
            1
        );
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;

GO
ALTER TABLE [dbo].[TONKHO_GIUCHO] ENABLE TRIGGER [trg_TONKHO_GIUCHO_Validate_MaBienSanPham]
GO

-- Phan loai xe cho cac dong xe seed (DONGXE.LoaiXe) - bo sung theo audit nghiep vu.
-- Co guard COL_LENGTH de an toan neu chay tren schema chua co cot LoaiXe.
IF COL_LENGTH('dbo.DONGXE', 'LoaiXe') IS NOT NULL
BEGIN
    UPDATE dbo.DONGXE SET LoaiXe = 'TayGa'  WHERE MaDongXe IN (1, 2, 5, 6);   -- Air Blade, LEAD, FreeGo, Latte
    UPDATE dbo.DONGXE SET LoaiXe = 'XeSo'   WHERE MaDongXe IN (3);            -- Wave Alpha
    UPDATE dbo.DONGXE SET LoaiXe = 'ConTay' WHERE MaDongXe IN (4);            -- Exciter
    UPDATE dbo.DONGXE SET LoaiXe = 'XeDien' WHERE MaDongXe IN (7, 8, 9, 10);  -- VinFast Evo200/Klara/Vento/Theon
END
GO




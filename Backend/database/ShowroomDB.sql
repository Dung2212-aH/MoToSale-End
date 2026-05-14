USE [ShowroomDB]
GO
/****** Object:  Table [dbo].[BIENSANPHAM]    Script Date: 5/7/2026 4:04:05 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_BIENSANPHAM_SKU] UNIQUE NONCLUSTERED 
(
	[SKU] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ANHSANPHAM]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[SANPHAM]    Script Date: 5/7/2026 4:04:06 PM ******/
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
	[MaShowroom] [int] NULL,
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
 CONSTRAINT [PK_SANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_SANPHAM_Code] UNIQUE NONCLUSTERED 
(
	[MaSanPhamKinhDoanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_SANPHAM_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_SANPHAM_BIENTHE_ANH]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[PHUTUNG_TUONGTHICH]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[HANGXE]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HANGXE_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HANGXE_TenHang] UNIQUE NONCLUSTERED 
(
	[TenHang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DONGXE]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DONGXE_Hang_Ten] UNIQUE NONCLUSTERED 
(
	[MaHangXe] ASC,
	[TenDongXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DONGXE_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_PHUTUNG_TUONGTHICH]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  View [dbo].[v_SANPHAM_TONKHO_KIEMTRA]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  View [dbo].[v_ANHSANPHAM_THEO_BIENTHE]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[TONKHO_GIUCHO]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  View [dbo].[v_TONKHO_KHADUNG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[DONHANG]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DONHANG](
	[MaDonHang] [int] IDENTITY(1,1) NOT NULL,
	[MaDonHangKinhDoanh] [nvarchar](50) NOT NULL,
	[MaNguoiDung] [int] NOT NULL,
	[MaShowroom] [int] NULL,
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DONHANG_Code] UNIQUE NONCLUSTERED 
(
	[MaDonHangKinhDoanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_TRA_GOP_TOMTAT]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 6) Optional view for admin/customer installment summary */
CREATE   VIEW [dbo].[v_TRA_GOP_TOMTAT]
AS
SELECT
    tg.MaTraGop,
    tg.MaDonHang,
    dh.MaDonHangKinhDoanh,
    dh.MaNguoiDung,
    dh.HoTenNhanHang,
    dh.SoDienThoaiNhanHang,
    dh.TongThanhToan,
    tg.SoTienTraTruoc,
    tg.SoTienTraGop,
    tg.SoThang,
    tg.LaiSuatThang,
    tg.SoTienMoiThang,
    tg.SoKyDaTra,
    SoKyConLai = tg.SoThang - tg.SoKyDaTra,
    SoTienUocTinhDaTra = CONVERT(DECIMAL(18,2), tg.SoKyDaTra * tg.SoTienMoiThang),
    SoTienUocTinhConLai = CONVERT(DECIMAL(18,2), (tg.SoThang - tg.SoKyDaTra) * tg.SoTienMoiThang),
    tg.TrangThai,
    tg.DonViTaiChinh,
    tg.NgayBatDau,
    tg.NgayKetThuc,
    tg.NgayTao,
    tg.NgayCapNhat
FROM dbo.TRA_GOP tg
INNER JOIN dbo.DONHANG dh ON dh.MaDonHang = tg.MaDonHang;
GO
/****** Object:  Table [dbo].[VOUCHER_NGUOIDUNG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  View [dbo].[v_VOUCHER_USER_USAGE]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[THANHTOAN]    Script Date: 5/7/2026 4:04:06 PM ******/
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
	[SoTienHoan] [decimal](18, 2) NOT NULL,
	[NoiDungChuyenKhoan] [nvarchar](500) NULL,
	[MaNganHang] [nvarchar](50) NULL,
	[LyDoHuy] [nvarchar](500) NULL,
	[NgayHuy] [datetime2](0) NULL,
	[ResponseRaw] [nvarchar](max) NULL,
 CONSTRAINT [PK_THANHTOAN] PRIMARY KEY CLUSTERED 
(
	[MaThanhToan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_THANHTOAN_Code] UNIQUE NONCLUSTERED 
(
	[MaThanhToanKinhDoanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_THANHTOAN_DONHANG_TONGHOP]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 4) View tổng hợp thanh toán thực thu của đơn hàng */
CREATE   VIEW [dbo].[v_THANHTOAN_DONHANG_TONGHOP]
AS
SELECT
    dh.MaDonHang,
    dh.MaDonHangKinhDoanh,
    dh.TongThanhToan,
    TongDaThanhToan = ISNULL(SUM(CASE WHEN tt.TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN tt.SoTien ELSE 0 END), 0),
    TongDaHoan = ISNULL(SUM(CASE WHEN tt.TrangThai IN ('PartiallyRefunded', 'Refunded') THEN tt.SoTienHoan ELSE 0 END), 0),
    TongThucThu = ISNULL(SUM(CASE WHEN tt.TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN tt.SoTien - tt.SoTienHoan ELSE 0 END), 0),
    SoTienConPhaiThu = CASE
        WHEN dh.TongThanhToan - ISNULL(SUM(CASE WHEN tt.TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN tt.SoTien - tt.SoTienHoan ELSE 0 END), 0) < 0 THEN 0
        ELSE dh.TongThanhToan - ISNULL(SUM(CASE WHEN tt.TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN tt.SoTien - tt.SoTienHoan ELSE 0 END), 0)
    END,
    SoLanThanhToanThanhCong = SUM(CASE WHEN tt.TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN 1 ELSE 0 END),
    SoLanDangCho = SUM(CASE WHEN tt.TrangThai = 'Pending' THEN 1 ELSE 0 END)
FROM dbo.DONHANG dh
LEFT JOIN dbo.THANHTOAN tt ON tt.MaDonHang = dh.MaDonHang
GROUP BY dh.MaDonHang, dh.MaDonHangKinhDoanh, dh.TongThanhToan;

GO
/****** Object:  Table [dbo].[BAIVIET]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_BAIVIET_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CHITIET_DONHANG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[CHITIET_GIOHANG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[DANHGIASANPHAM]    Script Date: 5/7/2026 4:04:06 PM ******/
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
	[HinhAnhUrl] [nvarchar](max) NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_DANHGIASANPHAM] PRIMARY KEY CLUSTERED 
(
	[MaDanhGia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DANHMUC]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DANHMUC_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DONHANG_VOUCHER]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[FAQ]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[GIOHANG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[LIENHE_YEUCAU]    Script Date: 5/7/2026 4:04:06 PM ******/
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
	[MaShowroom] [int] NULL,
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
/****** Object:  Table [dbo].[NGUOIDUNG]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_NGUOIDUNG_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NGUOIDUNG_DIACHI]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[NGUOIDUNG_VAITRO]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[SHOWROOM]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SHOWROOM](
	[MaShowroom] [int] IDENTITY(1,1) NOT NULL,
	[TenShowroom] [nvarchar](180) NOT NULL,
	[Slug] [nvarchar](220) NOT NULL,
	[DiaChi] [nvarchar](255) NOT NULL,
	[SoDienThoai] [nvarchar](20) NULL,
	[Email] [nvarchar](255) NULL,
	[GioMoCua] [nvarchar](255) NULL,
	[DangHoatDong] [bit] NOT NULL,
	[NgayTao] [datetime2](0) NOT NULL,
	[NgayCapNhat] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_SHOWROOM] PRIMARY KEY CLUSTERED 
(
	[MaShowroom] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_SHOWROOM_Slug] UNIQUE NONCLUSTERED 
(
	[Slug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[THANHTOAN_HOANTIEN]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[THANHTOAN_HOANTIEN](
	[MaHoanTien] [int] IDENTITY(1,1) NOT NULL,
	[MaThanhToan] [int] NOT NULL,
	[MaDonHang] [int] NOT NULL,
	[SoTienHoan] [decimal](18, 2) NOT NULL,
	[MaGiaoDichHoanTien] [nvarchar](120) NULL,
	[LyDo] [nvarchar](500) NULL,
	[TrangThai] [varchar](20) NOT NULL,
	[ResponseRaw] [nvarchar](max) NULL,
	[NgayTao] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_THANHTOAN_HOANTIEN] PRIMARY KEY CLUSTERED 
(
	[MaHoanTien] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VAITRO]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_VAITRO_TenVaiTro] UNIQUE NONCLUSTERED 
(
	[TenVaiTro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER]    Script Date: 5/7/2026 4:04:06 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_VOUCHER_Code] UNIQUE NONCLUSTERED 
(
	[MaVoucherCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VOUCHER_DANHMUC]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[VOUCHER_HANGXE]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[VOUCHER_SANPHAM]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Table [dbo].[YEUTHICH]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Index [IX_ANHSANPHAM_Product]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_ANHSANPHAM_Product] ON [dbo].[ANHSANPHAM]
(
	[MaSanPham] ASC,
	[ThuTuHienThi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ANHSANPHAM_OneMainImage_PerVariant]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ANHSANPHAM_OneMainImage_PerVariant] ON [dbo].[ANHSANPHAM]
(
	[MaBienSanPham] ASC
)
WHERE ([MaBienSanPham] IS NOT NULL AND [LaAnhChinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_ANHSANPHAM_Primary]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ANHSANPHAM_Primary] ON [dbo].[ANHSANPHAM]
(
	[MaSanPham] ASC
)
WHERE ([LaAnhChinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BAIVIET_Category]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_BAIVIET_Category] ON [dbo].[BAIVIET]
(
	[DanhMuc] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BAIVIET_Status_Published]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_BAIVIET_Status_Published] ON [dbo].[BAIVIET]
(
	[TrangThai] ASC,
	[XuatBanLuc] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BIENSANPHAM_MaSanPham_MauSac]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_BIENSANPHAM_MaSanPham_MauSac] ON [dbo].[BIENSANPHAM]
(
	[MaSanPham] ASC,
	[MauSac] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_BIENSANPHAM_Product]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_BIENSANPHAM_Product] ON [dbo].[BIENSANPHAM]
(
	[MaSanPham] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CHITIET_DONHANG_Product]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_CHITIET_DONHANG_Product] ON [dbo].[CHITIET_DONHANG]
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_CHITIET_GIOHANG_NoVariant]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CHITIET_GIOHANG_NoVariant] ON [dbo].[CHITIET_GIOHANG]
(
	[MaGioHang] ASC,
	[MaSanPham] ASC
)
WHERE ([MaBienSanPham] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_CHITIET_GIOHANG_WithVariant]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Index [IX_DANHGIA_Product_Status]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DANHGIA_Product_Status] ON [dbo].[DANHGIASANPHAM]
(
	[MaSanPham] ASC,
	[TrangThai] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DANHMUC_Parent]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DANHMUC_Parent] ON [dbo].[DANHMUC]
(
	[MaDanhMucCha] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONGXE_Hang]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONGXE_Hang] ON [dbo].[DONGXE]
(
	[MaHangXe] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_DatCoc]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_DatCoc] ON [dbo].[DONHANG]
(
	[LoaiDonHang] ASC,
	[TrangThaiThanhToan] ASC,
	[SoTienConLai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_GiaoNhan]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_GiaoNhan] ON [dbo].[DONHANG]
(
	[PhuongThucNhanHang] ASC,
	[TrangThaiVanChuyen] ASC,
	[NgayHenNhanXe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONHANG_MaGioHang]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_MaGioHang] ON [dbo].[DONHANG]
(
	[MaGioHang] ASC
)
WHERE ([MaGioHang] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_DONHANG_Status_Date]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_Status_Date] ON [dbo].[DONHANG]
(
	[TrangThaiDonHang] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DONHANG_User_Date]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_DONHANG_User_Date] ON [dbo].[DONHANG]
(
	[MaNguoiDung] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_DONHANG_MaGioHang_NotNull]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_DONHANG_MaGioHang_NotNull] ON [dbo].[DONHANG]
(
	[MaGioHang] ASC
)
WHERE ([MaGioHang] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_FAQ_Category_Active]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_FAQ_Category_Active] ON [dbo].[FAQ]
(
	[DanhMuc] ASC,
	[DangHoatDong] ASC,
	[ThuTuHienThi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_GIOHANG_User_Active]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_GIOHANG_User_Active] ON [dbo].[GIOHANG]
(
	[MaNguoiDung] ASC
)
WHERE ([TrangThai]='Active')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LIENHE_Product]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_LIENHE_Product] ON [dbo].[LIENHE_YEUCAU]
(
	[MaSanPham] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_LIENHE_Status_Date]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_LIENHE_Status_Date] ON [dbo].[LIENHE_YEUCAU]
(
	[TrangThai] ASC,
	[NgayTao] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_NGUOIDUNG_SoDienThoai]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_NGUOIDUNG_SoDienThoai] ON [dbo].[NGUOIDUNG]
(
	[SoDienThoai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_NGUOIDUNG_DIACHI_MacDinh]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_NGUOIDUNG_DIACHI_MacDinh] ON [dbo].[NGUOIDUNG_DIACHI]
(
	[MaNguoiDung] ASC
)
WHERE ([LaMacDinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_PHUTUNG_TUONGTHICH_LocXe]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Index [IX_PHUTUNG_TUONGTHICH_MaPhuTung]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_PHUTUNG_TUONGTHICH_MaPhuTung] ON [dbo].[PHUTUNG_TUONGTHICH]
(
	[MaPhuTung] ASC,
	[DangHoatDong] ASC
)
INCLUDE([MaHangXe],[MaDongXe],[NamTu],[NamDen],[ApDungTatCaXe]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SANPHAM_Brand_Model]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_SANPHAM_Brand_Model] ON [dbo].[SANPHAM]
(
	[MaHangXe] ASC,
	[MaDongXe] ASC,
	[DangHoatDong] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_SANPHAM_Price]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_SANPHAM_Price] ON [dbo].[SANPHAM]
(
	[GiaGoc] ASC,
	[GiaKhuyenMai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_THANHTOAN_MaDonHang_Loai_TrangThai]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_THANHTOAN_MaDonHang_Loai_TrangThai] ON [dbo].[THANHTOAN]
(
	[MaDonHang] ASC,
	[LoaiThanhToan] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_THANHTOAN_Order]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_THANHTOAN_Order] ON [dbo].[THANHTOAN]
(
	[MaDonHang] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_THANHTOAN_HOANTIEN_MaThanhToan]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_THANHTOAN_HOANTIEN_MaThanhToan] ON [dbo].[THANHTOAN_HOANTIEN]
(
	[MaThanhToan] ASC,
	[TrangThai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_TONKHO_GIUCHO_Active_ByVariant]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_TONKHO_GIUCHO_Active_ByVariant] ON [dbo].[TONKHO_GIUCHO]
(
	[MaSanPham] ASC,
	[MaBienSanPham] ASC,
	[TrangThai] ASC,
	[HetHanLuc] ASC
)
INCLUDE([SoLuong],[MaDonHang]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_TONKHO_GIUCHO_Active_OrderDetail]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_TONKHO_GIUCHO_Active_OrderDetail] ON [dbo].[TONKHO_GIUCHO]
(
	[MaChiTietDonHang] ASC
)
WHERE ([TrangThai]='Active' AND [MaChiTietDonHang] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_VOUCHER_Active_Time]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_VOUCHER_Active_Time] ON [dbo].[VOUCHER]
(
	[DangHoatDong] ASC,
	[NgayBatDau] ASC,
	[NgayKetThuc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_VOUCHER_MaVoucherCode]    Script Date: 5/7/2026 4:04:06 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_VOUCHER_MaVoucherCode] ON [dbo].[VOUCHER]
(
	[MaVoucherCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ANHSANPHAM] ADD  CONSTRAINT [DF_ANHSANPHAM_LaAnhChinh]  DEFAULT ((0)) FOR [LaAnhChinh]
GO
ALTER TABLE [dbo].[ANHSANPHAM] ADD  CONSTRAINT [DF_ANHSANPHAM_ThuTu]  DEFAULT ((0)) FOR [ThuTuHienThi]
GO
ALTER TABLE [dbo].[ANHSANPHAM] ADD  CONSTRAINT [DF_ANHSANPHAM_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[BAIVIET] ADD  CONSTRAINT [DF_BAIVIET_TrangThai]  DEFAULT ('Draft') FOR [TrangThai]
GO
ALTER TABLE [dbo].[BAIVIET] ADD  CONSTRAINT [DF_BAIVIET_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[BAIVIET] ADD  CONSTRAINT [DF_BAIVIET_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[BIENSANPHAM] ADD  CONSTRAINT [DF_BIENSANPHAM_TrangThai]  DEFAULT ('Available') FOR [TrangThai]
GO
ALTER TABLE [dbo].[BIENSANPHAM] ADD  CONSTRAINT [DF_BIENSANPHAM_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[BIENSANPHAM] ADD  CONSTRAINT [DF_BIENSANPHAM_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] ADD  CONSTRAINT [DF_CHITIET_GIOHANG_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[CHITIET_GIOHANG] ADD  CONSTRAINT [DF_CHITIET_GIOHANG_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] ADD  CONSTRAINT [DF_DANHGIA_TrangThai]  DEFAULT ('Pending') FOR [TrangThai]
GO
ALTER TABLE [dbo].[DANHGIASANPHAM] ADD  CONSTRAINT [DF_DANHGIA_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[DANHMUC] ADD  CONSTRAINT [DF_DANHMUC_ThuTu]  DEFAULT ((0)) FOR [ThuTuHienThi]
GO
ALTER TABLE [dbo].[DANHMUC] ADD  CONSTRAINT [DF_DANHMUC_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[DANHMUC] ADD  CONSTRAINT [DF_DANHMUC_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[DANHMUC] ADD  CONSTRAINT [DF_DANHMUC_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[DONGXE] ADD  CONSTRAINT [DF_DONGXE_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[DONGXE] ADD  CONSTRAINT [DF_DONGXE_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[DONGXE] ADD  CONSTRAINT [DF_DONGXE_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_Subtotal]  DEFAULT ((0)) FOR [TongTienHang]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_Discount]  DEFAULT ((0)) FOR [TienGiam]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_Shipping]  DEFAULT ((0)) FOR [PhiVanChuyen]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_Total]  DEFAULT ((0)) FOR [TongThanhToan]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_OrderStatus]  DEFAULT ('Pending') FOR [TrangThaiDonHang]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_PaymentStatus]  DEFAULT ('Unpaid') FOR [TrangThaiThanhToan]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_PhuongThucNhanHang]  DEFAULT ('Delivery') FOR [PhuongThucNhanHang]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_TrangThaiVanChuyen]  DEFAULT ('NotShipped') FOR [TrangThaiVanChuyen]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_LoaiDonHang]  DEFAULT ('FullPayment') FOR [LoaiDonHang]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_TienDatCoc]  DEFAULT ((0)) FOR [TienDatCoc]
GO
ALTER TABLE [dbo].[DONHANG] ADD  CONSTRAINT [DF_DONHANG_SoTienConLai]  DEFAULT ((0)) FOR [SoTienConLai]
GO
ALTER TABLE [dbo].[DONHANG_VOUCHER] ADD  CONSTRAINT [DF_DONHANG_VOUCHER_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[FAQ] ADD  CONSTRAINT [DF_FAQ_ThuTu]  DEFAULT ((0)) FOR [ThuTuHienThi]
GO
ALTER TABLE [dbo].[FAQ] ADD  CONSTRAINT [DF_FAQ_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[FAQ] ADD  CONSTRAINT [DF_FAQ_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[FAQ] ADD  CONSTRAINT [DF_FAQ_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[GIOHANG] ADD  CONSTRAINT [DF_GIOHANG_TrangThai]  DEFAULT ('Active') FOR [TrangThai]
GO
ALTER TABLE [dbo].[GIOHANG] ADD  CONSTRAINT [DF_GIOHANG_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[GIOHANG] ADD  CONSTRAINT [DF_GIOHANG_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[HANGXE] ADD  CONSTRAINT [DF_HANGXE_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[HANGXE] ADD  CONSTRAINT [DF_HANGXE_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[HANGXE] ADD  CONSTRAINT [DF_HANGXE_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] ADD  CONSTRAINT [DF_LIENHE_TrangThai]  DEFAULT ('New') FOR [TrangThai]
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] ADD  CONSTRAINT [DF_LIENHE_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[NGUOIDUNG] ADD  CONSTRAINT [DF_NGUOIDUNG_TrangThai]  DEFAULT ('Active') FOR [TrangThai]
GO
ALTER TABLE [dbo].[NGUOIDUNG] ADD  CONSTRAINT [DF_NGUOIDUNG_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[NGUOIDUNG] ADD  CONSTRAINT [DF_NGUOIDUNG_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[NGUOIDUNG_DIACHI] ADD  CONSTRAINT [DF_NGUOIDUNG_DIACHI_LaMacDinh]  DEFAULT ((1)) FOR [LaMacDinh]
GO
ALTER TABLE [dbo].[NGUOIDUNG_DIACHI] ADD  CONSTRAINT [DF_NGUOIDUNG_DIACHI_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[NGUOIDUNG_DIACHI] ADD  CONSTRAINT [DF_NGUOIDUNG_DIACHI_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[NGUOIDUNG_VAITRO] ADD  CONSTRAINT [DF_NGUOIDUNG_VAITRO_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] ADD  CONSTRAINT [DF_PHUTUNG_TUONGTHICH_ApDungTatCaXe]  DEFAULT ((0)) FOR [ApDungTatCaXe]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] ADD  CONSTRAINT [DF_PHUTUNG_TUONGTHICH_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] ADD  CONSTRAINT [DF_PHUTUNG_TUONGTHICH_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[PHUTUNG_TUONGTHICH] ADD  CONSTRAINT [DF_PHUTUNG_TUONGTHICH_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [DF_SANPHAM_SoLuongTon]  DEFAULT ((0)) FOR [SoLuongTon]
GO
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [DF_SANPHAM_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [DF_SANPHAM_TrangThai]  DEFAULT ('Available') FOR [TrangThaiSanPham]
GO
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [DF_SANPHAM_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[SANPHAM] ADD  CONSTRAINT [DF_SANPHAM_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[SHOWROOM] ADD  CONSTRAINT [DF_SHOWROOM_DangHoatDong]  DEFAULT ((1)) FOR [DangHoatDong]
GO
ALTER TABLE [dbo].[SHOWROOM] ADD  CONSTRAINT [DF_SHOWROOM_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[SHOWROOM] ADD  CONSTRAINT [DF_SHOWROOM_NgayCapNhat]  DEFAULT (sysutcdatetime()) FOR [NgayCapNhat]
GO
ALTER TABLE [dbo].[THANHTOAN] ADD  CONSTRAINT [DF_THANHTOAN_TrangThai]  DEFAULT ('Pending') FOR [TrangThai]
GO
ALTER TABLE [dbo].[THANHTOAN] ADD  CONSTRAINT [DF_THANHTOAN_NgayTao]  DEFAULT (sysutcdatetime()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[THANHTOAN] ADD  CONSTRAINT [DF_THANHTOAN_SoTienHoan]  DEFAULT ((0)) FOR [SoTienHoan]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] ADD  CONSTRAINT [DF_THANHTOAN_HOANTIEN_TrangThai]  DEFAULT ('Succeeded') FOR [TrangThai]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] ADD  CONSTRAINT [DF_THANHTOAN_HOANTIEN_NgayTao]  DEFAULT (sysdatetime()) FOR [NgayTao]
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
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [FK_DONHANG_SHOWROOM] FOREIGN KEY([MaShowroom])
REFERENCES [dbo].[SHOWROOM] ([MaShowroom])
GO
ALTER TABLE [dbo].[DONHANG] CHECK CONSTRAINT [FK_DONHANG_SHOWROOM]
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
ALTER TABLE [dbo].[LIENHE_YEUCAU]  WITH NOCHECK ADD  CONSTRAINT [FK_LIENHE_SHOWROOM] FOREIGN KEY([MaShowroom])
REFERENCES [dbo].[SHOWROOM] ([MaShowroom])
GO
ALTER TABLE [dbo].[LIENHE_YEUCAU] CHECK CONSTRAINT [FK_LIENHE_SHOWROOM]
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
ALTER TABLE [dbo].[SANPHAM]  WITH NOCHECK ADD  CONSTRAINT [FK_SANPHAM_SHOWROOM] FOREIGN KEY([MaShowroom])
REFERENCES [dbo].[SHOWROOM] ([MaShowroom])
GO
ALTER TABLE [dbo].[SANPHAM] CHECK CONSTRAINT [FK_SANPHAM_SHOWROOM]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH NOCHECK ADD  CONSTRAINT [FK_THANHTOAN_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [FK_THANHTOAN_DONHANG]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN]  WITH NOCHECK ADD  CONSTRAINT [FK_THANHTOAN_HOANTIEN_DONHANG] FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DONHANG] ([MaDonHang])
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] CHECK CONSTRAINT [FK_THANHTOAN_HOANTIEN_DONHANG]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN]  WITH NOCHECK ADD  CONSTRAINT [FK_THANHTOAN_HOANTIEN_THANHTOAN] FOREIGN KEY([MaThanhToan])
REFERENCES [dbo].[THANHTOAN] ([MaThanhToan])
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] CHECK CONSTRAINT [FK_THANHTOAN_HOANTIEN_THANHTOAN]
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
ALTER TABLE [dbo].[DONHANG]  WITH NOCHECK ADD  CONSTRAINT [CK_DONHANG_OrderStatus] CHECK  (([TrangThaiDonHang]='Cancelled' OR [TrangThaiDonHang]='Completed' OR [TrangThaiDonHang]='Processing' OR [TrangThaiDonHang]='Confirmed' OR [TrangThaiDonHang]='AwaitingPayment' OR [TrangThaiDonHang]='Checkout' OR [TrangThaiDonHang]='Pending'))
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
ALTER TABLE [dbo].[SHOWROOM]  WITH NOCHECK ADD  CONSTRAINT [CK_SHOWROOM_Email] CHECK  (([Email] IS NULL OR [Email] like N'%_@_%._%'))
GO
ALTER TABLE [dbo].[SHOWROOM] CHECK CONSTRAINT [CK_SHOWROOM_Email]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH CHECK ADD  CONSTRAINT [CK_THANHTOAN_LoaiThanhToan] CHECK  (([LoaiThanhToan]='Installment' OR [LoaiThanhToan]='Remaining' OR [LoaiThanhToan]='Deposit' OR [LoaiThanhToan]='Full'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_LoaiThanhToan]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_Method] CHECK  (([PhuongThuc]='VNPay' OR [PhuongThuc]='Momo' OR [PhuongThuc]='Card' OR [PhuongThuc]='BankTransfer' OR [PhuongThuc]='COD'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_Method]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_SoTien] CHECK  (([SoTien]>=(0)))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_SoTien]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_SoTienHoan] CHECK  (([SoTienHoan]>=(0) AND [SoTienHoan]<=[SoTien]))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_SoTienHoan]
GO
ALTER TABLE [dbo].[THANHTOAN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_Status] CHECK  (([TrangThai]='PartiallyRefunded' OR [TrangThai]='Refunded' OR [TrangThai]='Cancelled' OR [TrangThai]='Failed' OR [TrangThai]='Paid' OR [TrangThai]='Pending'))
GO
ALTER TABLE [dbo].[THANHTOAN] CHECK CONSTRAINT [CK_THANHTOAN_Status]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_HOANTIEN_SoTien] CHECK  (([SoTienHoan]>(0)))
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] CHECK CONSTRAINT [CK_THANHTOAN_HOANTIEN_SoTien]
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN]  WITH NOCHECK ADD  CONSTRAINT [CK_THANHTOAN_HOANTIEN_TrangThai] CHECK  (([TrangThai]='Cancelled' OR [TrangThai]='Failed' OR [TrangThai]='Succeeded' OR [TrangThai]='Pending'))
GO
ALTER TABLE [dbo].[THANHTOAN_HOANTIEN] CHECK CONSTRAINT [CK_THANHTOAN_HOANTIEN_TrangThai]
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
/****** Object:  StoredProcedure [dbo].[sp_DonHang_BatDauCheckout]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_DonHang_DongBoTrangThaiThanhToan]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DonHang_DongBoTrangThaiThanhToan]
    @MaDonHang INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @MaDonHang IS NULL
        THROW 51100, N'MaDonHang không được để trống.', 1;

    DECLARE
        @TongThanhToan DECIMAL(18,2),
        @LoaiDonHang VARCHAR(20),
        @TienDatCoc DECIMAL(18,2),
        @TongDaGhiNhan DECIMAL(18,2),
        @TongDaHoan DECIMAL(18,2),
        @TongThucThu DECIMAL(18,2),
        @SoTienConLai DECIMAL(18,2),
        @SoGiaoDichPending INT,
        @SoGiaoDichFailed INT,
        @SoGiaoDichThanhCong INT,
        @SoGiaoDichTong INT,
        @TrangThaiMoi VARCHAR(20);

    SELECT
        @TongThanhToan = TongThanhToan,
        @LoaiDonHang = LoaiDonHang,
        @TienDatCoc = TienDatCoc
    FROM dbo.DONHANG WITH (UPDLOCK, ROWLOCK)
    WHERE MaDonHang = @MaDonHang;

    IF @TongThanhToan IS NULL
        THROW 51101, N'Không tìm thấy đơn hàng cần đồng bộ trạng thái thanh toán.', 1;

    SELECT
        @TongDaGhiNhan = ISNULL(SUM(CASE
            WHEN TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN SoTien
            ELSE 0
        END), 0),
        @TongDaHoan = ISNULL(SUM(CASE
            WHEN TrangThai IN ('PartiallyRefunded', 'Refunded') THEN SoTienHoan
            ELSE 0
        END), 0),
        @SoGiaoDichPending = ISNULL(SUM(CASE WHEN TrangThai = 'Pending' THEN 1 ELSE 0 END), 0),
        @SoGiaoDichFailed = ISNULL(SUM(CASE WHEN TrangThai = 'Failed' THEN 1 ELSE 0 END), 0),
        @SoGiaoDichThanhCong = ISNULL(SUM(CASE WHEN TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded') THEN 1 ELSE 0 END), 0),
        @SoGiaoDichTong = COUNT(1)
    FROM dbo.THANHTOAN WITH (READCOMMITTEDLOCK)
    WHERE MaDonHang = @MaDonHang;

    SET @TongThucThu = ISNULL(@TongDaGhiNhan, 0) - ISNULL(@TongDaHoan, 0);
    IF @TongThucThu < 0 SET @TongThucThu = 0;

    SET @SoTienConLai = @TongThanhToan - @TongThucThu;
    IF @SoTienConLai < 0 SET @SoTienConLai = 0;

    SET @TrangThaiMoi = CASE
        WHEN @TongThanhToan <= 0 THEN 'Paid'
        WHEN @TongThucThu >= @TongThanhToan THEN 'Paid'
        WHEN @TongDaGhiNhan > 0 AND @TongThucThu = 0 AND @TongDaHoan >= @TongDaGhiNhan THEN 'Refunded'
        WHEN @TongThucThu > 0 THEN 'PartiallyPaid'
        WHEN @SoGiaoDichTong > 0
             AND @SoGiaoDichPending = 0
             AND @SoGiaoDichThanhCong = 0
             AND @SoGiaoDichFailed > 0 THEN 'Failed'
        ELSE 'Unpaid'
    END;

    UPDATE dbo.DONHANG
    SET
        TrangThaiThanhToan = @TrangThaiMoi,
        TienDatCoc = CASE
            -- Với đơn đặt cọc, ghi nhận số tiền đã thu nhưng không vượt quá mức tiền cọc cấu hình.
            WHEN @LoaiDonHang = 'Deposit' THEN
                CASE
                    WHEN @TongThucThu >= ISNULL(@TienDatCoc, 0) THEN ISNULL(@TienDatCoc, 0)
                    ELSE @TongThucThu
                END
            ELSE 0
        END,
        SoTienConLai = @SoTienConLai,
        NgayThanhToanThanhCong = CASE
            WHEN @TrangThaiMoi = 'Paid' AND NgayThanhToanThanhCong IS NULL THEN SYSDATETIME()
            ELSE NgayThanhToanThanhCong
        END,
        NgayCapNhat = SYSDATETIME()
    WHERE MaDonHang = @MaDonHang;

    SELECT
        dh.MaDonHang,
        dh.MaDonHangKinhDoanh,
        dh.LoaiDonHang,
        dh.TongThanhToan,
        dh.TienDatCoc,
        TongDaGhiNhan = @TongDaGhiNhan,
        TongDaHoan = @TongDaHoan,
        TongThucThu = @TongThucThu,
        dh.SoTienConLai,
        dh.TrangThaiThanhToan
    FROM dbo.DONHANG dh
    WHERE dh.MaDonHang = @MaDonHang;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_DonHang_HuyVaNhaGiuCho]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_DonHang_XacNhanThanhToanTruTon]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DonHang_XacNhanThanhToanTruTon]
    @MaDonHang INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC dbo.sp_TonKho_DonGiuChoHetHan;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.TONKHO_GIUCHO
            WHERE MaDonHang = @MaDonHang
              AND TrangThai = 'Active'
              AND HetHanLuc > SYSDATETIME()
        )
            THROW 52000, N'Đơn hàng không có giữ chỗ tồn kho còn hiệu lực. Vui lòng checkout lại.', 1;

        DECLARE
            @LoaiDonHang VARCHAR(20),
            @TongThanhToan DECIMAL(18,2),
            @TienDatCocCanThu DECIMAL(18,2),
            @TongThucThu DECIMAL(18,2),
            @TrangThaiThanhToan VARCHAR(20);

        SELECT
            @LoaiDonHang = LoaiDonHang,
            @TongThanhToan = TongThanhToan,
            @TienDatCocCanThu = TienDatCoc
        FROM dbo.DONHANG WITH (UPDLOCK, ROWLOCK)
        WHERE MaDonHang = @MaDonHang;

        IF @LoaiDonHang IS NULL
            THROW 52001, N'Không tìm thấy đơn hàng cần xác nhận thanh toán.', 1;

        SELECT @TongThucThu = ISNULL(SUM(CASE
            WHEN TrangThai IN ('Paid', 'PartiallyRefunded', 'Refunded')
                THEN SoTien - SoTienHoan
            ELSE 0
        END), 0)
        FROM dbo.THANHTOAN WITH (READCOMMITTEDLOCK)
        WHERE MaDonHang = @MaDonHang;

        IF @LoaiDonHang = 'FullPayment' AND @TongThucThu < @TongThanhToan
            THROW 52002, N'Đơn thanh toán toàn bộ chưa thu đủ tiền, không được xác nhận đã thanh toán đủ.', 1;

        IF @LoaiDonHang = 'Deposit' AND @TongThucThu < @TienDatCocCanThu
            THROW 52003, N'Đơn đặt cọc chưa thu đủ tiền cọc, không được xác nhận đơn.', 1;

        -- Trừ tồn kho biến thể.
        UPDATE bt
        SET bt.SoLuongTon = bt.SoLuongTon - x.SoLuong,
            bt.NgayCapNhat = SYSDATETIME()
        FROM dbo.BIENSANPHAM bt WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN
        (
            SELECT MaBienSanPham, SUM(SoLuong) AS SoLuong
            FROM dbo.TONKHO_GIUCHO
            WHERE MaDonHang = @MaDonHang
              AND MaBienSanPham IS NOT NULL
              AND TrangThai = 'Active'
              AND HetHanLuc > SYSDATETIME()
            GROUP BY MaBienSanPham
        ) x ON x.MaBienSanPham = bt.MaBienSanPham;

        -- Trừ tồn kho sản phẩm nếu dòng hàng không dùng biến thể.
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon - x.SoLuong,
            sp.NgayCapNhat = SYSDATETIME()
        FROM dbo.SANPHAM sp WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN
        (
            SELECT MaSanPham, SUM(SoLuong) AS SoLuong
            FROM dbo.TONKHO_GIUCHO
            WHERE MaDonHang = @MaDonHang
              AND MaBienSanPham IS NULL
              AND TrangThai = 'Active'
              AND HetHanLuc > SYSDATETIME()
            GROUP BY MaSanPham
        ) x ON x.MaSanPham = sp.MaSanPham;

        UPDATE dbo.TONKHO_GIUCHO
        SET TrangThai = 'Confirmed',
            NgayCapNhat = SYSDATETIME(),
            GhiChu = ISNULL(GhiChu + N' | ', N'') +
                     CASE
                        WHEN @LoaiDonHang = 'Deposit'
                            THEN N'Đã nhận đủ tiền cọc, đã giữ/trừ tồn kho thật'
                        ELSE N'Đã thanh toán đủ, đã trừ tồn kho thật'
                     END
        WHERE MaDonHang = @MaDonHang
          AND TrangThai = 'Active';

        -- Đồng bộ đúng trạng thái thanh toán trước, tránh set Paid cứng.
        EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang = @MaDonHang;

        SELECT @TrangThaiThanhToan = TrangThaiThanhToan
        FROM dbo.DONHANG
        WHERE MaDonHang = @MaDonHang;

        UPDATE dbo.DONHANG
        SET TrangThaiDonHang = 'Confirmed',
            NgayCapNhat = SYSDATETIME()
        WHERE MaDonHang = @MaDonHang;

        COMMIT TRANSACTION;

        SELECT
            @MaDonHang AS MaDonHang,
            @LoaiDonHang AS LoaiDonHang,
            @TongThucThu AS TongThucThu,
            @TongThanhToan AS TongThanhToan,
            @TrangThaiThanhToan AS TrangThaiThanhToan,
            CASE
                WHEN @TrangThaiThanhToan = 'Paid'
                    THEN N'Đã thanh toán đủ và xác nhận đơn hàng.'
                ELSE N'Đã nhận tiền cọc và xác nhận đơn hàng. Đơn vẫn còn số tiền cần thanh toán.'
            END AS ThongBao;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[sp_PhuTung_LayTheoXe]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_PhuTung_UpsertTuongThich]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_SANPHAM_DongBoSoLuongTon]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_SANPHAM_DongBoTatCaSoLuongTon]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_ThanhToan_HoanTien]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 9) Hoàn tiền một phần hoặc toàn phần cho giao dịch Paid/PartiallyRefunded */
CREATE   PROCEDURE [dbo].[sp_ThanhToan_HoanTien]
    @MaThanhToan int,
    @SoTienHoan decimal(18,2),
    @LyDo nvarchar(500) = NULL,
    @MaGiaoDichHoanTien nvarchar(120) = NULL,
    @ResponseRaw nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoTienHoan <= 0
        THROW 51040, N'Số tiền hoàn phải lớn hơn 0.', 1;

    DECLARE @MaDonHang int,
            @SoTien decimal(18,2),
            @SoTienHoanHienTai decimal(18,2),
            @TrangThai varchar(20),
            @TongHoanSau decimal(18,2),
            @TrangThaiMoi varchar(20);

    BEGIN TRAN;

    SELECT
        @MaDonHang = MaDonHang,
        @SoTien = SoTien,
        @SoTienHoanHienTai = SoTienHoan,
        @TrangThai = TrangThai
    FROM dbo.THANHTOAN WITH (UPDLOCK, ROWLOCK)
    WHERE MaThanhToan = @MaThanhToan;

    IF @MaDonHang IS NULL
        THROW 51041, N'Không tìm thấy giao dịch thanh toán.', 1;

    IF @TrangThai NOT IN ('Paid', 'PartiallyRefunded')
        THROW 51042, N'Chỉ được hoàn tiền giao dịch đã thanh toán.', 1;

    SET @TongHoanSau = @SoTienHoanHienTai + @SoTienHoan;

    IF @TongHoanSau > @SoTien
        THROW 51043, N'Số tiền hoàn vượt quá số tiền đã thanh toán.', 1;

    SET @TrangThaiMoi = CASE WHEN @TongHoanSau = @SoTien THEN 'Refunded' ELSE 'PartiallyRefunded' END;

    INSERT INTO dbo.THANHTOAN_HOANTIEN
    (
        MaThanhToan, MaDonHang, SoTienHoan, MaGiaoDichHoanTien,
        LyDo, TrangThai, ResponseRaw, NgayTao
    )
    VALUES
    (
        @MaThanhToan, @MaDonHang, @SoTienHoan, @MaGiaoDichHoanTien,
        @LyDo, 'Succeeded', @ResponseRaw, SYSDATETIME()
    );

    UPDATE dbo.THANHTOAN
    SET SoTienHoan = @TongHoanSau,
        TrangThai = @TrangThaiMoi,
        MaGiaoDichHoanTien = COALESCE(@MaGiaoDichHoanTien, MaGiaoDichHoanTien),
        NgayHoanTien = SYSDATETIME(),
        ResponseRaw = COALESCE(@ResponseRaw, ResponseRaw)
    WHERE MaThanhToan = @MaThanhToan;

    EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang = @MaDonHang;

    COMMIT;

    SELECT * FROM dbo.v_THANHTOAN_DONHANG_TONGHOP WHERE MaDonHang = @MaDonHang;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_ThanhToan_HuyGiaoDich]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 8) Hủy giao dịch: chỉ nên hủy giao dịch Pending hoặc Failed */
CREATE   PROCEDURE [dbo].[sp_ThanhToan_HuyGiaoDich]
    @MaThanhToan int,
    @LyDoHuy nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaDonHang int, @TrangThai varchar(20);

    BEGIN TRAN;

    SELECT @MaDonHang = MaDonHang, @TrangThai = TrangThai
    FROM dbo.THANHTOAN WITH (UPDLOCK, ROWLOCK)
    WHERE MaThanhToan = @MaThanhToan;

    IF @MaDonHang IS NULL
        THROW 51030, N'Không tìm thấy giao dịch thanh toán.', 1;

    IF @TrangThai NOT IN ('Pending', 'Failed')
        THROW 51031, N'Chỉ được hủy giao dịch Pending hoặc Failed. Giao dịch đã Paid thì dùng nghiệp vụ hoàn tiền.', 1;

    UPDATE dbo.THANHTOAN
    SET TrangThai = 'Cancelled',
        LyDoHuy = @LyDoHuy,
        NgayHuy = SYSDATETIME()
    WHERE MaThanhToan = @MaThanhToan;

    EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang = @MaDonHang;

    COMMIT;

    SELECT * FROM dbo.THANHTOAN WHERE MaThanhToan = @MaThanhToan;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_ThanhToan_TaoGiaoDich]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 6) Tạo giao dịch thanh toán: dùng cho đặt cọc, trả phần còn lại, hoặc trả toàn bộ */
CREATE   PROCEDURE [dbo].[sp_ThanhToan_TaoGiaoDich]
    @MaDonHang int,
    @LoaiThanhToan varchar(30),      -- Deposit / Remaining / Full
    @SoTien decimal(18,2),
    @PhuongThuc varchar(30),         -- COD / BankTransfer / Card / Momo / VNPay
    @MaGiaoDich nvarchar(120) = NULL,
    @NoiDungChuyenKhoan nvarchar(500) = NULL,
    @MaNganHang nvarchar(50) = NULL,
    @ResponseRaw nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @LoaiThanhToan NOT IN ('Deposit', 'Remaining', 'Full')
        THROW 51010, N'LoaiThanhToan không hợp lệ.', 1;

    IF @SoTien <= 0
        THROW 51011, N'Số tiền thanh toán phải lớn hơn 0.', 1;

    IF @PhuongThuc NOT IN ('COD', 'BankTransfer', 'Card', 'Momo', 'VNPay')
        THROW 51012, N'Phương thức thanh toán không hợp lệ.', 1;

    DECLARE @TongThanhToan decimal(18,2),
            @TongThucThu decimal(18,2),
            @ConPhaiThu decimal(18,2),
            @MaThanhToan int,
            @Code nvarchar(50);

    BEGIN TRAN;

    SELECT @TongThanhToan = TongThanhToan
    FROM dbo.DONHANG WITH (UPDLOCK, ROWLOCK)
    WHERE MaDonHang = @MaDonHang;

    IF @TongThanhToan IS NULL
        THROW 51013, N'Không tìm thấy đơn hàng.', 1;

    SELECT @TongThucThu = TongThucThu
    FROM dbo.v_THANHTOAN_DONHANG_TONGHOP
    WHERE MaDonHang = @MaDonHang;

    SET @ConPhaiThu = CASE WHEN @TongThanhToan - ISNULL(@TongThucThu,0) < 0 THEN 0 ELSE @TongThanhToan - ISNULL(@TongThucThu,0) END;

    IF @ConPhaiThu <= 0
        THROW 51014, N'Đơn hàng đã thanh toán đủ.', 1;

    IF @SoTien > @ConPhaiThu
        THROW 51015, N'Số tiền thanh toán vượt quá số tiền còn phải thu.', 1;

    IF @LoaiThanhToan = 'Full' AND ISNULL(@TongThucThu,0) > 0
        THROW 51016, N'Đơn hàng đã có thanh toán trước đó, không thể tạo thanh toán Full.', 1;

    SET @Code = CONCAT(N'PAY', FORMAT(SYSDATETIME(), 'yyyyMMddHHmmss'), RIGHT(CONVERT(varchar(36), NEWID()), 6));

    INSERT INTO dbo.THANHTOAN
    (
        MaThanhToanKinhDoanh, MaDonHang, SoTien, PhuongThuc, TrangThai,
        MaGiaoDich, DaThanhToanLuc, NgayTao, LoaiThanhToan,
        NoiDungChuyenKhoan, MaNganHang, ResponseRaw
    )
    VALUES
    (
        @Code, @MaDonHang, @SoTien, @PhuongThuc, 'Pending',
        @MaGiaoDich, NULL, SYSDATETIME(), @LoaiThanhToan,
        @NoiDungChuyenKhoan, @MaNganHang, @ResponseRaw
    );

    SET @MaThanhToan = SCOPE_IDENTITY();

    COMMIT;

    SELECT * FROM dbo.THANHTOAN WHERE MaThanhToan = @MaThanhToan;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_ThanhToan_XacNhanThanhCong]    Script Date: 5/7/2026 4:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 7) Xác nhận giao dịch thành công */
CREATE   PROCEDURE [dbo].[sp_ThanhToan_XacNhanThanhCong]
    @MaThanhToan int,
    @MaGiaoDich nvarchar(120) = NULL,
    @ResponseRaw nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaDonHang int;

    BEGIN TRAN;

    SELECT @MaDonHang = MaDonHang
    FROM dbo.THANHTOAN WITH (UPDLOCK, ROWLOCK)
    WHERE MaThanhToan = @MaThanhToan
      AND TrangThai = 'Pending';

    IF @MaDonHang IS NULL
        THROW 51020, N'Không tìm thấy giao dịch Pending cần xác nhận.', 1;

    UPDATE dbo.THANHTOAN
    SET TrangThai = 'Paid',
        MaGiaoDich = COALESCE(@MaGiaoDich, MaGiaoDich),
        DaThanhToanLuc = SYSDATETIME(),
        ResponseRaw = COALESCE(@ResponseRaw, ResponseRaw)
    WHERE MaThanhToan = @MaThanhToan;

    EXEC dbo.sp_DonHang_DongBoTrangThaiThanhToan @MaDonHang = @MaDonHang;

    COMMIT;

    SELECT * FROM dbo.v_THANHTOAN_DONHANG_TONGHOP WHERE MaDonHang = @MaDonHang;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_TonKho_DonGiuChoHetHan]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_Voucher_GhiNhanSuDung]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_Voucher_HuySuDungTheoDon]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  StoredProcedure [dbo].[sp_Voucher_KiemTraTruocKhiTaoDon]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_ANHSANPHAM_Validate_MaBienSanPham]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_CHITIET_DONHANG_Validate_MaBienSanPham]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_CHITIET_GIOHANG_Validate_MaBienSanPham]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_PHUTUNG_TUONGTHICH_Validate]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_SANPHAM_Validate_HangXe_DongXe]    Script Date: 5/7/2026 4:04:06 PM ******/
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
/****** Object:  Trigger [dbo].[trg_TONKHO_GIUCHO_Validate_MaBienSanPham]    Script Date: 5/7/2026 4:04:06 PM ******/
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

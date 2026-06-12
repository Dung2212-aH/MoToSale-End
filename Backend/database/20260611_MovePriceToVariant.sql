/*
================================================================================
  Migration: 20260611_MovePriceToVariant
  Mục đích : Chuyển giá & tồn kho từ SANPHAM (sản phẩm) xuống BIENSANPHAM (biến thể/SKU).
             - BIENSANPHAM: thêm GiaGoc (NOT NULL) + GiaKhuyenMai (nullable),
               thay cho cột GiaGhiDe (giá ghi đè) khó hiểu.
             - SANPHAM: bỏ hẳn GiaGoc, GiaKhuyenMai, SoLuongTon.
  An toàn  : Idempotent (kiểm tra tồn tại trước mỗi bước). Backup DB trước khi chạy.
================================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*------------------------------------------------------------------
  1) Thêm cột giá mới vào BIENSANPHAM (nullable để backfill)
------------------------------------------------------------------*/
IF COL_LENGTH('dbo.BIENSANPHAM', 'GiaGoc') IS NULL
    ALTER TABLE dbo.BIENSANPHAM ADD GiaGoc decimal(18, 2) NULL;
GO
IF COL_LENGTH('dbo.BIENSANPHAM', 'GiaKhuyenMai') IS NULL
    ALTER TABLE dbo.BIENSANPHAM ADD GiaKhuyenMai decimal(18, 2) NULL;
GO

/*------------------------------------------------------------------
  2) Backfill các biến thể hiện có
     Giá hiệu lực cũ = COALESCE(bt.GiaGhiDe, sp.GiaKhuyenMai, sp.GiaGoc)
     => GiaGoc       = COALESCE(bt.GiaGhiDe, sp.GiaGoc)
        GiaKhuyenMai = nếu có GiaGhiDe thì NULL, ngược lại lấy sp.GiaKhuyenMai
------------------------------------------------------------------*/
-- Dùng dynamic SQL để câu lệnh chỉ được biên dịch khi các cột cũ còn tồn tại
-- (nếu không, SQL Server báo "Invalid column name" ngay ở bước biên dịch dù đã có IF).
IF COL_LENGTH('dbo.BIENSANPHAM', 'GiaGhiDe') IS NOT NULL
   AND COL_LENGTH('dbo.SANPHAM', 'GiaGoc') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        UPDATE bt
        SET bt.GiaGoc       = COALESCE(bt.GiaGhiDe, sp.GiaGoc),
            bt.GiaKhuyenMai = CASE WHEN bt.GiaGhiDe IS NOT NULL THEN NULL ELSE sp.GiaKhuyenMai END
        FROM dbo.BIENSANPHAM bt
        INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = bt.MaSanPham
        WHERE bt.GiaGoc IS NULL;';
END
GO

/*------------------------------------------------------------------
  3) Tạo biến thể mặc định cho sản phẩm CHƯA có biến thể nào
     (sao chép giá & tồn từ SANPHAM). SKU = MaSanPhamKinhDoanh + '-DEFAULT'
------------------------------------------------------------------*/
IF COL_LENGTH('dbo.SANPHAM', 'GiaGoc') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        INSERT INTO dbo.BIENSANPHAM
            (MaSanPham, TenBienThe, SKU, GiaGoc, GiaKhuyenMai, SoLuongTon, TrangThai, PhienBan, NgayTao, NgayCapNhat, MauSac)
        SELECT
            sp.MaSanPham,
            sp.TenSanPham,
            LEFT(sp.MaSanPhamKinhDoanh, 72) + N''-DEFAULT'',
            sp.GiaGoc,
            sp.GiaKhuyenMai,
            ISNULL(sp.SoLuongTon, 0),
            N''Available'',
            NULL,
            SYSUTCDATETIME(),
            SYSUTCDATETIME(),
            NULL
        FROM dbo.SANPHAM sp
        WHERE NOT EXISTS (SELECT 1 FROM dbo.BIENSANPHAM bt WHERE bt.MaSanPham = sp.MaSanPham);';
END
GO

/*------------------------------------------------------------------
  4) Khóa NOT NULL cho GiaGoc (backfill giá trị còn thiếu = 0)
------------------------------------------------------------------*/
UPDATE dbo.BIENSANPHAM SET GiaGoc = 0 WHERE GiaGoc IS NULL;
GO
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.BIENSANPHAM') AND name = 'GiaGoc' AND is_nullable = 1)
    ALTER TABLE dbo.BIENSANPHAM ALTER COLUMN GiaGoc decimal(18, 2) NOT NULL;
GO

/*------------------------------------------------------------------
  5) Gỡ trigger / stored procedure đồng bộ tồn lên SANPHAM (không còn cần)
------------------------------------------------------------------*/
IF OBJECT_ID('dbo.trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM;
GO
IF OBJECT_ID('dbo.sp_SANPHAM_DongBoSoLuongTon', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SANPHAM_DongBoSoLuongTon;
GO
IF OBJECT_ID('dbo.sp_SANPHAM_DongBoTatCaSoLuongTon', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_SANPHAM_DongBoTatCaSoLuongTon;
GO

/*------------------------------------------------------------------
  6) Cập nhật các VIEW phụ thuộc giá/tồn của SANPHAM
------------------------------------------------------------------*/
-- 6a) v_SANPHAM_BIENTHE_ANH: GiaBan lấy hoàn toàn từ biến thể
IF OBJECT_ID('dbo.v_SANPHAM_BIENTHE_ANH', 'V') IS NOT NULL
    DROP VIEW dbo.v_SANPHAM_BIENTHE_ANH;
GO
CREATE VIEW [dbo].[v_SANPHAM_BIENTHE_ANH]
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
    GiaBan = COALESCE(bt.GiaKhuyenMai, bt.GiaGoc),
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
        a.MaAnhSanPham, a.UrlAnh, a.AltText, a.LaAnhChinh, a.ThuTuHienThi
    FROM dbo.ANHSANPHAM a
    WHERE a.MaSanPham = sp.MaSanPham
      AND (a.MaBienSanPham = bt.MaBienSanPham OR a.MaBienSanPham IS NULL)
    ORDER BY
        CASE WHEN a.MaBienSanPham = bt.MaBienSanPham THEN 0 ELSE 1 END,
        CASE WHEN a.LaAnhChinh = 1 THEN 0 ELSE 1 END,
        a.ThuTuHienThi, a.MaAnhSanPham
) a;
GO

-- 6b) v_PHUTUNG_TUONGTHICH: lấy giá thấp nhất + tổng tồn từ biến thể
IF OBJECT_ID('dbo.v_PHUTUNG_TUONGTHICH', 'V') IS NOT NULL
    DROP VIEW dbo.v_PHUTUNG_TUONGTHICH;
GO
CREATE VIEW [dbo].[v_PHUTUNG_TUONGTHICH]
AS
SELECT
    ptt.MaTuongThich,
    ptt.MaPhuTung,
    sp.TenSanPham AS TenPhuTung,
    sp.Slug AS SlugPhuTung,
    sp.AnhChinhUrl,
    v.GiaThapNhat AS GiaGoc,
    v.GiaBanThapNhat AS GiaKhuyenMai,
    v.TongTon AS SoLuongTon,
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
OUTER APPLY
(
    SELECT
        MIN(bt.GiaGoc) AS GiaThapNhat,
        MIN(COALESCE(bt.GiaKhuyenMai, bt.GiaGoc)) AS GiaBanThapNhat,
        SUM(ISNULL(bt.SoLuongTon, 0)) AS TongTon
    FROM dbo.BIENSANPHAM bt
    WHERE bt.MaSanPham = sp.MaSanPham
) v
LEFT JOIN dbo.HANGXE hx ON hx.MaHangXe = ptt.MaHangXe
LEFT JOIN dbo.DONGXE dx ON dx.MaDongXe = ptt.MaDongXe;
GO

-- 6c) v_SANPHAM_TONKHO_KIEMTRA: chỉ còn tổng tồn theo biến thể
IF OBJECT_ID('dbo.v_SANPHAM_TONKHO_KIEMTRA', 'V') IS NOT NULL
    DROP VIEW dbo.v_SANPHAM_TONKHO_KIEMTRA;
GO
CREATE VIEW [dbo].[v_SANPHAM_TONKHO_KIEMTRA]
AS
SELECT
    sp.MaSanPham,
    sp.TenSanPham,
    sp.LoaiSanPham,
    COUNT(bsp.MaBienSanPham) AS SoBienThe,
    ISNULL(SUM(ISNULL(bsp.SoLuongTon, 0)), 0) AS TongSoLuongTon_BienThe
FROM dbo.SANPHAM sp
LEFT JOIN dbo.BIENSANPHAM bsp
    ON bsp.MaSanPham = sp.MaSanPham
GROUP BY sp.MaSanPham, sp.TenSanPham, sp.LoaiSanPham;
GO

-- 6d) v_TONKHO_KHADUNG: bỏ nhánh tồn theo SANPHAM (mọi SP đều có biến thể)
IF OBJECT_ID('dbo.v_TONKHO_KHADUNG', 'V') IS NOT NULL
    DROP VIEW dbo.v_TONKHO_KHADUNG;
GO
CREATE VIEW [dbo].[v_TONKHO_KHADUNG]
AS
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

/*------------------------------------------------------------------
  7) Index: bỏ index giá trên SANPHAM, tạo index giá trên BIENSANPHAM
------------------------------------------------------------------*/
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SANPHAM_Price' AND object_id = OBJECT_ID('dbo.SANPHAM'))
    DROP INDEX [IX_SANPHAM_Price] ON dbo.SANPHAM;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BIENSANPHAM_Gia' AND object_id = OBJECT_ID('dbo.BIENSANPHAM'))
    CREATE NONCLUSTERED INDEX [IX_BIENSANPHAM_Gia] ON dbo.BIENSANPHAM (MaSanPham ASC, GiaGoc ASC, GiaKhuyenMai ASC);
GO

/*------------------------------------------------------------------
  8) Bỏ cột GiaGhiDe ở BIENSANPHAM
------------------------------------------------------------------*/
IF COL_LENGTH('dbo.BIENSANPHAM', 'GiaGhiDe') IS NOT NULL
    ALTER TABLE dbo.BIENSANPHAM DROP COLUMN GiaGhiDe;
GO

/*------------------------------------------------------------------
  9) Bỏ cột giá/tồn ở SANPHAM
------------------------------------------------------------------*/
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SANPHAM_Price' AND object_id = OBJECT_ID('dbo.SANPHAM'))
    DROP INDEX [IX_SANPHAM_Price] ON dbo.SANPHAM;
GO
-- Bỏ mọi DEFAULT constraint gắn trên các cột sắp drop (tên constraint do SQL Server sinh tự động)
DECLARE @dropDefaults nvarchar(max) = N'';
SELECT @dropDefaults += N'ALTER TABLE dbo.SANPHAM DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';' + CHAR(10)
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.SANPHAM')
  AND c.name IN ('GiaGoc', 'GiaKhuyenMai', 'SoLuongTon');
IF @dropDefaults <> N'' EXEC sys.sp_executesql @dropDefaults;
GO
IF COL_LENGTH('dbo.SANPHAM', 'GiaKhuyenMai') IS NOT NULL
    ALTER TABLE dbo.SANPHAM DROP COLUMN GiaKhuyenMai;
GO
IF COL_LENGTH('dbo.SANPHAM', 'GiaGoc') IS NOT NULL
    ALTER TABLE dbo.SANPHAM DROP COLUMN GiaGoc;
GO
IF COL_LENGTH('dbo.SANPHAM', 'SoLuongTon') IS NOT NULL
    ALTER TABLE dbo.SANPHAM DROP COLUMN SoLuongTon;
GO

PRINT N'Migration 20260611_MovePriceToVariant hoàn tất.';
GO

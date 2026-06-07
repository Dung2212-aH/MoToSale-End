/* ============================================================================
   20260604_BusinessLogicAudit_Fixes.sql
   Sửa các lỗi nghiệp vụ phát hiện khi audit DB ShowroomDB.
   Script idempotent: chạy lại nhiều lần an toàn.

   Nội dung:
     §1. DONGXE: thêm cột LoaiXe (xe số / tay ga / côn tay / xe điện) + ràng buộc.
     §2. DONHANG: sửa DEFAULT trạng thái đơn ('Pending' -> 'AwaitingPayment').
     §3. Bỏ 2 stored procedure hỏng tham chiếu cột không tồn tại CheckoutHetHanLuc.
     §4. SANPHAM: chuẩn hóa LoaiSanPham + thêm CHECK {'XeMay','PhuTung'}.
     §5. Sửa trigger đồng bộ tồn kho: reset về 0 khi xóa biến thể cuối cùng.
     §6. Dọn dữ liệu voucher bị ghi nhận trùng (double-count) + tính lại SoLanDaDung.
   ============================================================================ */

USE [ShowroomDB];
GO

-- sqlcmd mặc định QUOTED_IDENTIFIER OFF; bật ON để UPDATE trên bảng có filtered index /
-- computed column không bị chặn (Msg 1934), và để trigger được tạo với đúng SET options.
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* ----------------------------------------------------------------------------
   §1. DONGXE.LoaiXe — để khi thêm "dòng xe" đã biết loại: xe số / tay ga / côn tay
   ---------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.DONGXE', 'LoaiXe') IS NULL
BEGIN
    ALTER TABLE dbo.DONGXE ADD LoaiXe varchar(20) NULL;
END;
GO

-- Backfill: suy ra loại xe từ danh mục của các sản phẩm thuộc dòng xe đó
-- (lấy danh mục xuất hiện nhiều nhất cho mỗi dòng xe).
UPDATE dx
SET dx.LoaiXe = m.LoaiXe
FROM dbo.DONGXE dx
INNER JOIN
(
    SELECT t.MaDongXe,
           CASE t.Slug
               WHEN 'xe-so'       THEN 'XeSo'
               WHEN 'xe-tay-ga'   THEN 'TayGa'
               WHEN 'xe-con-tay'  THEN 'ConTay'
               WHEN 'xe-dien'     THEN 'XeDien'
               ELSE 'Khac'
           END AS LoaiXe
    FROM
    (
        SELECT sp.MaDongXe,
               d.Slug,
               ROW_NUMBER() OVER (PARTITION BY sp.MaDongXe ORDER BY COUNT(*) DESC) AS rn
        FROM dbo.SANPHAM sp
        INNER JOIN dbo.DANHMUC d ON d.MaDanhMuc = sp.MaDanhMuc
        WHERE sp.MaDongXe IS NOT NULL
        GROUP BY sp.MaDongXe, d.Slug
    ) t
    WHERE t.rn = 1
) m ON m.MaDongXe = dx.MaDongXe
WHERE dx.LoaiXe IS NULL;
GO

-- Các dòng xe chưa xác định được loại -> 'Khac'
UPDATE dbo.DONGXE SET LoaiXe = 'Khac' WHERE LoaiXe IS NULL;
GO

-- DEFAULT + NOT NULL + CHECK
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_DONGXE_LoaiXe')
    ALTER TABLE dbo.DONGXE ADD CONSTRAINT DF_DONGXE_LoaiXe DEFAULT ('Khac') FOR LoaiXe;
GO

ALTER TABLE dbo.DONGXE ALTER COLUMN LoaiXe varchar(20) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_DONGXE_LoaiXe')
    ALTER TABLE dbo.DONGXE WITH CHECK ADD CONSTRAINT CK_DONGXE_LoaiXe
        CHECK (LoaiXe IN ('XeSo', 'TayGa', 'ConTay', 'XeDien', 'Khac'));
GO

/* ----------------------------------------------------------------------------
   §2. DONHANG: DEFAULT trạng thái đơn 'Pending' vi phạm chính CK_DONHANG_OrderStatus
       (chỉ cho phép AwaitingPayment / Confirmed / Cancelled).
   ---------------------------------------------------------------------------- */
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_DONHANG_OrderStatus')
    ALTER TABLE dbo.DONHANG DROP CONSTRAINT DF_DONHANG_OrderStatus;
GO
ALTER TABLE dbo.DONHANG ADD CONSTRAINT DF_DONHANG_OrderStatus
    DEFAULT ('AwaitingPayment') FOR TrangThaiDonHang;
GO

/* ----------------------------------------------------------------------------
   §3. Bỏ 2 stored procedure hỏng: tham chiếu cột DONHANG.CheckoutHetHanLuc
       (không tồn tại) -> sẽ lỗi nếu chạy. Backend đã tự xử lý giữ chỗ bằng C#,
       không gọi 2 proc này.
   ---------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.sp_DonHang_BatDauCheckout', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DonHang_BatDauCheckout;
GO
IF OBJECT_ID('dbo.sp_TonKho_DonGiuChoHetHan', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_TonKho_DonGiuChoHetHan;
GO

/* ----------------------------------------------------------------------------
   §4. SANPHAM.LoaiSanPham: chuẩn hóa về {'XeMay','PhuTung'} rồi thêm CHECK.
       Tránh việc code NormalizeProductType biến 'PhuKien'/'Accessory'... -> 'XeMay'
       làm mất phân loại phụ tùng (trigger tương thích phụ tùng cần đúng loại).
   ---------------------------------------------------------------------------- */
UPDATE dbo.SANPHAM
SET LoaiSanPham = 'PhuTung'
WHERE LoaiSanPham IN ('PhuKien', 'PhuTungXeMay', 'Part', 'Accessory', 'SparePart', N'PhụTùng', N'PhụKiện');
GO
UPDATE dbo.SANPHAM
SET LoaiSanPham = 'XeMay'
WHERE LoaiSanPham NOT IN ('XeMay', 'PhuTung');
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_SANPHAM_LoaiSanPham')
    ALTER TABLE dbo.SANPHAM WITH CHECK ADD CONSTRAINT CK_SANPHAM_LoaiSanPham
        CHECK (LoaiSanPham IN ('XeMay', 'PhuTung'));
GO

/* ----------------------------------------------------------------------------
   §5. Sửa trigger đồng bộ tồn kho.
       Lỗi cũ: khi xóa biến thể CUỐI CÙNG, mệnh đề WHERE EXISTS(biến thể) không khớp
       nên SANPHAM.SoLuongTon giữ giá trị tồn cũ (tồn ảo). Bỏ guard để luôn tính lại
       SUM biến thể (=0 khi không còn biến thể nào).
   ---------------------------------------------------------------------------- */
CREATE OR ALTER TRIGGER [dbo].[trg_BIENSANPHAM_Sync_SoLuongTon_SANPHAM]
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
    ) x;
END;
GO

/* ----------------------------------------------------------------------------
   §6. Dọn dữ liệu voucher bị ghi nhận trùng do bug double-count
       (mỗi đơn dùng voucher trước đây sinh 2 bản ghi VOUCHER_NGUOIDUNG 'Used').
       Giữ 1 bản 'Used' cho mỗi (MaVoucher, MaNguoiDung, MaDonHang), các bản dư
       chuyển 'Cancelled'; rồi tính lại VOUCHER.SoLanDaDung theo số 'Used' thực tế.
   ---------------------------------------------------------------------------- */
;WITH dups AS
(
    SELECT MaVoucherNguoiDung,
           ROW_NUMBER() OVER
           (
               PARTITION BY MaVoucher, MaNguoiDung, MaDonHang
               ORDER BY MaVoucherNguoiDung
           ) AS rn
    FROM dbo.VOUCHER_NGUOIDUNG
    WHERE TrangThai = 'Used' AND MaDonHang IS NOT NULL
)
UPDATE vnd
SET TrangThai = 'Cancelled'
FROM dbo.VOUCHER_NGUOIDUNG vnd
INNER JOIN dups ON dups.MaVoucherNguoiDung = vnd.MaVoucherNguoiDung
WHERE dups.rn > 1;
GO

UPDATE v
SET SoLanDaDung = ISNULL(u.Cnt, 0),
    NgayCapNhat = SYSDATETIME()
FROM dbo.VOUCHER v
OUTER APPLY
(
    SELECT COUNT(*) AS Cnt
    FROM dbo.VOUCHER_NGUOIDUNG vnd
    WHERE vnd.MaVoucher = v.MaVoucher
      AND vnd.TrangThai = 'Used'
) u;
GO

PRINT N'20260604_BusinessLogicAudit_Fixes.sql applied successfully.';
GO

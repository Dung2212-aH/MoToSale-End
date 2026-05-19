-- Add NgayCapNhat column to DANHGIASANPHAM if it does not exist
IF COL_LENGTH('dbo.DANHGIASANPHAM', 'NgayCapNhat') IS NULL
BEGIN
    ALTER TABLE dbo.DANHGIASANPHAM
        ADD NgayCapNhat datetime2(0) NOT NULL
            CONSTRAINT DF_DANHGIA_NgayCapNhat DEFAULT (sysutcdatetime());
END
GO

-- Backfill NgayCapNhat from NgayTao for existing rows (separate batch so column is resolved)
UPDATE dbo.DANHGIASANPHAM
    SET NgayCapNhat = NgayTao
    WHERE NgayCapNhat = '1900-01-01';
GO

-- Add unique index on (MaNguoiDung, MaSanPham) if it does not exist and no duplicates
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_DANHGIA_User_Product'
      AND object_id = OBJECT_ID('dbo.DANHGIASANPHAM')
)
AND NOT EXISTS (
    SELECT 1
    FROM dbo.DANHGIASANPHAM
    GROUP BY MaNguoiDung, MaSanPham
    HAVING COUNT(*) > 1
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DANHGIA_User_Product
        ON dbo.DANHGIASANPHAM (MaNguoiDung, MaSanPham);
END
GO

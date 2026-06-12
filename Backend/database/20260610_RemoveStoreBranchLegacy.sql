/*
    Don du lieu cua hang/chi nhanh legacy cho he thong 1 cua hang.

    Chay sau 20260610_InventoryLedgerMigration.sql.
    Script nay khong xoa nghiep vu; chi bo chieu MaCuaHang ra khoi cac bang van hanh
    va xoa bang CUAHANG neu khong con khoa ngoai nao phu thuoc.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

BEGIN TRY
BEGIN TRANSACTION;

DECLARE @sql NVARCHAR(MAX) = N'';

;WITH target_columns AS
(
    SELECT OBJECT_ID(N'dbo.DONNHAPHANG') AS object_id, N'DONNHAPHANG' AS table_name, N'MaCuaHang' AS column_name UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUNHAPKHO'), N'PHIEUNHAPKHO', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUSUACHUA'), N'PHIEUSUACHUA', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.CHAMCONG'), N'CHAMCONG', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUTRAHANG'), N'PHIEUTRAHANG', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.CALAMVIEC'), N'CALAMVIEC', N'MaCuaHang'
)
SELECT @sql += N'ALTER TABLE dbo.' + QUOTENAME(tc.table_name) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM target_columns tc
INNER JOIN sys.foreign_key_columns fkc ON fkc.parent_object_id = tc.object_id
INNER JOIN sys.foreign_keys fk ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON c.object_id = tc.object_id AND c.column_id = fkc.parent_column_id
WHERE tc.object_id IS NOT NULL
  AND c.name = tc.column_name;

EXEC sp_executesql @sql;

SET @sql = N'';
;WITH target_columns AS
(
    SELECT OBJECT_ID(N'dbo.DONNHAPHANG') AS object_id, N'DONNHAPHANG' AS table_name, N'MaCuaHang' AS column_name UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUNHAPKHO'), N'PHIEUNHAPKHO', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUSUACHUA'), N'PHIEUSUACHUA', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.CHAMCONG'), N'CHAMCONG', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.PHIEUTRAHANG'), N'PHIEUTRAHANG', N'MaCuaHang' UNION ALL
    SELECT OBJECT_ID(N'dbo.CALAMVIEC'), N'CALAMVIEC', N'MaCuaHang'
)
SELECT @sql += N'ALTER TABLE dbo.' + QUOTENAME(tc.table_name) + N' DROP COLUMN ' + QUOTENAME(tc.column_name) + N';' + CHAR(13)
FROM target_columns tc
WHERE tc.object_id IS NOT NULL
  AND COL_LENGTH(N'dbo.' + tc.table_name, tc.column_name) IS NOT NULL;

EXEC sp_executesql @sql;

IF OBJECT_ID(N'dbo.CUAHANG', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1
       FROM sys.foreign_keys
       WHERE referenced_object_id = OBJECT_ID(N'dbo.CUAHANG')
   )
BEGIN
    DROP TABLE dbo.CUAHANG;
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

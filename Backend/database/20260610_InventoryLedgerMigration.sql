/*
    Chuyen quan ly ton kho sang mo hinh so kho cho 1 cua hang.

    Nguyen tac nghiep vu:
    - TONKHO_HIENTAI.SoLuongThucTe la ton vat ly cua showroom hien tai.
    - TONKHO_GIUCHO la ton da giu cho don hang chua hoan tat.
    - Ton kha dung = SoLuongThucTe - tong giu cho Active con han.
    - Moi lan nhap/xuat/dieu chinh/ban/hoan phai ghi 1 dong TONKHO_BIENDONG.
    - SANPHAM.SoLuongTon va BIENSANPHAM.SoLuongTon chi con la cot tuong thich doc cu,
      duoc dong bo tu TONKHO_HIENTAI, khong con la nguon su that.
    - Khong luu MaCuaHang/MaChiNhanh trong ton kho vi he thong chi quan ly 1 cua hang.
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.v_TONKHO_KHADUNG', N'V') IS NOT NULL
    DROP VIEW dbo.v_TONKHO_KHADUNG;
GO

BEGIN TRY
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.TONKHO_HIENTAI', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TONKHO_HIENTAI
    (
        MaTonKho INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TONKHO_HIENTAI PRIMARY KEY,
        MaSanPham INT NOT NULL,
        MaBienSanPham INT NULL,
        SoLuongThucTe INT NOT NULL CONSTRAINT DF_TONKHO_HIENTAI_SoLuongThucTe DEFAULT (0),
        MucCanhBaoTonThap INT NOT NULL CONSTRAINT DF_TONKHO_HIENTAI_MucCanhBao DEFAULT (5),
        NgayTao DATETIME2(0) NOT NULL CONSTRAINT DF_TONKHO_HIENTAI_NgayTao DEFAULT SYSUTCDATETIME(),
        NgayCapNhat DATETIME2(0) NOT NULL CONSTRAINT DF_TONKHO_HIENTAI_NgayCapNhat DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TONKHO_HIENTAI_SANPHAM FOREIGN KEY (MaSanPham) REFERENCES dbo.SANPHAM(MaSanPham),
        CONSTRAINT FK_TONKHO_HIENTAI_BIENSANPHAM FOREIGN KEY (MaBienSanPham) REFERENCES dbo.BIENSANPHAM(MaBienSanPham),
        CONSTRAINT CK_TONKHO_HIENTAI_SoLuong CHECK (SoLuongThucTe >= 0 AND MucCanhBaoTonThap >= 0)
    );
END;

IF COL_LENGTH(N'dbo.TONKHO_HIENTAI', N'MaCuaHang') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TONKHO_HIENTAI_BienThe' AND object_id = OBJECT_ID(N'dbo.TONKHO_HIENTAI'))
        DROP INDEX UX_TONKHO_HIENTAI_BienThe ON dbo.TONKHO_HIENTAI;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TONKHO_HIENTAI_SanPhamGoc' AND object_id = OBJECT_ID(N'dbo.TONKHO_HIENTAI'))
        DROP INDEX UX_TONKHO_HIENTAI_SanPhamGoc ON dbo.TONKHO_HIENTAI;
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TONKHO_HIENTAI_CUAHANG')
        ALTER TABLE dbo.TONKHO_HIENTAI DROP CONSTRAINT FK_TONKHO_HIENTAI_CUAHANG;

    ;WITH g AS
    (
        SELECT
            MaTonKho,
            SUM(SoLuongThucTe) OVER (PARTITION BY MaSanPham, ISNULL(MaBienSanPham, -1)) AS TongTon,
            ROW_NUMBER() OVER (PARTITION BY MaSanPham, ISNULL(MaBienSanPham, -1) ORDER BY MaTonKho) AS rn
        FROM dbo.TONKHO_HIENTAI
    )
    UPDATE tk
    SET SoLuongThucTe = g.TongTon, NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.TONKHO_HIENTAI tk
    INNER JOIN g ON g.MaTonKho = tk.MaTonKho
    WHERE g.rn = 1;

    ;WITH d AS
    (
        SELECT MaTonKho, ROW_NUMBER() OVER (PARTITION BY MaSanPham, ISNULL(MaBienSanPham, -1) ORDER BY MaTonKho) AS rn
        FROM dbo.TONKHO_HIENTAI
    )
    DELETE tk
    FROM dbo.TONKHO_HIENTAI tk
    INNER JOIN d ON d.MaTonKho = tk.MaTonKho
    WHERE d.rn > 1;

    ALTER TABLE dbo.TONKHO_HIENTAI DROP COLUMN MaCuaHang;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TONKHO_HIENTAI_BienThe' AND object_id = OBJECT_ID(N'dbo.TONKHO_HIENTAI'))
    CREATE UNIQUE INDEX UX_TONKHO_HIENTAI_BienThe
        ON dbo.TONKHO_HIENTAI(MaSanPham, MaBienSanPham)
        WHERE MaBienSanPham IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TONKHO_HIENTAI_SanPhamGoc' AND object_id = OBJECT_ID(N'dbo.TONKHO_HIENTAI'))
    CREATE UNIQUE INDEX UX_TONKHO_HIENTAI_SanPhamGoc
        ON dbo.TONKHO_HIENTAI(MaSanPham)
        WHERE MaBienSanPham IS NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TONKHO_HIENTAI_SanPham' AND object_id = OBJECT_ID(N'dbo.TONKHO_HIENTAI'))
    CREATE INDEX IX_TONKHO_HIENTAI_SanPham
        ON dbo.TONKHO_HIENTAI(MaSanPham, MaBienSanPham);

IF OBJECT_ID(N'dbo.TONKHO_BIENDONG', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TONKHO_BIENDONG
    (
        MaBienDongTonKho INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TONKHO_BIENDONG PRIMARY KEY,
        MaSanPham INT NOT NULL,
        MaBienSanPham INT NULL,
        LoaiBienDong VARCHAR(30) NOT NULL,
        SoLuongThayDoi INT NOT NULL,
        TonSau INT NOT NULL,
        LoaiThamChieu NVARCHAR(80) NULL,
        MaThamChieu INT NULL,
        LyDo NVARCHAR(500) NULL,
        MaNguoiThucHien INT NULL,
        ThoiDiem DATETIME2(0) NOT NULL,
        NgayTao DATETIME2(0) NOT NULL CONSTRAINT DF_TONKHO_BIENDONG_NgayTao DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TONKHO_BIENDONG_SANPHAM FOREIGN KEY (MaSanPham) REFERENCES dbo.SANPHAM(MaSanPham),
        CONSTRAINT FK_TONKHO_BIENDONG_BIENSANPHAM FOREIGN KEY (MaBienSanPham) REFERENCES dbo.BIENSANPHAM(MaBienSanPham),
        CONSTRAINT CK_TONKHO_BIENDONG_SoLuong CHECK (SoLuongThayDoi <> 0 AND TonSau >= 0)
    );
END;

IF COL_LENGTH(N'dbo.TONKHO_BIENDONG', N'MaCuaHang') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TONKHO_BIENDONG_DoiTuong_ThoiDiem' AND object_id = OBJECT_ID(N'dbo.TONKHO_BIENDONG'))
        DROP INDEX IX_TONKHO_BIENDONG_DoiTuong_ThoiDiem ON dbo.TONKHO_BIENDONG;
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TONKHO_BIENDONG_CUAHANG')
        ALTER TABLE dbo.TONKHO_BIENDONG DROP CONSTRAINT FK_TONKHO_BIENDONG_CUAHANG;
    ALTER TABLE dbo.TONKHO_BIENDONG DROP COLUMN MaCuaHang;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TONKHO_BIENDONG_DoiTuong_ThoiDiem' AND object_id = OBJECT_ID(N'dbo.TONKHO_BIENDONG'))
    CREATE INDEX IX_TONKHO_BIENDONG_DoiTuong_ThoiDiem
        ON dbo.TONKHO_BIENDONG(MaSanPham, MaBienSanPham, ThoiDiem DESC);

;WITH TonCu AS
(
    SELECT
        sp.MaSanPham,
        CAST(NULL AS INT) AS MaBienSanPham,
        sp.SoLuongTon AS SoLuongThucTe,
        CAST(5 AS INT) AS MucCanhBaoTonThap
    FROM dbo.SANPHAM sp
    WHERE NOT EXISTS (SELECT 1 FROM dbo.BIENSANPHAM bt WHERE bt.MaSanPham = sp.MaSanPham)

    UNION ALL

    SELECT
        bt.MaSanPham,
        bt.MaBienSanPham,
        ISNULL(bt.SoLuongTon, 0) AS SoLuongThucTe,
        CAST(CASE WHEN ISNULL(bt.GiaGhiDe, sp.GiaGoc) > 10000000 THEN 2 ELSE 5 END AS INT) AS MucCanhBaoTonThap
    FROM dbo.BIENSANPHAM bt
    INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = bt.MaSanPham
)
MERGE dbo.TONKHO_HIENTAI AS target
USING TonCu AS source
ON target.MaSanPham = source.MaSanPham
   AND ISNULL(target.MaBienSanPham, -1) = ISNULL(source.MaBienSanPham, -1)
WHEN MATCHED THEN
    UPDATE SET
        SoLuongThucTe = source.SoLuongThucTe,
        MucCanhBaoTonThap = source.MucCanhBaoTonThap,
        NgayCapNhat = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (MaSanPham, MaBienSanPham, SoLuongThucTe, MucCanhBaoTonThap, NgayTao, NgayCapNhat)
    VALUES (source.MaSanPham, source.MaBienSanPham, source.SoLuongThucTe, source.MucCanhBaoTonThap, SYSUTCDATETIME(), SYSUTCDATETIME());

INSERT INTO dbo.TONKHO_BIENDONG
    (MaSanPham, MaBienSanPham, LoaiBienDong, SoLuongThayDoi, TonSau, LoaiThamChieu, MaThamChieu, LyDo, ThoiDiem, NgayTao)
SELECT
    tk.MaSanPham,
    tk.MaBienSanPham,
    'TonDauKy',
    tk.SoLuongThucTe,
    tk.SoLuongThucTe,
    N'MigrateTonCu',
    tk.MaTonKho,
    N'Migrate ton cu tu SANPHAM/BIENSANPHAM sang TONKHO_HIENTAI',
    SYSUTCDATETIME(),
    SYSUTCDATETIME()
FROM dbo.TONKHO_HIENTAI tk
WHERE tk.SoLuongThucTe > 0
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.TONKHO_BIENDONG bd
      WHERE bd.LoaiBienDong = 'TonDauKy'
        AND bd.LoaiThamChieu = N'MigrateTonCu'
        AND bd.MaSanPham = tk.MaSanPham
        AND ISNULL(bd.MaBienSanPham, -1) = ISNULL(tk.MaBienSanPham, -1)
  );

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_TONKHO_DongBoCotCu
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE bt
    SET
        SoLuongTon = ISNULL(x.SoLuongThucTe, 0),
        NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.BIENSANPHAM bt
    OUTER APPLY
    (
        SELECT SUM(tk.SoLuongThucTe) AS SoLuongThucTe
        FROM dbo.TONKHO_HIENTAI tk
        WHERE tk.MaBienSanPham = bt.MaBienSanPham
    ) x;

    UPDATE sp
    SET
        SoLuongTon = ISNULL(x.SoLuongThucTe, 0),
        NgayCapNhat = SYSUTCDATETIME()
    FROM dbo.SANPHAM sp
    OUTER APPLY
    (
        SELECT SUM(tk.SoLuongThucTe) AS SoLuongThucTe
        FROM dbo.TONKHO_HIENTAI tk
        WHERE tk.MaSanPham = sp.MaSanPham
    ) x;
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_TONKHO_ApDungBienDong
    @MaSanPham INT,
    @MaBienSanPham INT = NULL,
    @LoaiBienDong VARCHAR(30),
    @SoLuongThayDoi INT,
    @LyDo NVARCHAR(500) = NULL,
    @LoaiThamChieu NVARCHAR(80) = NULL,
    @MaThamChieu INT = NULL,
    @MaNguoiThucHien INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoLuongThayDoi = 0
        THROW 51000, N'So luong thay doi phai khac 0.', 1;

    IF @MaBienSanPham IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.BIENSANPHAM WHERE MaBienSanPham = @MaBienSanPham AND MaSanPham = @MaSanPham)
        THROW 51001, N'Bien the khong thuoc san pham.', 1;

    IF @MaBienSanPham IS NULL
       AND EXISTS (SELECT 1 FROM dbo.BIENSANPHAM WHERE MaSanPham = @MaSanPham)
        THROW 51002, N'San pham co bien the phai nhap/xuat theo bien the.', 1;

    DECLARE @TonTruoc INT;
    DECLARE @TonSau INT;

    BEGIN TRANSACTION;

    SELECT @TonTruoc = SoLuongThucTe
    FROM dbo.TONKHO_HIENTAI WITH (UPDLOCK, HOLDLOCK)
    WHERE MaSanPham = @MaSanPham
      AND ISNULL(MaBienSanPham, -1) = ISNULL(@MaBienSanPham, -1);

    IF @TonTruoc IS NULL
    BEGIN
        INSERT INTO dbo.TONKHO_HIENTAI
            (MaSanPham, MaBienSanPham, SoLuongThucTe, MucCanhBaoTonThap, NgayTao, NgayCapNhat)
        VALUES
            (@MaSanPham, @MaBienSanPham, 0, 5, SYSUTCDATETIME(), SYSUTCDATETIME());
        SET @TonTruoc = 0;
    END;

    SET @TonSau = @TonTruoc + @SoLuongThayDoi;
    IF @TonSau < 0
        THROW 51003, N'Ton kho thuc te khong du.', 1;

    UPDATE dbo.TONKHO_HIENTAI
    SET SoLuongThucTe = @TonSau,
        NgayCapNhat = SYSUTCDATETIME()
    WHERE MaSanPham = @MaSanPham
      AND ISNULL(MaBienSanPham, -1) = ISNULL(@MaBienSanPham, -1);

    INSERT INTO dbo.TONKHO_BIENDONG
        (MaSanPham, MaBienSanPham, LoaiBienDong, SoLuongThayDoi, TonSau, LoaiThamChieu, MaThamChieu, LyDo, MaNguoiThucHien, ThoiDiem, NgayTao)
    VALUES
        (@MaSanPham, @MaBienSanPham, @LoaiBienDong, @SoLuongThayDoi, @TonSau, @LoaiThamChieu, @MaThamChieu, @LyDo, @MaNguoiThucHien, SYSUTCDATETIME(), SYSUTCDATETIME());

    EXEC dbo.sp_TONKHO_DongBoCotCu;

    COMMIT TRANSACTION;
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SANPHAM_DongBoSoLuongTon
    @MaSanPham INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_TONKHO_DongBoCotCu;
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_SANPHAM_DongBoTatCaSoLuongTon
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_TONKHO_DongBoCotCu;
END;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER VIEW dbo.v_TONKHO_KHADUNG
AS
SELECT
    tk.MaSanPham,
    sp.MaSanPhamKinhDoanh,
    tk.MaBienSanPham,
    bt.SKU,
    sp.TenSanPham,
    bt.TenBienThe,
    tk.SoLuongThucTe AS TonKhoThucTe,
    ISNULL(gc.SoLuongDangGiu, 0) AS SoLuongDangGiu,
    tk.SoLuongThucTe - ISNULL(gc.SoLuongDangGiu, 0) AS TonKhoKhaDung
FROM dbo.TONKHO_HIENTAI tk
INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = tk.MaSanPham
LEFT JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = tk.MaBienSanPham
OUTER APPLY
(
    SELECT SUM(g.SoLuong) AS SoLuongDangGiu
    FROM dbo.TONKHO_GIUCHO g
    WHERE g.MaSanPham = tk.MaSanPham
      AND ISNULL(g.MaBienSanPham, -1) = ISNULL(tk.MaBienSanPham, -1)
      AND g.TrangThai = 'Active'
      AND g.HetHanLuc > SYSDATETIME()
) gc;
GO

EXEC dbo.sp_TONKHO_DongBoCotCu;
GO

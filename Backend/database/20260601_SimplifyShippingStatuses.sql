SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

UPDATE dbo.DONHANG
SET TrangThaiVanChuyen = CASE
    WHEN TrangThaiVanChuyen IN ('Delivered', 'PickedUp') THEN 'Delivered'
    WHEN TrangThaiVanChuyen = 'Shipping' THEN 'Shipping'
    ELSE 'Preparing'
END
WHERE TrangThaiVanChuyen NOT IN ('Preparing', 'Shipping', 'Delivered');
GO

IF OBJECT_ID(N'dbo.DF_DONHANG_TrangThaiVanChuyen', N'D') IS NOT NULL
BEGIN
    ALTER TABLE dbo.DONHANG DROP CONSTRAINT DF_DONHANG_TrangThaiVanChuyen;
END;
GO

IF OBJECT_ID(N'dbo.CK_DONHANG_TrangThaiVanChuyen', N'C') IS NOT NULL
BEGIN
    ALTER TABLE dbo.DONHANG DROP CONSTRAINT CK_DONHANG_TrangThaiVanChuyen;
END;
GO

ALTER TABLE dbo.DONHANG
ADD CONSTRAINT DF_DONHANG_TrangThaiVanChuyen DEFAULT ('Preparing') FOR TrangThaiVanChuyen;
GO

ALTER TABLE dbo.DONHANG WITH CHECK
ADD CONSTRAINT CK_DONHANG_TrangThaiVanChuyen
CHECK (TrangThaiVanChuyen IN ('Preparing', 'Shipping', 'Delivered'));
GO

ALTER TABLE dbo.DONHANG CHECK CONSTRAINT CK_DONHANG_TrangThaiVanChuyen;
GO

IF OBJECT_ID(N'[dbo].[MATKHAU_DATLAI]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MATKHAU_DATLAI](
        [MaToken] [int] IDENTITY(1,1) NOT NULL,
        [MaNguoiDung] [int] NOT NULL,
        [TokenHash] [varchar](128) NOT NULL,
        [HetHanLuc] [datetime2](0) NOT NULL,
        [DaDungLuc] [datetime2](0) NULL,
        [NgayTao] [datetime2](0) NOT NULL,
        CONSTRAINT [PK_MATKHAU_DATLAI] PRIMARY KEY CLUSTERED ([MaToken] ASC),
        CONSTRAINT [FK_MATKHAU_DATLAI_NGUOIDUNG] FOREIGN KEY([MaNguoiDung])
            REFERENCES [dbo].[NGUOIDUNG] ([MaNguoiDung])
            ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_MATKHAU_DATLAI_TokenHash]
        ON [dbo].[MATKHAU_DATLAI] ([TokenHash] ASC);

    CREATE NONCLUSTERED INDEX [IX_MATKHAU_DATLAI_NguoiDung_HetHan]
        ON [dbo].[MATKHAU_DATLAI] ([MaNguoiDung] ASC, [HetHanLuc] ASC);

    ALTER TABLE [dbo].[MATKHAU_DATLAI]
        ADD CONSTRAINT [DF_MATKHAU_DATLAI_NgayTao] DEFAULT (sysutcdatetime()) FOR [NgayTao];
END
GO

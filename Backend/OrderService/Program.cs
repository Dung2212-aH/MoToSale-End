using System.Text;
using OrderService.Data;
using OrderService.Repositories;
using OrderService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey chua duoc cau hinh.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await EnsureInstallmentAndConfigAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled: services run on HTTP in dev mode

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "OrderService" }));

app.Map("/error", () => Results.Problem());

app.Run();

static async Task EnsureInstallmentAndConfigAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    // Bring the schema in line with the entity model BEFORE the app accepts any traffic.
    // If this fails we abort startup — better than serving requests that throw 500s on every read.
    const string sql = @"
IF OBJECT_ID(N'dbo.HOSO_TRAGOP', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HOSO_TRAGOP(
        MaHoSoTraGop INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaDonHang INT NOT NULL,
        TienTraTruoc DECIMAL(18,2) NOT NULL,
        SoTienGoc DECIMAL(18,2) NOT NULL,
        SoKy INT NOT NULL,
        LaiSuatNam DECIMAL(9,4) NOT NULL,
        TongTienLai DECIMAL(18,2) NOT NULL,
        TongPhaiTra DECIMAL(18,2) NOT NULL,
        TrangThai VARCHAR(20) NOT NULL,
        NgayTao DATETIME2(0) NOT NULL,
        NgayCapNhat DATETIME2(0) NOT NULL,
        HoTenNguoiVay NVARCHAR(150) NOT NULL DEFAULT(N''''),
        SoCCCD VARCHAR(20) NOT NULL DEFAULT(''''),
        NgheNghiep NVARCHAR(100) NULL,
        ThuNhapHangThang DECIMAL(18,2) NULL,
        CONSTRAINT FK_HOSO_TRAGOP_DONHANG FOREIGN KEY (MaDonHang) REFERENCES dbo.DONHANG(MaDonHang),
        CONSTRAINT UQ_HOSO_TRAGOP_DONHANG UNIQUE (MaDonHang)
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.HOSO_TRAGOP','HoTenNguoiVay') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD HoTenNguoiVay NVARCHAR(150) NOT NULL CONSTRAINT DF_HOSO_TRAGOP_HoTen DEFAULT(N'''');
    IF COL_LENGTH('dbo.HOSO_TRAGOP','SoCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD SoCCCD VARCHAR(20) NOT NULL CONSTRAINT DF_HOSO_TRAGOP_CCCD DEFAULT('''');
    IF COL_LENGTH('dbo.HOSO_TRAGOP','NgheNghiep') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgheNghiep NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','ThuNhapHangThang') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD ThuNhapHangThang DECIMAL(18,2) NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','NgaySinh') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgaySinh DATE NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','SoDienThoai') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD SoDienThoai VARCHAR(20) NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','DiaChiThuongTru') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD DiaChiThuongTru NVARCHAR(255) NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','TenCongTy') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD TenCongTy NVARCHAR(150) NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','ThoiGianLamViecThang') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD ThoiGianLamViecThang INT NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','NgayCapCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgayCapCCCD DATE NULL;
    IF COL_LENGTH('dbo.HOSO_TRAGOP','NoiCapCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NoiCapCCCD NVARCHAR(150) NULL;
END;

IF OBJECT_ID(N'dbo.KY_TRAGOP', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KY_TRAGOP(
        MaKyTraGop INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaHoSoTraGop INT NOT NULL,
        KyThu INT NOT NULL,
        NgayDenHan DATETIME2(0) NOT NULL,
        SoTienGoc DECIMAL(18,2) NOT NULL,
        SoTienLai DECIMAL(18,2) NOT NULL,
        TongTien DECIMAL(18,2) NOT NULL,
        TrangThai VARCHAR(20) NOT NULL,
        NgayThanhToan DATETIME2(0) NULL,
        NgayTao DATETIME2(0) NOT NULL,
        NgayCapNhat DATETIME2(0) NOT NULL,
        CONSTRAINT FK_KY_TRAGOP_HOSO FOREIGN KEY (MaHoSoTraGop) REFERENCES dbo.HOSO_TRAGOP(MaHoSoTraGop) ON DELETE CASCADE
    );
    CREATE INDEX IX_KY_TRAGOP_HoSo ON dbo.KY_TRAGOP(MaHoSoTraGop, KyThu);
END;

IF OBJECT_ID(N'dbo.YEUCAU_HOANTIEN', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.YEUCAU_HOANTIEN(
        MaYeuCauHoanTien INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaDonHang INT NOT NULL,
        SoTien DECIMAL(18,2) NOT NULL,
        TenNganHang NVARCHAR(100) NOT NULL,
        SoTaiKhoan VARCHAR(20) NOT NULL,
        ChuTaiKhoan NVARCHAR(150) NOT NULL,
        LyDo NVARCHAR(500) NULL,
        TrangThai VARCHAR(20) NOT NULL,
        NgayTao DATETIME2(0) NOT NULL,
        NgayHoanTat DATETIME2(0) NULL,
        GhiChuAdmin NVARCHAR(500) NULL,
        MaGiaoDichHoan NVARCHAR(120) NULL,
        CONSTRAINT FK_YEUCAU_HOANTIEN_DONHANG FOREIGN KEY (MaDonHang) REFERENCES dbo.DONHANG(MaDonHang)
    );
    CREATE INDEX IX_YEUCAU_HOANTIEN_DonHang ON dbo.YEUCAU_HOANTIEN(MaDonHang, TrangThai);
END;

-- Drop the legacy 'Failed' value from CK_DONHANG_PaymentStatus. Failed is a per-transaction
-- concept and never makes sense at the order level (an order is either Unpaid, PartiallyPaid,
-- Paid, Refunded, or Cancelled).
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_DONHANG_PaymentStatus')
BEGIN
    UPDATE dbo.DONHANG SET TrangThaiThanhToan = 'Unpaid' WHERE TrangThaiThanhToan = 'Failed';
    ALTER TABLE dbo.DONHANG DROP CONSTRAINT CK_DONHANG_PaymentStatus;
END;
ALTER TABLE dbo.DONHANG WITH CHECK ADD CONSTRAINT CK_DONHANG_PaymentStatus
    CHECK (TrangThaiThanhToan IN ('Unpaid','PartiallyPaid','Paid','Refunded','Cancelled'));

-- Simplified order status: AwaitingPayment / Confirmed / Cancelled only. Shipping progress is
-- tracked separately in TrangThaiVanChuyen. Migrate any legacy in-flight statuses into Confirmed
-- (they were all post-confirmation shipping stages anyway).
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_DONHANG_OrderStatus')
BEGIN
    UPDATE dbo.DONHANG SET TrangThaiDonHang = 'Confirmed'
        WHERE TrangThaiDonHang IN ('Processing','Shipping','Delivered','Completed');
    UPDATE dbo.DONHANG SET TrangThaiDonHang = 'AwaitingPayment'
        WHERE TrangThaiDonHang IN ('Pending','Checkout');
    ALTER TABLE dbo.DONHANG DROP CONSTRAINT CK_DONHANG_OrderStatus;
END;
ALTER TABLE dbo.DONHANG WITH CHECK ADD CONSTRAINT CK_DONHANG_OrderStatus
    CHECK (TrangThaiDonHang IN ('AwaitingPayment','Confirmed','Cancelled'));

IF OBJECT_ID(N'dbo.HETHONG_CAUHINH', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HETHONG_CAUHINH(
        [Key] NVARCHAR(100) NOT NULL PRIMARY KEY,
        [Value] NVARCHAR(MAX) NULL,
        MoTa NVARCHAR(500) NULL,
        NgayCapNhat DATETIME2(0) NOT NULL
    );
END;

MERGE dbo.HETHONG_CAUHINH AS t
USING (VALUES
    (N'BankBin', N'Ma ngan hang / BIN nhan chuyen khoan (VietQR)'),
    (N'BankAccountNo', N'So tai khoan nhan chuyen khoan'),
    (N'BankAccountName', N'Ten chu tai khoan'),
    (N'InstallmentAnnualRate', N'Lai suat tra gop/nam (%)'),
    (N'InstallmentMinDownPaymentPercent', N'Ty le tra truoc toi thieu khi tra gop (%)'),
    (N'InstallmentAllowedTerms', N'Cac ky han tra gop cho phep (thang, cach nhau dau phay)'),
    (N'PaymentHoldMinutes', N'Thoi gian giu cho ton kho cho thanh toan (phut)'),
    (N'DepositMinPercent', N'Ty le dat coc toi thieu cho don Dat coc (%)')
) AS s([Key], MoTa)
ON t.[Key] = s.[Key]
WHEN NOT MATCHED THEN
    INSERT ([Key], [Value], MoTa, NgayCapNhat)
    VALUES (s.[Key],
        CASE s.[Key]
            WHEN N'InstallmentAnnualRate' THEN N'12'
            WHEN N'InstallmentMinDownPaymentPercent' THEN N'30'
            WHEN N'InstallmentAllowedTerms' THEN N'6,9,12'
            WHEN N'PaymentHoldMinutes' THEN N'1440'
            WHEN N'DepositMinPercent' THEN N'20'
            ELSE NULL
        END,
        s.MoTa, SYSUTCDATETIME());";
    await db.Database.ExecuteSqlRawAsync(sql);
}

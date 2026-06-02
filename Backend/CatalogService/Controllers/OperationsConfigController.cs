using CatalogService.Data;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/operations")]
public class OperationsConfigController : ControllerBase
{
    private readonly CatalogDbContext _db;
    private readonly IAuditLogService _auditLog;

    public OperationsConfigController(CatalogDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        await EnsureTablesAsync();
        var rows = await _db.Database.SqlQueryRaw<WarehouseRow>(
            """
            SELECT MaKho, TenKho, LoaiKho, DiaChi, Hotline, DangHoatDong, NgayTao, NgayCapNhat
            FROM dbo.CUAHANG_KHO
            ORDER BY DangHoatDong DESC, MaKho
            """
        ).ToListAsync();

        return Ok(new { items = rows });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("warehouses")]
    public async Task<IActionResult> SaveWarehouse(WarehouseRequest request)
    {
        await EnsureTablesAsync();
        if (string.IsNullOrWhiteSpace(request.TenKho))
        {
            return BadRequest(new { message = "Ten kho/showroom la bat buoc." });
        }

        var now = DateTime.UtcNow;
        if (request.MaKho.HasValue)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE dbo.CUAHANG_KHO
                SET TenKho = {request.TenKho.Trim()}, LoaiKho = {NormalizeWarehouseType(request.LoaiKho)}, DiaChi = {TrimToNull(request.DiaChi)}, Hotline = {TrimToNull(request.Hotline)}, DangHoatDong = {request.DangHoatDong}, NgayCapNhat = {now}
                WHERE MaKho = {request.MaKho.Value}
                """);
            await _auditLog.WriteAsync(this, "Warehouse", request.MaKho.Value.ToString(), "Update", null, request);
            return Ok(new { id = request.MaKho.Value });
        }

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.CUAHANG_KHO (TenKho, LoaiKho, DiaChi, Hotline, DangHoatDong, NgayTao, NgayCapNhat)
            VALUES ({request.TenKho.Trim()}, {NormalizeWarehouseType(request.LoaiKho)}, {TrimToNull(request.DiaChi)}, {TrimToNull(request.Hotline)}, {request.DangHoatDong}, {now}, {now})
            """);
        await _auditLog.WriteAsync(this, "Warehouse", "New", "Create", null, request);
        return Ok(new { message = "Luu kho/showroom thanh cong." });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        await EnsureTablesAsync();
        var rows = await _db.Database.SqlQueryRaw<SettingRow>(
            "SELECT [Key], [Value], MoTa, NgayCapNhat FROM dbo.HETHONG_CAUHINH ORDER BY [Key]"
        ).ToListAsync();

        return Ok(new { items = rows });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings(SettingsRequest request)
    {
        await EnsureTablesAsync();
        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }

            await UpsertSettingAsync(item.Key.Trim(), item.Value, item.MoTa);
        }

        await _auditLog.WriteAsync(this, "SystemSettings", "All", "Update", null, request.Items);
        return Ok(new { message = "Luu cau hinh he thong thanh cong." });
    }

    private async Task UpsertSettingAsync(string key, string? value, string? description)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            MERGE dbo.HETHONG_CAUHINH AS target
            USING (SELECT {key} AS [Key]) AS source
            ON target.[Key] = source.[Key]
            WHEN MATCHED THEN
                UPDATE SET [Value] = {value}, MoTa = {description}, NgayCapNhat = SYSDATETIME()
            WHEN NOT MATCHED THEN
                INSERT ([Key], [Value], MoTa, NgayCapNhat)
                VALUES ({key}, {value}, {description}, SYSDATETIME());
            """);
    }

    private async Task EnsureTablesAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.CUAHANG_KHO', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.CUAHANG_KHO (
                    MaKho INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    TenKho NVARCHAR(200) NOT NULL,
                    LoaiKho VARCHAR(30) NOT NULL,
                    DiaChi NVARCHAR(500) NULL,
                    Hotline NVARCHAR(30) NULL,
                    DangHoatDong BIT NOT NULL,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM dbo.CUAHANG_KHO)
            BEGIN
                INSERT INTO dbo.CUAHANG_KHO (TenKho, LoaiKho, DiaChi, Hotline, DangHoatDong, NgayTao, NgayCapNhat)
                VALUES (N'Cửa hàng chính', 'StoreWarehouse', NULL, NULL, 1, SYSDATETIME(), SYSDATETIME());
            END;

            IF OBJECT_ID(N'dbo.HETHONG_CAUHINH', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HETHONG_CAUHINH (
                    [Key] NVARCHAR(100) NOT NULL PRIMARY KEY,
                    [Value] NVARCHAR(MAX) NULL,
                    MoTa NVARCHAR(500) NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM dbo.HETHONG_CAUHINH)
            BEGIN
                INSERT INTO dbo.HETHONG_CAUHINH ([Key], [Value], MoTa, NgayCapNhat)
                VALUES
                    (N'StoreName', N'MoToSale', N'Tên cửa hàng trên chứng từ', SYSDATETIME()),
                    (N'Hotline', NULL, N'Hotline cửa hàng', SYSDATETIME()),
                    (N'Address', NULL, N'Địa chỉ cửa hàng', SYSDATETIME()),
                    (N'DefaultLowStockThreshold', N'5', N'Ngưỡng tồn thấp mặc định', SYSDATETIME()),
                    (N'DepositPolicy', NULL, N'Chính sách đặt cọc', SYSDATETIME()),
                    (N'CancelPolicy', NULL, N'Chính sách hủy đơn', SYSDATETIME()),
                    (N'WarrantyPolicy', NULL, N'Chính sách bảo hành', SYSDATETIME()),
                    (N'DefaultShippingFee', N'0', N'Phí vận chuyển mặc định', SYSDATETIME());
            END;
            """);
    }

    private static string NormalizeWarehouseType(string? value)
    {
        return value?.Trim() switch
        {
            "Showroom" => "Showroom",
            "Warehouse" => "Warehouse",
            _ => "StoreWarehouse"
        };
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public class WarehouseRequest
{
    public int? MaKho { get; set; }
    public string TenKho { get; set; } = "";
    public string? LoaiKho { get; set; }
    public string? DiaChi { get; set; }
    public string? Hotline { get; set; }
    public bool DangHoatDong { get; set; } = true;
}

public class SettingsRequest
{
    public List<SettingItemRequest> Items { get; set; } = new();
}

public class SettingItemRequest
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? MoTa { get; set; }
}

public class WarehouseRow
{
    public int MaKho { get; set; }
    public string TenKho { get; set; } = "";
    public string LoaiKho { get; set; } = "";
    public string? DiaChi { get; set; }
    public string? Hotline { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class SettingRow
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? MoTa { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

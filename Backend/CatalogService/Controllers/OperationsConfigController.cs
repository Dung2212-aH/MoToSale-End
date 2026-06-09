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

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        await EnsureStoreTableAsync();
        var rows = await _db.Database.SqlQueryRaw<StoreRow>(
            "SELECT MaCuaHang, MaCuaHangKinhDoanh, TenCuaHang, LoaiCuaHang, DiaChi, SoDienThoai, DangHoatDong FROM dbo.CUAHANG ORDER BY MaCuaHang"
        ).ToListAsync();

        var items = rows.Select(r => new
        {
            id = r.MaCuaHang,
            maKho = r.MaCuaHang,
            code = r.MaCuaHangKinhDoanh,
            name = r.TenCuaHang,
            tenKho = r.TenCuaHang,
            type = r.LoaiCuaHang,
            loaiKho = r.LoaiCuaHang,
            addressLine = r.DiaChi,
            diaChi = r.DiaChi,
            phone = r.SoDienThoai,
            hotline = r.SoDienThoai,
            isActive = r.DangHoatDong,
            dangHoatDong = r.DangHoatDong
        });
        return Ok(new { items });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("warehouses")]
    public async Task<IActionResult> SaveWarehouse(WarehouseRequest request)
    {
        await EnsureStoreTableAsync();

        var name = request.Name ?? request.TenKho;
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Ten cua hang/kho la bat buoc." });
        }
        var type = request.Type ?? request.LoaiKho ?? "Showroom";
        var address = request.AddressLine ?? request.DiaChi;
        var phone = request.Phone ?? request.Hotline;
        var isActive = request.IsActive ?? request.DangHoatDong ?? true;
        var id = request.Id ?? request.MaKho ?? 0;

        if (id > 0)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE dbo.CUAHANG
                SET TenCuaHang = {name.Trim()}, LoaiCuaHang = {type}, DiaChi = {address}, SoDienThoai = {phone},
                    DangHoatDong = {isActive}, NgayCapNhat = SYSDATETIME()
                WHERE MaCuaHang = {id}
                """);
            await _auditLog.WriteAsync(this, "Store", id.ToString(), "Update", null, new { name, type });
            return Ok(new { id });
        }

        var code = $"KHO{DateTime.UtcNow:yyyyMMddHHmmss}";
        var newId = await _db.Database.SqlQueryRaw<int>($"""
            INSERT INTO dbo.CUAHANG (MaCuaHangKinhDoanh, TenCuaHang, LoaiCuaHang, DiaChi, SoDienThoai, DangHoatDong, NgayTao, NgayCapNhat)
            OUTPUT INSERTED.MaCuaHang AS Value
            VALUES ({code}, {name.Trim()}, {type}, {address}, {phone}, {isActive}, SYSDATETIME(), SYSDATETIME())
            """).FirstAsync();
        await _auditLog.WriteAsync(this, "Store", newId.ToString(), "Create", null, new { name, type });
        return Ok(new { id = newId });
    }

    private async Task EnsureStoreTableAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.CUAHANG', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.CUAHANG (
                    MaCuaHang INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaCuaHangKinhDoanh VARCHAR(40) NOT NULL,
                    TenCuaHang NVARCHAR(150) NOT NULL,
                    LoaiCuaHang VARCHAR(20) NOT NULL DEFAULT 'Showroom',
                    DiaChi NVARCHAR(255) NULL,
                    SoDienThoai VARCHAR(20) NULL,
                    DangHoatDong BIT NOT NULL DEFAULT 1,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
                INSERT INTO dbo.CUAHANG (MaCuaHangKinhDoanh, TenCuaHang, LoaiCuaHang, DiaChi, SoDienThoai, DangHoatDong, NgayTao, NgayCapNhat)
                VALUES ('KHO_CHINH', N'Cửa hàng chính', 'Showroom', NULL, NULL, 1, SYSDATETIME(), SYSDATETIME());
            END;
            """);
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
                    (N'DefaultLowStockThreshold', N'5', N'Ngưỡng tồn thấp mặc định', SYSDATETIME()),
                    (N'DepositPolicy', NULL, N'Chính sách đặt cọc', SYSDATETIME()),
                    (N'CancelPolicy', NULL, N'Chính sách hủy đơn', SYSDATETIME()),
                    (N'WarrantyPolicy', NULL, N'Chính sách bảo hành', SYSDATETIME()),
                    (N'DefaultShippingFee', N'0', N'Phí vận chuyển mặc định', SYSDATETIME());
            END;
            """);
    }

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

public class SettingRow
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? MoTa { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class WarehouseRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? AddressLine { get; set; }
    public string? Phone { get; set; }
    public bool? IsActive { get; set; }
    // Bi danh tieng Viet
    public int? MaKho { get; set; }
    public string? TenKho { get; set; }
    public string? LoaiKho { get; set; }
    public string? DiaChi { get; set; }
    public string? Hotline { get; set; }
    public bool? DangHoatDong { get; set; }
}

public class StoreRow
{
    public int MaCuaHang { get; set; }
    public string MaCuaHangKinhDoanh { get; set; } = "";
    public string TenCuaHang { get; set; } = "";
    public string LoaiCuaHang { get; set; } = "";
    public string? DiaChi { get; set; }
    public string? SoDienThoai { get; set; }
    public bool DangHoatDong { get; set; }
}

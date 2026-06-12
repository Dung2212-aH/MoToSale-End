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
    public IActionResult GetWarehouses()
    {
        return Ok(new { items = Array.Empty<object>() });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("warehouses")]
    public IActionResult SaveWarehouse(WarehouseRequest request)
    {
        return BadRequest(new { message = "He thong chi quan ly 1 cua hang, khong cau hinh kho/chi nhanh trong database." });
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
                    (N'DefaultLowStockThreshold', N'5', N'Nguong ton thap mac dinh', SYSDATETIME()),
                    (N'DepositPolicy', NULL, N'Chinh sach dat coc', SYSDATETIME()),
                    (N'CancelPolicy', NULL, N'Chinh sach huy don', SYSDATETIME()),
                    (N'WarrantyPolicy', NULL, N'Chinh sach bao hanh', SYSDATETIME()),
                    (N'DefaultShippingFee', N'0', N'Phi van chuyen mac dinh', SYSDATETIME());
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
    public int? MaKho { get; set; }
    public string? TenKho { get; set; }
    public string? LoaiKho { get; set; }
    public string? DiaChi { get; set; }
    public string? Hotline { get; set; }
    public bool? DangHoatDong { get; set; }
}

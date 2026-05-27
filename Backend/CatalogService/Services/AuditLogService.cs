using System.Security.Claims;
using System.Text.Json;
using CatalogService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services;

public interface IAuditLogService
{
    Task EnsureTableAsync();
    Task WriteAsync(ControllerBase controller, string entityType, string entityId, string action, object? oldValue = null, object? newValue = null, string? note = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly CatalogDbContext _db;

    public AuditLogService(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task EnsureTableAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.HE_THONG_NHATKY', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HE_THONG_NHATKY (
                    MaNhatKy BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    LoaiDoiTuong NVARCHAR(80) NOT NULL,
                    MaDoiTuong NVARCHAR(80) NOT NULL,
                    HanhDong NVARCHAR(40) NOT NULL,
                    GiaTriTruoc NVARCHAR(MAX) NULL,
                    GiaTriSau NVARCHAR(MAX) NULL,
                    MaNguoiThucHien INT NULL,
                    TenNguoiThucHien NVARCHAR(255) NULL,
                    GhiChu NVARCHAR(500) NULL,
                    DiaChiIp NVARCHAR(64) NULL,
                    UserAgent NVARCHAR(500) NULL,
                    ThoiGian DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_HE_THONG_NHATKY_Time
                    ON dbo.HE_THONG_NHATKY (ThoiGian DESC, MaNhatKy DESC);
                CREATE INDEX IX_HE_THONG_NHATKY_Target
                    ON dbo.HE_THONG_NHATKY (LoaiDoiTuong, MaDoiTuong, ThoiGian DESC);
                CREATE INDEX IX_HE_THONG_NHATKY_Actor
                    ON dbo.HE_THONG_NHATKY (MaNguoiThucHien, ThoiGian DESC);
            END;
            """);
    }

    public async Task WriteAsync(
        ControllerBase controller,
        string entityType,
        string entityId,
        string action,
        object? oldValue = null,
        object? newValue = null,
        string? note = null)
    {
        await EnsureTableAsync();

        var user = controller.User;
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        int? userId = int.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;
        var userName = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("name") ?? user.Identity?.Name;
        var ip = controller.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = controller.Request.Headers.UserAgent.ToString();
        var oldJson = oldValue is null ? null : JsonSerializer.Serialize(oldValue);
        var newJson = newValue is null ? null : JsonSerializer.Serialize(newValue);

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.HE_THONG_NHATKY
                (LoaiDoiTuong, MaDoiTuong, HanhDong, GiaTriTruoc, GiaTriSau, MaNguoiThucHien, TenNguoiThucHien, GhiChu, DiaChiIp, UserAgent, ThoiGian)
            VALUES
                ({entityType}, {entityId}, {action}, {oldJson}, {newJson}, {userId}, {userName}, {note}, {ip}, {userAgent}, SYSUTCDATETIME())
            """);
    }
}

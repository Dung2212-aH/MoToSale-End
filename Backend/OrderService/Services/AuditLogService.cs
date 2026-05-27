using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Services;

public interface IAuditLogService
{
    Task EnsureTableAsync();
    Task WriteAsync(ControllerBase controller, string entityType, string entityId, string action, object? oldValue = null, object? newValue = null, string? note = null);
}

public class AuditLogService : IAuditLogService
{
    private readonly OrderDbContext _db;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AuditLogService(OrderDbContext db)
    {
        _db = db;
    }

    public async Task EnsureTableAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[HE_THONG_NHATKY]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[HE_THONG_NHATKY](
                    [MaNhatKy] [bigint] IDENTITY(1,1) NOT NULL,
                    [LoaiDoiTuong] [nvarchar](100) NOT NULL,
                    [MaDoiTuong] [nvarchar](100) NOT NULL,
                    [HanhDong] [nvarchar](100) NOT NULL,
                    [GiaTriTruoc] [nvarchar](max) NULL,
                    [GiaTriSau] [nvarchar](max) NULL,
                    [MaNguoiThucHien] [int] NULL,
                    [TenNguoiThucHien] [nvarchar](200) NULL,
                    [GhiChu] [nvarchar](1000) NULL,
                    [DiaChiIp] [nvarchar](80) NULL,
                    [UserAgent] [nvarchar](500) NULL,
                    [ThoiGian] [datetime2](0) NOT NULL,
                    CONSTRAINT [PK_HE_THONG_NHATKY] PRIMARY KEY CLUSTERED ([MaNhatKy] ASC)
                );

                CREATE INDEX [IX_HE_THONG_NHATKY_Time] ON [dbo].[HE_THONG_NHATKY]([ThoiGian] DESC);
                CREATE INDEX [IX_HE_THONG_NHATKY_Target] ON [dbo].[HE_THONG_NHATKY]([LoaiDoiTuong], [MaDoiTuong]);
                CREATE INDEX [IX_HE_THONG_NHATKY_Actor] ON [dbo].[HE_THONG_NHATKY]([MaNguoiThucHien], [ThoiGian] DESC);
            END
            """);
    }

    public async Task WriteAsync(ControllerBase controller, string entityType, string entityId, string action, object? oldValue = null, object? newValue = null, string? note = null)
    {
        await EnsureTableAsync();

        var userIdClaim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? controller.User.FindFirstValue("sub");
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (int?)null;
        var userName = controller.User.FindFirstValue(ClaimTypes.Name) ?? controller.User.FindFirstValue("name") ?? controller.User.Identity?.Name;
        var ip = controller.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = controller.Request.Headers["User-Agent"].ToString();
        var beforeJson = oldValue is null ? null : JsonSerializer.Serialize(oldValue, JsonOptions);
        var afterJson = newValue is null ? null : JsonSerializer.Serialize(newValue, JsonOptions);

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO [dbo].[HE_THONG_NHATKY]
                ([LoaiDoiTuong], [MaDoiTuong], [HanhDong], [GiaTriTruoc], [GiaTriSau],
                 [MaNguoiThucHien], [TenNguoiThucHien], [GhiChu], [DiaChiIp], [UserAgent], [ThoiGian])
             VALUES
                ({entityType}, {entityId}, {action}, {beforeJson}, {afterJson},
                 {userId}, {userName}, {note}, {ip}, {userAgent}, {DateTime.UtcNow})
             """);
    }
}

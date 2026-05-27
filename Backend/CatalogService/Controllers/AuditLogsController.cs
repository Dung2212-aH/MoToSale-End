using CatalogService.Data;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly CatalogDbContext _db;
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(CatalogDbContext db, IAuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogSearchRequest request)
    {
        await _auditLogService.EnsureTableAsync();

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var rows = await _db.Database.SqlQueryRaw<AuditLogRow>(
            """
            SELECT
                MaNhatKy,
                LoaiDoiTuong,
                MaDoiTuong,
                HanhDong,
                GiaTriTruoc,
                GiaTriSau,
                MaNguoiThucHien,
                TenNguoiThucHien,
                GhiChu,
                DiaChiIp,
                UserAgent,
                ThoiGian
            FROM dbo.HE_THONG_NHATKY
            ORDER BY ThoiGian DESC, MaNhatKy DESC
            """
        ).ToListAsync();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            rows = rows.Where(x => string.Equals(x.LoaiDoiTuong, request.EntityType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            rows = rows.Where(x => string.Equals(x.HanhDong, request.Action, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (request.ActorUserId.HasValue)
        {
            rows = rows.Where(x => x.MaNguoiThucHien == request.ActorUserId.Value).ToList();
        }

        if (request.From.HasValue)
        {
            rows = rows.Where(x => x.ThoiGian >= request.From.Value).ToList();
        }

        if (request.To.HasValue)
        {
            rows = rows.Where(x => x.ThoiGian <= request.To.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            rows = rows.Where(x =>
                Contains(x.MaDoiTuong, keyword) ||
                Contains(x.TenNguoiThucHien, keyword) ||
                Contains(x.GhiChu, keyword) ||
                Contains(x.GiaTriTruoc, keyword) ||
                Contains(x.GiaTriSau, keyword)).ToList();
        }

        var total = rows.Count;
        var items = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    private static bool Contains(string? source, string value)
    {
        return source?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
    }
}

public class AuditLogSearchRequest
{
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public int? ActorUserId { get; set; }
    public string? Keyword { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuditLogRow
{
    public long MaNhatKy { get; set; }
    public string LoaiDoiTuong { get; set; } = string.Empty;
    public string MaDoiTuong { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string? GiaTriTruoc { get; set; }
    public string? GiaTriSau { get; set; }
    public int? MaNguoiThucHien { get; set; }
    public string? TenNguoiThucHien { get; set; }
    public string? GhiChu { get; set; }
    public string? DiaChiIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ThoiGian { get; set; }
}

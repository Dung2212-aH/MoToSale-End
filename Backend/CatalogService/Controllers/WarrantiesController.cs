using System.Security.Claims;
using CatalogService.Data;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/warranties")]
public class WarrantiesController : ControllerBase
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Received",
        "Processing",
        "WaitingParts",
        "Completed",
        "Rejected"
    };

    private readonly CatalogDbContext _db;
    private readonly IAuditLogService _auditLog;

    public WarrantiesController(CatalogDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        await EnsureTablesAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _db.Database.SqlQueryRaw<WarrantyRow>(
            """
            SELECT
                MaBaoHanh,
                MaPhieuBaoHanh,
                MaDonHang,
                MaNguoiDung,
                TenKhachHang,
                SoDienThoai,
                MaSanPham,
                MaBienSanPham,
                SKU,
                TenSanPham,
                SoKhung,
                SoMay,
                NgayMua,
                HetHanBaoHanh,
                LoiKhachBao,
                TrangThai,
                ChiPhiDuKien,
                ChiPhiThucTe,
                GhiChu,
                MaNguoiTao,
                NgayTao,
                NgayCapNhat
            FROM dbo.BAOHANH_PHIEU
            ORDER BY NgayTao DESC, MaBaoHanh DESC
            """
        ).ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            rows = rows.Where(x =>
                Contains(x.MaPhieuBaoHanh, s) ||
                Contains(x.TenKhachHang, s) ||
                Contains(x.SoDienThoai, s) ||
                Contains(x.SKU, s) ||
                Contains(x.TenSanPham, s)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            rows = rows.Where(x => string.Equals(x.TrangThai, status.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = rows.Count;
        var items = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        await EnsureTablesAsync();
        var warranty = (await _db.Database.SqlQueryRaw<WarrantyRow>($"SELECT MaBaoHanh, MaPhieuBaoHanh, MaDonHang, MaNguoiDung, TenKhachHang, SoDienThoai, MaSanPham, MaBienSanPham, SKU, TenSanPham, SoKhung, SoMay, NgayMua, HetHanBaoHanh, LoiKhachBao, TrangThai, ChiPhiDuKien, ChiPhiThucTe, GhiChu, MaNguoiTao, NgayTao, NgayCapNhat FROM dbo.BAOHANH_PHIEU WHERE MaBaoHanh = {id}").ToListAsync()).FirstOrDefault();
        if (warranty is null)
        {
            return NotFound(new { message = "Khong tim thay phieu bao hanh." });
        }

        var histories = await _db.Database.SqlQueryRaw<WarrantyHistoryRow>($"SELECT MaLichSuBaoHanh, MaBaoHanh, TrangThaiCu, TrangThaiMoi, GhiChu, MaNguoiThucHien, NgayTao FROM dbo.BAOHANH_LICHSU WHERE MaBaoHanh = {id} ORDER BY NgayTao DESC, MaLichSuBaoHanh DESC").ToListAsync();
        return Ok(new { warranty, histories });
    }

    [HttpPost]
    public async Task<IActionResult> Create(WarrantyRequest request)
    {
        await EnsureTablesAsync();
        if (string.IsNullOrWhiteSpace(request.TenKhachHang) || string.IsNullOrWhiteSpace(request.SoDienThoai) || string.IsNullOrWhiteSpace(request.TenSanPham) || string.IsNullOrWhiteSpace(request.LoiKhachBao))
        {
            return BadRequest(new { message = "Khach hang, so dien thoai, san pham va loi khach bao la bat buoc." });
        }

        var now = DateTime.UtcNow;
        var code = $"BH-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        var userId = GetCurrentUserId();

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.BAOHANH_PHIEU
                (MaPhieuBaoHanh, MaDonHang, MaNguoiDung, TenKhachHang, SoDienThoai, MaSanPham, MaBienSanPham, SKU, TenSanPham, SoKhung, SoMay, NgayMua, HetHanBaoHanh, LoiKhachBao, TrangThai, ChiPhiDuKien, ChiPhiThucTe, GhiChu, MaNguoiTao, NgayTao, NgayCapNhat)
            VALUES
                ({code}, {request.MaDonHang}, {request.MaNguoiDung}, {request.TenKhachHang.Trim()}, {request.SoDienThoai.Trim()}, {request.MaSanPham}, {request.MaBienSanPham}, {TrimToNull(request.SKU)}, {request.TenSanPham.Trim()}, {TrimToNull(request.SoKhung)}, {TrimToNull(request.SoMay)}, {request.NgayMua}, {request.HetHanBaoHanh}, {request.LoiKhachBao.Trim()}, N'Received', {request.ChiPhiDuKien}, {request.ChiPhiThucTe}, {TrimToNull(request.GhiChu)}, {userId}, {now}, {now})
            """);

        var id = (await _db.Database.SqlQueryRaw<WarrantyIdRow>($"SELECT MaBaoHanh FROM dbo.BAOHANH_PHIEU WHERE MaPhieuBaoHanh = N'{code.Replace("'", "''")}'").ToListAsync()).First().MaBaoHanh;
        await InsertHistoryAsync(id, null, "Received", "Tiep nhan bao hanh", userId);
        await _auditLog.WriteAsync(this, "Warranty", id.ToString(), "Create", null, new { code, request.TenKhachHang, request.TenSanPham });

        return CreatedAtAction(nameof(GetById), new { id }, new { id, maPhieuBaoHanh = code, trangThai = "Received" });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, WarrantyStatusRequest request)
    {
        await EnsureTablesAsync();
        var status = AllowedStatuses.FirstOrDefault(x => x.Equals(request.TrangThai?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status is null)
        {
            return BadRequest(new { message = "Trang thai bao hanh khong hop le." });
        }

        var warranty = (await _db.Database.SqlQueryRaw<WarrantyRow>($"SELECT MaBaoHanh, MaPhieuBaoHanh, MaDonHang, MaNguoiDung, TenKhachHang, SoDienThoai, MaSanPham, MaBienSanPham, SKU, TenSanPham, SoKhung, SoMay, NgayMua, HetHanBaoHanh, LoiKhachBao, TrangThai, ChiPhiDuKien, ChiPhiThucTe, GhiChu, MaNguoiTao, NgayTao, NgayCapNhat FROM dbo.BAOHANH_PHIEU WHERE MaBaoHanh = {id}").ToListAsync()).FirstOrDefault();
        if (warranty is null)
        {
            return NotFound(new { message = "Khong tim thay phieu bao hanh." });
        }

        var oldStatus = warranty.TrangThai;
        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE dbo.BAOHANH_PHIEU
            SET TrangThai = {status}, ChiPhiThucTe = COALESCE({request.ChiPhiThucTe}, ChiPhiThucTe), GhiChu = COALESCE({TrimToNull(request.GhiChu)}, GhiChu), NgayCapNhat = {now}
            WHERE MaBaoHanh = {id}
            """);
        await InsertHistoryAsync(id, oldStatus, status, request.GhiChu, userId);
        await _auditLog.WriteAsync(this, "Warranty", id.ToString(), "UpdateStatus", new { TrangThai = oldStatus }, new { TrangThai = status, request.ChiPhiThucTe }, request.GhiChu);

        return Ok(new { id, trangThai = status });
    }

    private async Task EnsureTablesAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.BAOHANH_PHIEU', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.BAOHANH_PHIEU (
                    MaBaoHanh INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaPhieuBaoHanh NVARCHAR(40) NOT NULL UNIQUE,
                    MaDonHang INT NULL,
                    MaNguoiDung INT NULL,
                    TenKhachHang NVARCHAR(200) NOT NULL,
                    SoDienThoai NVARCHAR(30) NOT NULL,
                    MaSanPham INT NULL,
                    MaBienSanPham INT NULL,
                    SKU NVARCHAR(80) NULL,
                    TenSanPham NVARCHAR(255) NOT NULL,
                    SoKhung NVARCHAR(80) NULL,
                    SoMay NVARCHAR(80) NULL,
                    NgayMua DATETIME2(0) NULL,
                    HetHanBaoHanh DATETIME2(0) NULL,
                    LoiKhachBao NVARCHAR(1000) NOT NULL,
                    TrangThai VARCHAR(30) NOT NULL,
                    ChiPhiDuKien DECIMAL(18,2) NULL,
                    ChiPhiThucTe DECIMAL(18,2) NULL,
                    GhiChu NVARCHAR(1000) NULL,
                    MaNguoiTao INT NULL,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_BAOHANH_PHIEU_Status_Time ON dbo.BAOHANH_PHIEU (TrangThai, NgayTao DESC);
            END;

            IF OBJECT_ID(N'dbo.BAOHANH_LICHSU', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.BAOHANH_LICHSU (
                    MaLichSuBaoHanh INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaBaoHanh INT NOT NULL,
                    TrangThaiCu VARCHAR(30) NULL,
                    TrangThaiMoi VARCHAR(30) NOT NULL,
                    GhiChu NVARCHAR(1000) NULL,
                    MaNguoiThucHien INT NULL,
                    NgayTao DATETIME2(0) NOT NULL,
                    CONSTRAINT FK_BAOHANH_LICHSU_PHIEU FOREIGN KEY (MaBaoHanh) REFERENCES dbo.BAOHANH_PHIEU (MaBaoHanh)
                );
                CREATE INDEX IX_BAOHANH_LICHSU_Phieu ON dbo.BAOHANH_LICHSU (MaBaoHanh, NgayTao DESC);
            END;
            """);
    }

    private async Task InsertHistoryAsync(int warrantyId, string? oldStatus, string newStatus, string? note, int? userId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.BAOHANH_LICHSU (MaBaoHanh, TrangThaiCu, TrangThaiMoi, GhiChu, MaNguoiThucHien, NgayTao)
            VALUES ({warrantyId}, {oldStatus}, {newStatus}, {TrimToNull(note)}, {userId}, {DateTime.UtcNow})
            """);
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }

    private static bool Contains(string? value, string search) => value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public class WarrantyRequest
{
    public int? MaDonHang { get; set; }
    public int? MaNguoiDung { get; set; }
    public string TenKhachHang { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public int? MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? SoKhung { get; set; }
    public string? SoMay { get; set; }
    public DateTime? NgayMua { get; set; }
    public DateTime? HetHanBaoHanh { get; set; }
    public string LoiKhachBao { get; set; } = "";
    public decimal? ChiPhiDuKien { get; set; }
    public decimal? ChiPhiThucTe { get; set; }
    public string? GhiChu { get; set; }
}

public class WarrantyStatusRequest
{
    public string? TrangThai { get; set; }
    public decimal? ChiPhiThucTe { get; set; }
    public string? GhiChu { get; set; }
}

public class WarrantyIdRow
{
    public int MaBaoHanh { get; set; }
}

public class WarrantyRow
{
    public int MaBaoHanh { get; set; }
    public string MaPhieuBaoHanh { get; set; } = "";
    public int? MaDonHang { get; set; }
    public int? MaNguoiDung { get; set; }
    public string TenKhachHang { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public int? MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? SoKhung { get; set; }
    public string? SoMay { get; set; }
    public DateTime? NgayMua { get; set; }
    public DateTime? HetHanBaoHanh { get; set; }
    public string LoiKhachBao { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public decimal? ChiPhiDuKien { get; set; }
    public decimal? ChiPhiThucTe { get; set; }
    public string? GhiChu { get; set; }
    public int? MaNguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class WarrantyHistoryRow
{
    public int MaLichSuBaoHanh { get; set; }
    public int MaBaoHanh { get; set; }
    public string? TrangThaiCu { get; set; }
    public string TrangThaiMoi { get; set; } = "";
    public string? GhiChu { get; set; }
    public int? MaNguoiThucHien { get; set; }
    public DateTime NgayTao { get; set; }
}

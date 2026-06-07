using CatalogService.Data;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly CatalogDbContext _db;
    private readonly IAuditLogService _auditLog;
    public ModelsController(CatalogDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    private static readonly string[] AllowedLoaiXe = { "XeSo", "TayGa", "ConTay", "XeDien", "Khac" };

    // Trả 'Khac' nếu rỗng; trả giá trị chuẩn nếu hợp lệ; trả null nếu giá trị không hợp lệ.
    private static string? NormalizeLoaiXe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Khac";
        return AllowedLoaiXe.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? brandId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.VehicleModels.AsNoTracking().AsQueryable();
        if (brandId.HasValue)
            query = query.Where(m => m.MaHangXe == brandId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(m => m.TenDongXe.ToLower().Contains(s) || m.Slug.Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.TenDongXe)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new { id = m.MaDongXe, maHangXe = m.MaHangXe, tenDongXe = m.TenDongXe, slug = m.Slug, loaiXe = m.LoaiXe, dangHoatDong = m.DangHoatDong, ngayTao = m.NgayTao })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _db.VehicleModels.AsNoTracking().FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model == null) return NotFound();
        return Ok(new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, loaiXe = model.LoaiXe, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(VehicleModelRequest request)
    {
        if (!await _db.Brands.AnyAsync(b => b.MaHangXe == request.MaHangXe))
        {
            return BadRequest(new { message = "Hang xe khong ton tai." });
        }

        var loaiXe = NormalizeLoaiXe(request.LoaiXe);
        if (loaiXe is null)
        {
            return BadRequest(new { message = "Loai xe khong hop le (XeSo/TayGa/ConTay/XeDien/Khac)." });
        }

        var now = DateTime.UtcNow;
        var model = new CatalogService.Entities.VehicleModel
        {
            MaHangXe = request.MaHangXe,
            TenDongXe = request.TenDongXe.Trim(),
            Slug = request.Slug.Trim(),
            LoaiXe = loaiXe,
            DangHoatDong = request.DangHoatDong,
            NgayTao = now,
            NgayCapNhat = now
        };

        _db.VehicleModels.Add(model);
        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "VehicleModel", model.MaDongXe.ToString(), "Create", null, new { model.MaDongXe, model.MaHangXe, model.TenDongXe, model.Slug, model.LoaiXe, model.DangHoatDong });
        return CreatedAtAction(nameof(GetById), new { id = model.MaDongXe }, new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, loaiXe = model.LoaiXe, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VehicleModelRequest request)
    {
        var model = await _db.VehicleModels.FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model is null) return NotFound();
        var oldValue = new { model.MaHangXe, model.TenDongXe, model.Slug, model.LoaiXe, model.DangHoatDong };

        if (!await _db.Brands.AnyAsync(b => b.MaHangXe == request.MaHangXe))
        {
            return BadRequest(new { message = "Hang xe khong ton tai." });
        }

        var loaiXe = NormalizeLoaiXe(request.LoaiXe);
        if (loaiXe is null)
        {
            return BadRequest(new { message = "Loai xe khong hop le (XeSo/TayGa/ConTay/XeDien/Khac)." });
        }

        model.MaHangXe = request.MaHangXe;
        model.TenDongXe = request.TenDongXe.Trim();
        model.Slug = request.Slug.Trim();
        model.LoaiXe = loaiXe;
        model.DangHoatDong = request.DangHoatDong;
        model.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "VehicleModel", model.MaDongXe.ToString(), "Update", oldValue, new { model.MaHangXe, model.TenDongXe, model.Slug, model.LoaiXe, model.DangHoatDong });
        return Ok(new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, loaiXe = model.LoaiXe, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.VehicleModels.FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model is null) return NotFound();
        var oldValue = new { model.MaDongXe, model.MaHangXe, model.TenDongXe, model.Slug, model.DangHoatDong };

        var hasProducts = await _db.Products.AnyAsync(p => p.MaDongXe == id);
        if (hasProducts)
        {
            return BadRequest(new { message = "Khong the xoa dong xe dang co san pham." });
        }

        _db.VehicleModels.Remove(model);
        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "VehicleModel", id.ToString(), "Delete", oldValue, null);
        return NoContent();
    }
}

public class VehicleModelRequest
{
    public int MaHangXe { get; set; }
    public string TenDongXe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    // Loại xe: XeSo / TayGa / ConTay / XeDien / Khac (rỗng -> Khac)
    public string? LoaiXe { get; set; }
    public bool DangHoatDong { get; set; } = true;
}

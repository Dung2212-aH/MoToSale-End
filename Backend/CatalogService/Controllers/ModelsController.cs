using CatalogService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly CatalogDbContext _db;
    public ModelsController(CatalogDbContext db) => _db = db;

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
            .Select(m => new { id = m.MaDongXe, maHangXe = m.MaHangXe, tenDongXe = m.TenDongXe, slug = m.Slug, dangHoatDong = m.DangHoatDong, ngayTao = m.NgayTao })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _db.VehicleModels.AsNoTracking().FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model == null) return NotFound();
        return Ok(new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(VehicleModelRequest request)
    {
        if (!await _db.Brands.AnyAsync(b => b.MaHangXe == request.MaHangXe))
        {
            return BadRequest(new { message = "Hang xe khong ton tai." });
        }

        var now = DateTime.UtcNow;
        var model = new CatalogService.Entities.VehicleModel
        {
            MaHangXe = request.MaHangXe,
            TenDongXe = request.TenDongXe.Trim(),
            Slug = request.Slug.Trim(),
            DangHoatDong = request.DangHoatDong,
            NgayTao = now,
            NgayCapNhat = now
        };

        _db.VehicleModels.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.MaDongXe }, new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VehicleModelRequest request)
    {
        var model = await _db.VehicleModels.FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model is null) return NotFound();

        if (!await _db.Brands.AnyAsync(b => b.MaHangXe == request.MaHangXe))
        {
            return BadRequest(new { message = "Hang xe khong ton tai." });
        }

        model.MaHangXe = request.MaHangXe;
        model.TenDongXe = request.TenDongXe.Trim();
        model.Slug = request.Slug.Trim();
        model.DangHoatDong = request.DangHoatDong;
        model.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = model.MaDongXe, maHangXe = model.MaHangXe, tenDongXe = model.TenDongXe, slug = model.Slug, dangHoatDong = model.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.VehicleModels.FirstOrDefaultAsync(m => m.MaDongXe == id);
        if (model is null) return NotFound();

        var hasProducts = await _db.Products.AnyAsync(p => p.MaDongXe == id);
        if (hasProducts)
        {
            return BadRequest(new { message = "Khong the xoa dong xe dang co san pham." });
        }

        _db.VehicleModels.Remove(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class VehicleModelRequest
{
    public int MaHangXe { get; set; }
    public string TenDongXe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; } = true;
}

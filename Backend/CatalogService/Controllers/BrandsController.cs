using CatalogService.Data;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly CatalogDbContext _db;
    private readonly IImageStorageService _imageStorage;
    private readonly IAuditLogService _auditLog;

    public BrandsController(CatalogDbContext db, IImageStorageService imageStorage, IAuditLogService auditLog)
    {
        _db = db;
        _imageStorage = imageStorage;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Brands.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(b => b.TenHang.ToLower().Contains(s) || b.Slug.Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.TenHang)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new { id = b.MaHangXe, tenHang = b.TenHang, slug = b.Slug, logoUrl = b.LogoUrl, dangHoatDong = b.DangHoatDong, ngayTao = b.NgayTao })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById(int id)
    {
        var brand = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.MaHangXe == id);
        if (brand == null) return NotFound();
        return Ok(new { id = brand.MaHangXe, tenHang = brand.TenHang, slug = brand.Slug, logoUrl = brand.LogoUrl, dangHoatDong = brand.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateBrand(BrandRequest request)
    {
        var now = DateTime.UtcNow;
        var brand = new CatalogService.Entities.Brand
        {
            TenHang = request.TenHang.Trim(),
            Slug = request.Slug.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            DangHoatDong = request.DangHoatDong,
            NgayTao = now,
            NgayCapNhat = now
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Brand", brand.MaHangXe.ToString(), "Create", null, new { brand.MaHangXe, brand.TenHang, brand.Slug, brand.LogoUrl, brand.DangHoatDong });
        return CreatedAtAction(nameof(GetBrandById), new { id = brand.MaHangXe }, new { id = brand.MaHangXe, tenHang = brand.TenHang, slug = brand.Slug, logoUrl = brand.LogoUrl, dangHoatDong = brand.DangHoatDong });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBrand(int id, BrandRequest request)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.MaHangXe == id);
        if (brand is null) return NotFound();
        var oldValue = new { brand.TenHang, brand.Slug, brand.LogoUrl, brand.DangHoatDong };

        brand.TenHang = request.TenHang.Trim();
        brand.Slug = request.Slug.Trim();
        brand.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        brand.DangHoatDong = request.DangHoatDong;
        brand.NgayCapNhat = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Brand", brand.MaHangXe.ToString(), "Update", oldValue, new { brand.TenHang, brand.Slug, brand.LogoUrl, brand.DangHoatDong });
        return Ok(new { id = brand.MaHangXe, tenHang = brand.TenHang, slug = brand.Slug, logoUrl = brand.LogoUrl, dangHoatDong = brand.DangHoatDong });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.MaHangXe == id);
        if (brand is null) return NotFound();
        var oldValue = new { brand.MaHangXe, brand.TenHang, brand.Slug, brand.LogoUrl, brand.DangHoatDong };

        var hasModels = await _db.VehicleModels.AnyAsync(m => m.MaHangXe == id);
        var hasProducts = await _db.Products.AnyAsync(p => p.MaHangXe == id);
        if (hasModels || hasProducts)
        {
            return BadRequest(new { message = "Khong the xoa hang xe dang co dong xe hoac san pham." });
        }

        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Brand", id.ToString(), "Delete", oldValue, null);
        return NoContent();
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:int}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui long chon file logo." });
        }

        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.MaHangXe == id);
        if (brand is null)
        {
            return NotFound(new { message = "Khong tim thay hang xe." });
        }
        var oldLogoUrl = brand.LogoUrl;

        var url = await _imageStorage.SaveImageAsync(file, "brands", HttpContext.RequestAborted);
        brand.LogoUrl = url;
        brand.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Brand", brand.MaHangXe.ToString(), "UpdateLogo", new { LogoUrl = oldLogoUrl }, new { brand.LogoUrl });

        return Ok(new { id = brand.MaHangXe, logoUrl = brand.LogoUrl });
    }
}

public class BrandRequest
{
    public string TenHang { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool DangHoatDong { get; set; } = true;
}

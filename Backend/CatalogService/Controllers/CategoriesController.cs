using CatalogService.Data;
using CatalogService.DTOs.Categories;
using CatalogService.Entities;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly CatalogDbContext _dbContext;

    public CategoriesController(ICatalogService catalogService, CatalogDbContext dbContext)
    {
        _catalogService = catalogService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] bool activeOnly = true)
    {
        return Ok(await _catalogService.GetCategoriesAsync(activeOnly));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryCreateDto request)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _dbContext.Categories.AnyAsync(c => c.Slug == slug))
        {
            return BadRequest(new { message = "Slug danh muc da ton tai." });
        }

        var now = DateTime.UtcNow;
        var category = new Category
        {
            MaDanhMucCha = request.MaDanhMucCha,
            TenDanhMuc = request.TenDanhMuc.Trim(),
            Slug = slug,
            MoTa = TrimToNull(request.MoTa),
            ThuTuHienThi = request.ThuTuHienThi,
            DangHoatDong = request.DangHoatDong,
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), ToCategoryDto(category));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDto request)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.MaDanhMuc == id);
        if (category is null)
        {
            return NotFound(new { message = "Khong tim thay danh muc." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc != id && c.Slug == slug))
        {
            return BadRequest(new { message = "Slug danh muc da ton tai." });
        }

        category.MaDanhMucCha = request.MaDanhMucCha;
        category.TenDanhMuc = request.TenDanhMuc.Trim();
        category.Slug = slug;
        category.MoTa = TrimToNull(request.MoTa);
        category.ThuTuHienThi = request.ThuTuHienThi;
        category.DangHoatDong = request.DangHoatDong;
        category.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(ToCategoryDto(category));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.MaDanhMuc == id);
        if (category is null)
        {
            return NotFound(new { message = "Khong tim thay danh muc." });
        }

        category.DangHoatDong = false;
        category.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static CategoryDto ToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            MaDanhMuc = category.MaDanhMuc,
            MaDanhMucCha = category.MaDanhMucCha,
            TenDanhMuc = category.TenDanhMuc,
            Slug = category.Slug,
            MoTa = category.MoTa,
            ThuTuHienThi = category.ThuTuHienThi,
            DangHoatDong = category.DangHoatDong
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

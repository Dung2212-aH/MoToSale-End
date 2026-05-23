using CatalogService.Data;
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.MaDanhMuc == id)
            .Select(c => new
            {
                id = c.MaDanhMuc,
                maDanhMuc = c.MaDanhMuc,
                tenDanhMuc = c.TenDanhMuc,
                slug = c.Slug,
                moTa = c.MoTa,
                danhMucChaId = c.MaDanhMucCha,
                maDanhMucCha = c.MaDanhMucCha,
                thuTu = c.ThuTuHienThi,
                thuTuHienThi = c.ThuTuHienThi,
                dangHoatDong = c.DangHoatDong,
                ngayTao = c.NgayTao,
                ngayCapNhat = c.NgayCapNhat
            })
            .FirstOrDefaultAsync();

        return category is null ? NotFound(new { message = "Khong tim thay danh muc." }) : Ok(category);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryRequest request)
    {
        var slug = NormalizeSlug(request.Slug, request.TenDanhMuc);
        if (await _dbContext.Categories.AnyAsync(c => c.Slug == slug))
        {
            return BadRequest(new { message = "Slug danh muc da ton tai." });
        }

        if (request.DanhMucChaId.HasValue && !await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc == request.DanhMucChaId.Value))
        {
            return BadRequest(new { message = "Danh muc cha khong hop le." });
        }

        var now = DateTime.UtcNow;
        var category = new Category
        {
            TenDanhMuc = request.TenDanhMuc.Trim(),
            Slug = slug,
            MoTa = TrimToNull(request.MoTa),
            MaDanhMucCha = request.DanhMucChaId,
            ThuTuHienThi = request.ThuTu ?? request.ThuTuHienThi ?? 0,
            DangHoatDong = request.DangHoatDong ?? request.IsActive ?? true,
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.MaDanhMuc }, new { id = category.MaDanhMuc });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.MaDanhMuc == id);
        if (category is null)
        {
            return NotFound(new { message = "Khong tim thay danh muc." });
        }

        var slug = NormalizeSlug(request.Slug, request.TenDanhMuc);
        if (await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc != id && c.Slug == slug))
        {
            return BadRequest(new { message = "Slug danh muc da ton tai." });
        }

        if (request.DanhMucChaId == id)
        {
            return BadRequest(new { message = "Danh muc cha khong hop le." });
        }

        if (request.DanhMucChaId.HasValue && !await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc == request.DanhMucChaId.Value))
        {
            return BadRequest(new { message = "Danh muc cha khong hop le." });
        }

        if (request.DanhMucChaId.HasValue && await IsDescendantCategoryAsync(id, request.DanhMucChaId.Value))
        {
            return BadRequest(new { message = "Khong the chon danh muc con lam danh muc cha." });
        }

        category.TenDanhMuc = request.TenDanhMuc.Trim();
        category.Slug = slug;
        category.MoTa = TrimToNull(request.MoTa);
        category.MaDanhMucCha = request.DanhMucChaId;
        category.ThuTuHienThi = request.ThuTu ?? request.ThuTuHienThi ?? category.ThuTuHienThi;
        category.DangHoatDong = request.DangHoatDong ?? request.IsActive ?? category.DangHoatDong;
        category.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { id = category.MaDanhMuc });
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

        if (await _dbContext.Categories.AnyAsync(c => c.MaDanhMucCha == id))
        {
            return BadRequest(new { message = "Khong the xoa danh muc dang co danh muc con." });
        }

        if (await _dbContext.Products.AnyAsync(p => p.MaDanhMuc == id))
        {
            return BadRequest(new { message = "Khong the xoa danh muc dang co san pham." });
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static string NormalizeSlug(string? slug, string name)
    {
        var value = string.IsNullOrWhiteSpace(slug) ? name : slug;
        return value.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<bool> IsDescendantCategoryAsync(int categoryId, int candidateParentId)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(c => new { c.MaDanhMuc, c.MaDanhMucCha })
            .ToListAsync();

        var childrenByParent = categories
            .Where(c => c.MaDanhMucCha.HasValue)
            .GroupBy(c => c.MaDanhMucCha!.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.MaDanhMuc).ToList());

        var stack = new Stack<int>();
        if (childrenByParent.TryGetValue(categoryId, out var directChildren))
        {
            foreach (var childId in directChildren)
            {
                stack.Push(childId);
            }
        }

        while (stack.Count > 0)
        {
            var currentId = stack.Pop();
            if (currentId == candidateParentId)
            {
                return true;
            }

            if (childrenByParent.TryGetValue(currentId, out var childIds))
            {
                foreach (var childId in childIds)
                {
                    stack.Push(childId);
                }
            }
        }

        return false;
    }
}

public class CategoryRequest
{
    public string TenDanhMuc { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? MoTa { get; set; }
    public int? DanhMucChaId { get; set; }
    public int? MaDanhMucCha { get => DanhMucChaId; set => DanhMucChaId = value; }
    public int? ThuTu { get; set; }
    public int? ThuTuHienThi { get; set; }
    public bool? DangHoatDong { get; set; }
    public bool? IsActive { get; set; }
}

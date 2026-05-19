using CatalogService.Data;
using CatalogService.DTOs.Products;
using CatalogService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly CatalogDbContext _dbContext;

    public ProductsController(
        ICatalogService catalogService,
        CatalogDbContext dbContext)
    {
        _catalogService = catalogService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchDto search)
    {
        return Ok(await _catalogService.GetProductsAsync(search));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _catalogService.GetProductByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var reviewSummary = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.MaSanPham == id && r.TrangThai == "Approved")
            .GroupBy(r => r.MaSanPham)
            .Select(g => new
            {
                TotalReviews = g.Count(),
                AverageRating = g.Average(r => r.Diem)
            })
            .FirstOrDefaultAsync();

        product.TongDanhGia = reviewSummary?.TotalReviews ?? 0;
        product.DiemTrungBinh = reviewSummary?.AverageRating ?? 0;

        return Ok(product);
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters()
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.DangHoatDong)
            .OrderBy(c => c.ThuTuHienThi)
            .ThenBy(c => c.TenDanhMuc)
            .Select(c => new
            {
                id = c.MaDanhMuc,
                name = c.TenDanhMuc,
                slug = c.Slug,
                parentCategoryId = c.MaDanhMucCha,
                sortOrder = c.ThuTuHienThi,
                isActive = c.DangHoatDong
            })
            .ToListAsync();

        var brands = await _dbContext.Brands
            .AsNoTracking()
            .Where(b => b.DangHoatDong)
            .OrderBy(b => b.TenHang)
            .Select(b => new
            {
                id = b.MaHangXe,
                name = b.TenHang,
                slug = b.Slug
            })
            .ToListAsync();

        var carModels = await _dbContext.VehicleModels
            .AsNoTracking()
            .Join(
                _dbContext.Brands.AsNoTracking(),
                model => model.MaHangXe,
                brand => brand.MaHangXe,
                (model, brand) => new { model, brand })
            .Where(m => m.model.DangHoatDong && m.brand.DangHoatDong)
            .OrderBy(m => m.brand.TenHang)
            .ThenBy(m => m.model.TenDongXe)
            .Select(m => new
            {
                id = m.model.MaDongXe,
                brandId = m.model.MaHangXe,
                brandName = m.brand.TenHang,
                name = m.model.TenDongXe,
                slug = m.model.Slug
            })
            .ToListAsync();

        var showrooms = await _dbContext.Showrooms
            .AsNoTracking()
            .Where(s => s.DangHoatDong)
            .OrderBy(s => s.TenShowroom)
            .Select(s => new
            {
                id = s.MaShowroom,
                name = s.TenShowroom,
                slug = s.Slug
            })
            .ToListAsync();

        return Ok(new { categories, brands, carModels, showrooms, partCompatibleTypes = carModels });
    }
}
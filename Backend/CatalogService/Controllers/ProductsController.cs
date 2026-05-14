using CatalogService.Data;
using CatalogService.DTOs.ProductImages;
using CatalogService.DTOs.Products;
using CatalogService.Entities;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly CatalogDbContext _dbContext;
    private readonly IImageStorageService _imageStorage;

    public ProductsController(
        ICatalogService catalogService,
        CatalogDbContext dbContext,
        IImageStorageService imageStorage)
    {
        _catalogService = catalogService;
        _dbContext = dbContext;
        _imageStorage = imageStorage;
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
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateDto request)
    {
        var code = request.MaSanPhamKinhDoanh.Trim();
        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await _dbContext.Products.AnyAsync(p => p.MaSanPhamKinhDoanh == code))
        {
            return BadRequest(new { message = "Ma san pham kinh doanh da ton tai." });
        }

        if (await _dbContext.Products.AnyAsync(p => p.Slug == slug))
        {
            return BadRequest(new { message = "Slug san pham da ton tai." });
        }

        if (!await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc == request.MaDanhMuc))
        {
            return BadRequest(new { message = "Danh muc khong ton tai." });
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            MaSanPhamKinhDoanh = code,
            TenSanPham = request.TenSanPham.Trim(),
            Slug = slug,
            MaDanhMuc = request.MaDanhMuc,
            MaHangXe = request.MaHangXe,
            MaDongXe = request.MaDongXe,
            MaShowroom = request.MaShowroom,
            LoaiSanPham = request.LoaiSanPham.Trim(),
            MoTaNgan = TrimToNull(request.MoTaNgan),
            MoTa = TrimToNull(request.MoTa),
            GiaGoc = request.GiaGoc,
            GiaKhuyenMai = request.GiaKhuyenMai,
            SoLuongTon = request.SoLuongTon,
            AnhChinhUrl = TrimToNull(request.AnhChinhUrl),
            DangHoatDong = request.DangHoatDong,
            TrangThaiSanPham = request.TrangThaiSanPham.Trim(),
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.MaSanPham }, ToProductResponse(product));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product is null)
        {
            return NotFound(new { message = "Khong tim thay san pham." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _dbContext.Products.AnyAsync(p => p.MaSanPham != id && p.Slug == slug))
        {
            return BadRequest(new { message = "Slug san pham da ton tai." });
        }

        if (!await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc == request.MaDanhMuc))
        {
            return BadRequest(new { message = "Danh muc khong ton tai." });
        }

        product.TenSanPham = request.TenSanPham.Trim();
        product.Slug = slug;
        product.MaDanhMuc = request.MaDanhMuc;
        product.MaHangXe = request.MaHangXe;
        product.MaDongXe = request.MaDongXe;
        product.MaShowroom = request.MaShowroom;
        product.LoaiSanPham = request.LoaiSanPham.Trim();
        product.MoTaNgan = TrimToNull(request.MoTaNgan);
        product.MoTa = TrimToNull(request.MoTa);
        product.GiaGoc = request.GiaGoc;
        product.GiaKhuyenMai = request.GiaKhuyenMai;
        product.SoLuongTon = request.SoLuongTon;
        product.AnhChinhUrl = TrimToNull(request.AnhChinhUrl);
        product.DangHoatDong = request.DangHoatDong;
        product.TrangThaiSanPham = request.TrangThaiSanPham.Trim();
        product.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(ToProductResponse(product));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product is null)
        {
            return NotFound(new { message = "Khong tim thay san pham." });
        }

        product.DangHoatDong = false;
        product.TrangThaiSanPham = "Inactive";
        product.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return NoContent();
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

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProductImage(int id, [FromForm] ProductImageCreateDto request)
    {
        if (request.MaSanPham != 0 && request.MaSanPham != id)
        {
            return BadRequest(new { message = "MaSanPham khong khop voi duong dan." });
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product is null)
        {
            return NotFound(new { message = "Khong tim thay san pham." });
        }

        if (request.MaBienSanPham.HasValue)
        {
            var variantExists = await _dbContext.ProductVariants
                .AsNoTracking()
                .AnyAsync(v => v.MaBienSanPham == request.MaBienSanPham.Value && v.MaSanPham == id);

            if (!variantExists)
            {
                return BadRequest(new { message = "Bien the khong thuoc san pham." });
            }
        }

        string imagePath;
        try
        {
            imagePath = await _imageStorage.SaveImageAsync(request.Image, $"products/{id}", HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var shouldSetPrimary = request.LaAnhChinh || string.IsNullOrWhiteSpace(product.AnhChinhUrl);
        if (shouldSetPrimary)
        {
            var currentPrimaryImages = await _dbContext.ProductImages
                .Where(i => i.MaSanPham == id && i.LaAnhChinh)
                .ToListAsync();

            foreach (var currentPrimary in currentPrimaryImages)
            {
                currentPrimary.LaAnhChinh = false;
            }

            product.AnhChinhUrl = imagePath;
            product.NgayCapNhat = DateTime.UtcNow;
        }

        var productImage = new ProductImage
        {
            MaSanPham = id,
            MaBienSanPham = request.MaBienSanPham,
            UrlAnh = imagePath,
            AltText = TrimToNull(request.AltText) ?? product.TenSanPham,
            LaAnhChinh = shouldSetPrimary,
            ThuTuHienThi = request.ThuTuHienThi,
            NgayTao = DateTime.UtcNow
        };

        _dbContext.ProductImages.Add(productImage);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id }, ToProductImageDto(productImage));
    }

    private static ProductImageDto ToProductImageDto(ProductImage image)
    {
        return new ProductImageDto
        {
            MaAnhSanPham = image.MaAnhSanPham,
            MaSanPham = image.MaSanPham,
            MaBienSanPham = image.MaBienSanPham,
            UrlAnh = image.UrlAnh,
            AltText = image.AltText,
            LaAnhChinh = image.LaAnhChinh,
            ThuTuHienThi = image.ThuTuHienThi
        };
    }

    private static object ToProductResponse(Product product)
    {
        var giaBan = product.GiaKhuyenMai ?? product.GiaGoc;

        return new
        {
            product.MaSanPham,
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.Slug,
            product.MaDanhMuc,
            product.MaHangXe,
            product.MaDongXe,
            product.MaShowroom,
            product.LoaiSanPham,
            product.MoTaNgan,
            product.MoTa,
            product.GiaGoc,
            product.GiaKhuyenMai,
            GiaBan = giaBan,
            product.SoLuongTon,
            product.AnhChinhUrl,
            product.DangHoatDong,
            product.TrangThaiSanPham
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

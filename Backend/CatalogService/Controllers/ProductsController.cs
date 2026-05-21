using CatalogService.Data;
using CatalogService.DTOs.Products;
using CatalogService.Entities;
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

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] UpdateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenSanPham))
        {
            return BadRequest(new { message = "Ten san pham la bat buoc." });
        }

        if (!request.MaDanhMuc.HasValue)
        {
            return BadRequest(new { message = "Danh muc la bat buoc." });
        }

        if (!request.GiaGoc.HasValue || request.GiaGoc.Value <= 0)
        {
            return BadRequest(new { message = "Gia goc phai lon hon 0." });
        }

        if (!await _dbContext.Categories.AnyAsync(c => c.MaDanhMuc == request.MaDanhMuc.Value))
        {
            return BadRequest(new { message = "Danh muc khong hop le." });
        }

        if (request.MaHangXe.HasValue && !await _dbContext.Brands.AnyAsync(b => b.MaHangXe == request.MaHangXe.Value))
        {
            return BadRequest(new { message = "Hang xe khong hop le." });
        }

        if (request.MaDongXe.HasValue && !await _dbContext.VehicleModels.AnyAsync(m => m.MaDongXe == request.MaDongXe.Value))
        {
            return BadRequest(new { message = "Dong xe khong hop le." });
        }

        var businessCode = string.IsNullOrWhiteSpace(request.MaSanPhamKinhDoanh)
            ? $"SP_{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            : request.MaSanPhamKinhDoanh.Trim();
        if (await _dbContext.Products.AnyAsync(p => p.MaSanPhamKinhDoanh == businessCode))
        {
            return BadRequest(new { message = "Ma san pham kinh doanh da ton tai." });
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? request.TenSanPham.Trim().ToLowerInvariant().Replace(' ', '-')
            : request.Slug.Trim();
        if (await _dbContext.Products.AnyAsync(p => p.Slug == slug))
        {
            return BadRequest(new { message = "Slug san pham da ton tai." });
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            MaSanPhamKinhDoanh = businessCode,
            TenSanPham = request.TenSanPham.Trim(),
            Slug = slug,
            LoaiSanPham = string.IsNullOrWhiteSpace(request.LoaiSanPham) ? "XeMay" : request.LoaiSanPham,
            MaDanhMuc = request.MaDanhMuc.Value,
            MaHangXe = request.MaHangXe,
            MaDongXe = request.MaDongXe,
            MoTaNgan = request.MoTaNgan,
            GiaGoc = request.GiaGoc.Value,
            GiaKhuyenMai = request.GiaKhuyenMai,
            SoLuongTon = request.SoLuongTon ?? 0,
            AnhChinhUrl = request.AnhChinhUrl,
            TrangThaiSanPham = string.IsNullOrWhiteSpace(request.TrangThaiSanPham) ? "Available" : request.TrangThaiSanPham,
            DangHoatDong = request.DangHoatDong ?? true,
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.MaSanPham }, new { id = product.MaSanPham });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product == null) return NotFound();

        if (request.TenSanPham != null) product.TenSanPham = request.TenSanPham.Trim();
        if (request.Slug != null) product.Slug = request.Slug.Trim();
        if (request.MaSanPhamKinhDoanh != null) product.MaSanPhamKinhDoanh = request.MaSanPhamKinhDoanh.Trim();
        if (request.LoaiSanPham != null) product.LoaiSanPham = request.LoaiSanPham;
        if (request.MaDanhMuc.HasValue) product.MaDanhMuc = request.MaDanhMuc.Value;
        if (request.MaHangXe.HasValue) product.MaHangXe = request.MaHangXe;
        if (request.MaDongXe.HasValue) product.MaDongXe = request.MaDongXe;
        if (request.MoTaNgan != null) product.MoTaNgan = request.MoTaNgan;
        if (request.GiaGoc.HasValue) product.GiaGoc = request.GiaGoc.Value;
        if (request.GiaKhuyenMai.HasValue) product.GiaKhuyenMai = request.GiaKhuyenMai;
        if (request.SoLuongTon.HasValue) product.SoLuongTon = request.SoLuongTon.Value;
        if (request.AnhChinhUrl != null) product.AnhChinhUrl = request.AnhChinhUrl;
        if (request.TrangThaiSanPham != null) product.TrangThaiSanPham = request.TrangThaiSanPham;
        if (request.DangHoatDong.HasValue) product.DangHoatDong = request.DangHoatDong.Value;
        product.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { id = product.MaSanPham, message = "Cập nhật sản phẩm thành công." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product == null) return NotFound();

        var hasOrderHistory = await _dbContext.ReviewOrderItems.AnyAsync(i => i.MaSanPham == id);
        if (hasOrderHistory)
        {
            return BadRequest(new { message = "Khong the xoa san pham da co lich su don hang. Hay ngung ban thay the." });
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.VOUCHER_SANPHAM WHERE MaSanPham = {id}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.TONKHO_GIUCHO WHERE MaSanPham = {id}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.PHUTUNG_TUONGTHICH WHERE MaPhuTung = {id}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.CHITIET_GIOHANG WHERE MaSanPham = {id}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.DANHGIASANPHAM WHERE MaSanPham = {id}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.LIENHE_YEUCAU SET MaSanPham = NULL WHERE MaSanPham = {id}");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

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

        return Ok(new { categories, brands, carModels, partCompatibleTypes = carModels });
    }

    // ===== Product Variants =====

    [HttpGet("{productId:int}/variants")]
    public async Task<IActionResult> GetVariants(int productId)
    {
        var variants = await _dbContext.Set<CatalogService.Entities.ProductVariant>()
            .AsNoTracking()
            .Where(v => v.MaSanPham == productId)
            .OrderBy(v => v.TenBienThe)
            .Select(v => new
            {
                id = v.MaBienSanPham,
                maBienSanPham = v.MaBienSanPham,
                maSanPham = v.MaSanPham,
                tenBienThe = v.TenBienThe,
                sku = v.SKU,
                phienBan = v.PhienBan,
                mauSac = v.MauSac,
                giaGhiDe = v.GiaGhiDe,
                soLuongTon = v.SoLuongTon,
                trangThai = v.TrangThai,
                ngayTao = v.NgayTao
            })
            .ToListAsync();

        return Ok(variants);
    }

    [HttpPost("{productId:int}/variants")]
    public async Task<IActionResult> CreateVariant(int productId, [FromBody] VariantRequest request)
    {
        var productExists = await _dbContext.Products.AnyAsync(p => p.MaSanPham == productId);
        if (!productExists) return NotFound(new { message = "Sản phẩm không tồn tại." });

        var now = DateTime.UtcNow;
        var variant = new CatalogService.Entities.ProductVariant
        {
            MaSanPham = productId,
            TenBienThe = request.TenBienThe?.Trim() ?? "",
            SKU = request.Sku?.Trim() ?? "",
            PhienBan = request.PhienBan?.Trim(),
            MauSac = request.MauSac?.Trim(),
            GiaGhiDe = request.GiaGhiDe,
            SoLuongTon = request.SoLuongTon ?? 0,
            TrangThai = request.TrangThai ?? "Available",
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync();
        return Ok(new { id = variant.MaBienSanPham, message = "Thêm biến thể thành công." });
    }

    [HttpPatch("{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> UpdateVariant(int productId, int variantId, [FromBody] VariantRequest request)
    {
        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == variantId && v.MaSanPham == productId);
        if (variant == null) return NotFound();

        if (request.TenBienThe != null) variant.TenBienThe = request.TenBienThe.Trim();
        if (request.Sku != null) variant.SKU = request.Sku.Trim();
        if (request.PhienBan != null) variant.PhienBan = request.PhienBan.Trim();
        if (request.MauSac != null) variant.MauSac = request.MauSac.Trim();
        if (request.GiaGhiDe.HasValue) variant.GiaGhiDe = request.GiaGhiDe;
        if (request.SoLuongTon.HasValue) variant.SoLuongTon = request.SoLuongTon;
        if (request.TrangThai != null) variant.TrangThai = request.TrangThai;
        variant.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { id = variant.MaBienSanPham, message = "Cập nhật biến thể thành công." });
    }

    [HttpDelete("{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> DeleteVariant(int productId, int variantId)
    {
        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == variantId && v.MaSanPham == productId);
        if (variant == null) return NotFound();

        _dbContext.ProductVariants.Remove(variant);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ===== Product Images =====

    [HttpGet("{productId:int}/images")]
    public async Task<IActionResult> GetImages(int productId)
    {
        var images = await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.MaSanPham == productId)
            .OrderBy(i => i.ThuTuHienThi)
            .Select(i => new
            {
                id = i.MaAnhSanPham,
                maSanPham = i.MaSanPham,
                urlAnh = i.UrlAnh,
                altText = i.AltText,
                laAnhChinh = i.LaAnhChinh,
                thuTuHienThi = i.ThuTuHienThi,
                maBienSanPham = i.MaBienSanPham
            })
            .ToListAsync();

        return Ok(images);
    }

    [HttpPost("{productId:int}/images")]
    public async Task<IActionResult> UploadImage(int productId, [FromForm] IFormFile? file, [FromForm] bool isMain = false, [FromForm] int? maBienSanPham = null, [FromForm] int? imageId = null)
    {
        // If imageId is provided, just update that image as main
        if (imageId.HasValue && isMain)
        {
            var existingImg = await _dbContext.ProductImages.FirstOrDefaultAsync(i => i.MaAnhSanPham == imageId.Value && i.MaSanPham == productId);
            if (existingImg == null) return NotFound();

            // Unset other main images for same product+variant
            var others = await _dbContext.ProductImages
                .Where(i => i.MaSanPham == productId && i.MaBienSanPham == existingImg.MaBienSanPham && i.LaAnhChinh)
                .ToListAsync();
            foreach (var o in others) o.LaAnhChinh = false;

            existingImg.LaAnhChinh = true;
            await _dbContext.SaveChangesAsync();
            return Ok(new { id = existingImg.MaAnhSanPham, laAnhChinh = true });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file ảnh." });
        }

        // Verify product exists
        var productExists = await _dbContext.Products.AnyAsync(p => p.MaSanPham == productId);
        if (!productExists)
        {
            return NotFound(new { message = "Sản phẩm không tồn tại." });
        }

        // Save file
        string url;
        try
        {
            url = await _imageStorage.SaveImageAsync(file, "products", HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        // If setting as main, unset other main images for same product+variant
        if (isMain)
        {
            var existingMain = await _dbContext.ProductImages
                .Where(i => i.MaSanPham == productId && i.MaBienSanPham == maBienSanPham && i.LaAnhChinh)
                .ToListAsync();
            foreach (var img in existingMain)
            {
                img.LaAnhChinh = false;
            }
        }

        var maxOrder = await _dbContext.ProductImages
            .Where(i => i.MaSanPham == productId)
            .MaxAsync(i => (int?)i.ThuTuHienThi) ?? 0;

        var image = new ProductImage
        {
            MaSanPham = productId,
            MaBienSanPham = maBienSanPham,
            UrlAnh = url,
            AltText = file.FileName,
            LaAnhChinh = isMain,
            ThuTuHienThi = maxOrder + 1,
            NgayTao = DateTime.UtcNow
        };

        _dbContext.ProductImages.Add(image);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            id = image.MaAnhSanPham,
            urlAnh = image.UrlAnh,
            maBienSanPham = image.MaBienSanPham,
            laAnhChinh = image.LaAnhChinh,
            thuTuHienThi = image.ThuTuHienThi
        });
    }

    [HttpDelete("{productId:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int productId, int imageId)
    {
        var image = await _dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.MaAnhSanPham == imageId && i.MaSanPham == productId);

        if (image == null)
        {
            return NotFound();
        }

        _dbContext.ProductImages.Remove(image);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}

public class VariantRequest
{
    public string? TenBienThe { get; set; }
    public string? Sku { get; set; }
    public string? PhienBan { get; set; }
    public string? MauSac { get; set; }
    public decimal? GiaGhiDe { get; set; }
    public int? SoLuongTon { get; set; }
    public string? TrangThai { get; set; }
}

public class UpdateProductRequest
{
    public string? MaSanPhamKinhDoanh { get; set; }
    public string? TenSanPham { get; set; }
    public string? Slug { get; set; }
    public string? LoaiSanPham { get; set; }
    public int? MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string? MoTaNgan { get; set; }
    public decimal? GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public int? SoLuongTon { get; set; }
    public string? AnhChinhUrl { get; set; }
    public string? TrangThaiSanPham { get; set; }
    public bool? DangHoatDong { get; set; }
}

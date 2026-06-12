using CatalogService.Data;
using CatalogService.DTOs.PartCompatibilities;
using CatalogService.DTOs.Products;
using CatalogService.Entities;
using CatalogService.Services;
using System.Security.Claims;
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
    private readonly IAuditLogService _auditLog;

    public ProductsController(
        ICatalogService catalogService,
        CatalogDbContext dbContext,
        IImageStorageService imageStorage,
        IAuditLogService auditLog)
    {
        _catalogService = catalogService;
        _dbContext = dbContext;
        _imageStorage = imageStorage;
        _auditLog = auditLog;
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

    [Authorize(Roles = "Admin,Staff")]
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

        var businessRuleError = await ValidateProductBusinessRulesAsync(
            request.LoaiSanPham,
            request.MaDanhMuc,
            request.MaHangXe,
            request.MaDongXe);
        if (businessRuleError is not null)
        {
            return BadRequest(new { message = businessRuleError });
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
            LoaiSanPham = NormalizeProductType(request.LoaiSanPham),
            MaDanhMuc = request.MaDanhMuc.Value,
            MaHangXe = NormalizeProductType(request.LoaiSanPham) == "PhuTung" ? null : request.MaHangXe,
            MaDongXe = NormalizeProductType(request.LoaiSanPham) == "PhuTung" ? null : request.MaDongXe,
            MoTaNgan = request.MoTaNgan,
            AnhChinhUrl = request.AnhChinhUrl,
            TrangThaiSanPham = string.IsNullOrWhiteSpace(request.TrangThaiSanPham) ? "Available" : request.TrangThaiSanPham,
            DangHoatDong = request.DangHoatDong ?? true,
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Product", product.MaSanPham.ToString(), "Create", null, new
        {
            product.MaSanPham,
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.LoaiSanPham,
            product.MaDanhMuc,
            product.MaHangXe,
            product.MaDongXe,
            product.TrangThaiSanPham,
            product.DangHoatDong
        });

        return CreatedAtAction(nameof(GetProductById), new { id = product.MaSanPham }, new { id = product.MaSanPham });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product == null) return NotFound();
        var oldValue = new
        {
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.Slug,
            product.LoaiSanPham,
            product.MaDanhMuc,
            product.MaHangXe,
            product.MaDongXe,
            product.MoTaNgan,
            product.AnhChinhUrl,
            product.TrangThaiSanPham,
            product.DangHoatDong
        };

        var nextType = string.IsNullOrWhiteSpace(request.LoaiSanPham) ? product.LoaiSanPham : request.LoaiSanPham;
        var nextCategoryId = request.MaDanhMuc ?? product.MaDanhMuc;
        // UPDATE-only clear semantics: an explicit non-positive id means "clear the field".
        int? requestedBrandId = request.MaHangXe.HasValue
            ? (request.MaHangXe.Value > 0 ? request.MaHangXe.Value : (int?)null)
            : product.MaHangXe;
        int? requestedModelId = request.MaDongXe.HasValue
            ? (request.MaDongXe.Value > 0 ? request.MaDongXe.Value : (int?)null)
            : product.MaDongXe;
        var nextBrandId = NormalizeProductType(nextType) == "PhuTung" ? null : requestedBrandId;
        var nextModelId = NormalizeProductType(nextType) == "PhuTung" ? null : requestedModelId;

        var businessRuleError = await ValidateProductBusinessRulesAsync(nextType, nextCategoryId, nextBrandId, nextModelId);
        if (businessRuleError is not null)
        {
            return BadRequest(new { message = businessRuleError });
        }

        if (request.TenSanPham != null) product.TenSanPham = request.TenSanPham.Trim();
        if (request.Slug != null) product.Slug = request.Slug.Trim();
        if (request.MaSanPhamKinhDoanh != null) product.MaSanPhamKinhDoanh = request.MaSanPhamKinhDoanh.Trim();
        if (request.LoaiSanPham != null) product.LoaiSanPham = NormalizeProductType(request.LoaiSanPham);
        if (request.MaDanhMuc.HasValue) product.MaDanhMuc = request.MaDanhMuc.Value;
        if (NormalizeProductType(product.LoaiSanPham) == "PhuTung")
        {
            product.MaHangXe = null;
            product.MaDongXe = null;
        }
        else
        {
            if (request.MaHangXe.HasValue) product.MaHangXe = request.MaHangXe.Value > 0 ? request.MaHangXe.Value : null;
            if (request.MaDongXe.HasValue) product.MaDongXe = request.MaDongXe.Value > 0 ? request.MaDongXe.Value : null;
        }
        if (request.MoTaNgan != null) product.MoTaNgan = request.MoTaNgan;
        if (request.AnhChinhUrl != null) product.AnhChinhUrl = request.AnhChinhUrl;
        if (request.TrangThaiSanPham != null) product.TrangThaiSanPham = request.TrangThaiSanPham;
        if (request.DangHoatDong.HasValue) product.DangHoatDong = request.DangHoatDong.Value;
        product.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Product", product.MaSanPham.ToString(), "Update", oldValue, new
        {
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.Slug,
            product.LoaiSanPham,
            product.MaDanhMuc,
            product.MaHangXe,
            product.MaDongXe,
            product.MoTaNgan,
            product.AnhChinhUrl,
            product.TrangThaiSanPham,
            product.DangHoatDong
        });
        return Ok(new { id = product.MaSanPham, message = "Cập nhật sản phẩm thành công." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product == null) return NotFound();
        var oldValue = new
        {
            product.MaSanPham,
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.LoaiSanPham,
            product.MaDanhMuc,
            product.TrangThaiSanPham,
            product.DangHoatDong
        };

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
        await _auditLog.WriteAsync(this, "Product", id.ToString(), "Delete", oldValue, null, "Xoa cung san pham");

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

    [HttpGet("{partId:int}/compatibilities")]
    public async Task<IActionResult> GetPartCompatibilities(int partId)
    {
        var part = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.MaSanPham == partId);
        if (part is null)
        {
            return NotFound(new { message = "Khong tim thay phu tung." });
        }

        var rows = await (
            from compatibility in _dbContext.PartCompatibilities.AsNoTracking()
            join brandJoin in _dbContext.Brands.AsNoTracking()
                on compatibility.MaHangXe equals brandJoin.MaHangXe into brands
            from brand in brands.DefaultIfEmpty()
            join modelJoin in _dbContext.VehicleModels.AsNoTracking()
                on compatibility.MaDongXe equals modelJoin.MaDongXe into models
            from model in models.DefaultIfEmpty()
            where compatibility.MaPhuTung == partId
            orderby compatibility.ApDungTatCaXe descending, brand.TenHang, model.TenDongXe, compatibility.NamTu
            select new
            {
                compatibility.MaTuongThich,
                compatibility.MaPhuTung,
                compatibility.MaHangXe,
                tenHang = brand == null ? null : brand.TenHang,
                compatibility.MaDongXe,
                tenDongXe = model == null ? null : model.TenDongXe,
                compatibility.NamTu,
                compatibility.NamDen,
                compatibility.ApDungTatCaXe,
                compatibility.GhiChu,
                compatibility.DangHoatDong
            }).ToListAsync();

        return Ok(rows);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{partId:int}/compatibilities")]
    public async Task<IActionResult> CreatePartCompatibility(int partId, [FromBody] PartCompatibilityCreateDto request)
    {
        request.MaPhuTung = partId;
        var error = await ValidatePartCompatibilityRequestAsync(request.MaPhuTung, request.MaHangXe, request.MaDongXe, request.NamTu, request.NamDen, request.ApDungTatCaXe);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        await NormalizePartProductTypeAsync(partId);
        var normalized = await NormalizePartCompatibilityTargetAsync(request.MaHangXe, request.MaDongXe, request.ApDungTatCaXe);
        var now = DateTime.UtcNow;
        var compatibility = new PartCompatibility
        {
            MaPhuTung = partId,
            MaHangXe = normalized.BrandId,
            MaDongXe = normalized.ModelId,
            NamTu = request.NamTu,
            NamDen = request.NamDen,
            ApDungTatCaXe = request.ApDungTatCaXe,
            GhiChu = TrimToNull(request.GhiChu),
            DangHoatDong = request.DangHoatDong,
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.PartCompatibilities.Add(compatibility);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "PartCompatibility", compatibility.MaTuongThich.ToString(), "Create", null, compatibility);
        return CreatedAtAction(nameof(GetPartCompatibilities), new { partId }, new { id = compatibility.MaTuongThich });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{partId:int}/compatibilities/{id:int}")]
    public async Task<IActionResult> UpdatePartCompatibility(int partId, int id, [FromBody] PartCompatibilityUpdateDto request)
    {
        var compatibility = await _dbContext.PartCompatibilities.FirstOrDefaultAsync(c => c.MaTuongThich == id && c.MaPhuTung == partId);
        if (compatibility is null)
        {
            return NotFound(new { message = "Khong tim thay cau hinh tuong thich." });
        }
        var oldValue = new
        {
            compatibility.MaPhuTung,
            compatibility.MaHangXe,
            compatibility.MaDongXe,
            compatibility.NamTu,
            compatibility.NamDen,
            compatibility.ApDungTatCaXe,
            compatibility.GhiChu,
            compatibility.DangHoatDong
        };

        request.MaPhuTung = partId;
        var error = await ValidatePartCompatibilityRequestAsync(request.MaPhuTung, request.MaHangXe, request.MaDongXe, request.NamTu, request.NamDen, request.ApDungTatCaXe);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        await NormalizePartProductTypeAsync(partId);
        var normalized = await NormalizePartCompatibilityTargetAsync(request.MaHangXe, request.MaDongXe, request.ApDungTatCaXe);
        compatibility.MaHangXe = normalized.BrandId;
        compatibility.MaDongXe = normalized.ModelId;
        compatibility.NamTu = request.NamTu;
        compatibility.NamDen = request.NamDen;
        compatibility.ApDungTatCaXe = request.ApDungTatCaXe;
        compatibility.GhiChu = TrimToNull(request.GhiChu);
        compatibility.DangHoatDong = request.DangHoatDong;
        compatibility.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "PartCompatibility", compatibility.MaTuongThich.ToString(), "Update", oldValue, compatibility);
        return Ok(new { id = compatibility.MaTuongThich });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{partId:int}/compatibilities/{id:int}")]
    public async Task<IActionResult> DeletePartCompatibility(int partId, int id)
    {
        var compatibility = await _dbContext.PartCompatibilities.FirstOrDefaultAsync(c => c.MaTuongThich == id && c.MaPhuTung == partId);
        if (compatibility is null)
        {
            return NotFound();
        }
        var oldValue = new
        {
            compatibility.MaTuongThich,
            compatibility.MaPhuTung,
            compatibility.MaHangXe,
            compatibility.MaDongXe,
            compatibility.NamTu,
            compatibility.NamDen,
            compatibility.ApDungTatCaXe,
            compatibility.GhiChu,
            compatibility.DangHoatDong
        };

        _dbContext.PartCompatibilities.Remove(compatibility);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "PartCompatibility", id.ToString(), "Delete", oldValue, null);
        return NoContent();
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
                giaGoc = v.GiaGoc,
                giaKhuyenMai = v.GiaKhuyenMai,
                soLuongTon = v.SoLuongTon,
                trangThai = v.TrangThai,
                ngayTao = v.NgayTao
            })
            .ToListAsync();

        return Ok(variants);
    }

    [Authorize(Roles = "Admin,Staff")]
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
            GiaGoc = request.GiaGoc ?? 0m,
            GiaKhuyenMai = request.GiaKhuyenMai.HasValue && request.GiaKhuyenMai.Value > 0 ? request.GiaKhuyenMai : null,
            SoLuongTon = 0,
            TrangThai = request.TrangThai ?? "Available",
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductVariant", variant.MaBienSanPham.ToString(), "Create", null, variant);

        var initialStock = request.SoLuongTon ?? 0;
        if (initialStock > 0)
        {
            var product = await _dbContext.Products.FirstAsync(p => p.MaSanPham == productId);
            await EnsureInventoryAuditTableAsync();
            await ApplyStockMovementAsync(product.MaSanPham, variant.MaBienSanPham, "TonDauKy", initialStock, "Ton kho ban dau khi tao bien the", "ProductVariantCreate", variant.MaBienSanPham);
            await InsertInventoryAuditLogAsync(product, variant, "Initial", initialStock, 0, initialStock, "Ton kho ban dau khi tao bien the");
        }
        return Ok(new { id = variant.MaBienSanPham, message = "Thêm biến thể thành công." });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> UpdateVariant(int productId, int variantId, [FromBody] VariantRequest request)
    {
        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == variantId && v.MaSanPham == productId);
        if (variant == null) return NotFound();
        var oldValue = new
        {
            variant.TenBienThe,
            variant.SKU,
            variant.PhienBan,
            variant.MauSac,
            variant.GiaGoc,
            variant.GiaKhuyenMai,
            variant.TrangThai
        };

        if (request.TenBienThe != null) variant.TenBienThe = request.TenBienThe.Trim();
        if (request.Sku != null) variant.SKU = request.Sku.Trim();
        if (request.PhienBan != null) variant.PhienBan = request.PhienBan.Trim();
        if (request.MauSac != null) variant.MauSac = request.MauSac.Trim();
        if (request.GiaGoc.HasValue) variant.GiaGoc = request.GiaGoc.Value;
        if (request.GiaKhuyenMai.HasValue) variant.GiaKhuyenMai = request.GiaKhuyenMai.Value > 0 ? request.GiaKhuyenMai.Value : null;
        if (request.TrangThai != null) variant.TrangThai = request.TrangThai;
        variant.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductVariant", variant.MaBienSanPham.ToString(), "Update", oldValue, variant);
        return Ok(new { id = variant.MaBienSanPham, message = "Cập nhật biến thể thành công." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}/variants/{variantId:int}")]
    public async Task<IActionResult> DeleteVariant(int productId, int variantId)
    {
        var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == variantId && v.MaSanPham == productId);
        if (variant == null) return NotFound();
        var oldValue = new
        {
            variant.MaBienSanPham,
            variant.MaSanPham,
            variant.TenBienThe,
            variant.SKU,
            variant.TrangThai
        };

        _dbContext.ProductVariants.Remove(variant);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductVariant", variantId.ToString(), "Delete", oldValue, null);
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

    [Authorize(Roles = "Admin,Staff")]
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
            var owningProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == productId);
            if (owningProduct != null && existingImg.MaBienSanPham == null)
            {
                owningProduct.AnhChinhUrl = existingImg.UrlAnh;
                owningProduct.NgayCapNhat = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
            await _auditLog.WriteAsync(this, "ProductImage", existingImg.MaAnhSanPham.ToString(), "SetMain", null, new { existingImg.MaAnhSanPham, existingImg.UrlAnh, existingImg.MaBienSanPham });
            return Ok(new { id = existingImg.MaAnhSanPham, laAnhChinh = true });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn file ảnh." });
        }

        // Verify product exists (and grab it for AltText + main-image update)
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == productId);
        if (product == null)
        {
            return NotFound(new { message = "Sản phẩm không tồn tại." });
        }

        ProductVariant? variant = null;
        if (maBienSanPham.HasValue)
        {
            variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(v => v.MaBienSanPham == maBienSanPham.Value && v.MaSanPham == productId);
            if (variant == null)
            {
                return BadRequest(new { message = "Biến thể không thuộc sản phẩm này." });
            }
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

        ProductImage image;
        try
        {
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

            var altText = variant != null
                ? $"{product.TenSanPham} - {variant.TenBienThe}"
                : product.TenSanPham;

            image = new ProductImage
            {
                MaSanPham = productId,
                MaBienSanPham = maBienSanPham,
                UrlAnh = url,
                AltText = altText,
                LaAnhChinh = isMain,
                ThuTuHienThi = maxOrder + 1,
                NgayTao = DateTime.UtcNow
            };

            _dbContext.ProductImages.Add(image);

            if (isMain && maBienSanPham == null)
            {
                product.AnhChinhUrl = image.UrlAnh;
                product.NgayCapNhat = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // Rollback the file we just wrote so we don't leak orphans.
            _imageStorage.DeleteImage(url);
            throw;
        }

        await _auditLog.WriteAsync(this, "ProductImage", image.MaAnhSanPham.ToString(), "Create", null, new { image.MaAnhSanPham, image.MaSanPham, image.MaBienSanPham, image.UrlAnh, image.LaAnhChinh });

        return Ok(new
        {
            id = image.MaAnhSanPham,
            urlAnh = image.UrlAnh,
            maBienSanPham = image.MaBienSanPham,
            laAnhChinh = image.LaAnhChinh,
            thuTuHienThi = image.ThuTuHienThi
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int productId, int imageId)
    {
        var image = await _dbContext.ProductImages
            .FirstOrDefaultAsync(i => i.MaAnhSanPham == imageId && i.MaSanPham == productId);

        if (image == null)
        {
            return NotFound();
        }
        var oldValue = new { image.MaAnhSanPham, image.MaSanPham, image.MaBienSanPham, image.UrlAnh, image.LaAnhChinh };

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == productId);
        var deletedImageWasMain = image.LaAnhChinh;
        var shouldReplaceProductMainImage = product?.AnhChinhUrl == image.UrlAnh || (image.MaBienSanPham == null && image.LaAnhChinh);
        var deletedVariantId = image.MaBienSanPham;

        _dbContext.ProductImages.Remove(image);

        if (deletedImageWasMain)
        {
            var replacementImage = await _dbContext.ProductImages
                .Where(i => i.MaSanPham == productId && i.MaAnhSanPham != imageId && i.MaBienSanPham == deletedVariantId)
                .OrderByDescending(i => i.LaAnhChinh)
                .ThenBy(i => i.ThuTuHienThi)
                .ThenBy(i => i.MaAnhSanPham)
                .FirstOrDefaultAsync();

            if (replacementImage != null)
            {
                replacementImage.LaAnhChinh = true;
            }

            if (shouldReplaceProductMainImage && product != null)
            {
                product.AnhChinhUrl = replacementImage?.UrlAnh;
                product.NgayCapNhat = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync();

        // Best-effort cleanup of the physical file. DB is the source of truth — leftover files are
        // recoverable via a janitor, but a missing DB row with a live URL is worse than a leftover file.
        _imageStorage.DeleteImage(oldValue.UrlAnh);

        await _auditLog.WriteAsync(this, "ProductImage", imageId.ToString(), "Delete", oldValue, null);

        return NoContent();
    }

    private async Task EnsureInventoryAuditTableAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.TONKHO_DIEUCHINH_LOG', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_DIEUCHINH_LOG (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaSanPham INT NOT NULL,
                    MaBienSanPham INT NULL,
                    MaSanPhamKinhDoanh NVARCHAR(50) NOT NULL,
                    SKU NVARCHAR(80) NULL,
                    TenSanPham NVARCHAR(255) NOT NULL,
                    TenBienThe NVARCHAR(180) NULL,
                    LoaiGiaoDich VARCHAR(20) NOT NULL,
                    SoLuongThayDoi INT NOT NULL,
                    TonTruoc INT NOT NULL,
                    TonSau INT NOT NULL,
                    LyDo NVARCHAR(500) NOT NULL,
                    MaNguoiDung INT NULL,
                    NgayTao DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_TONKHO_DIEUCHINH_LOG_Target
                    ON dbo.TONKHO_DIEUCHINH_LOG (MaSanPham, MaBienSanPham, NgayTao DESC);
            END;
            """);
    }

    private async Task<string?> ValidateProductBusinessRulesAsync(string? productType, int? categoryId, int? brandId, int? modelId)
    {
        if (!string.IsNullOrWhiteSpace(productType)
            && !string.Equals(productType, "XeMay", StringComparison.OrdinalIgnoreCase)
            && !IsPartProductType(productType))
        {
            return "Loai san pham khong hop le.";
        }

        var normalizedType = NormalizeProductType(productType);

        if (!categoryId.HasValue)
        {
            return "Danh muc la bat buoc.";
        }

        if (!await CategoryBelongsToProductTypeAsync(categoryId.Value, normalizedType))
        {
            return normalizedType == "XeMay"
                ? "Danh muc phai thuoc nhom Xe may."
                : "Danh muc phai thuoc nhom Phu tung.";
        }

        if (normalizedType == "PhuTung" && (brandId.HasValue || modelId.HasValue))
        {
            return "Phu tung khong gan truc tiep voi hang xe/dong xe trong form san pham.";
        }

        if (normalizedType == "XeMay" && modelId.HasValue)
        {
            var model = await _dbContext.VehicleModels
                .AsNoTracking()
                .Where(m => m.MaDongXe == modelId.Value)
                .Select(m => new { m.MaHangXe, m.LoaiXe })
                .FirstOrDefaultAsync();
            if (model is null)
            {
                return "Dong xe khong hop le.";
            }
            if (brandId.HasValue && model.MaHangXe != brandId.Value)
            {
                return "Dong xe khong thuoc hang xe da chon.";
            }

            // Ràng buộc nhất quán: nếu danh mục xác định được loại xe (xe số/tay ga/côn tay/điện)
            // và dòng xe đã có loại xe cụ thể thì hai bên phải khớp nhau.
            var categoryLoaiXe = await GetCategoryVehicleTypeAsync(categoryId.Value);
            if (categoryLoaiXe is not null
                && !string.Equals(model.LoaiXe, "Khac", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(categoryLoaiXe, model.LoaiXe, StringComparison.OrdinalIgnoreCase))
            {
                return $"Danh muc khong khop loai xe cua dong xe (dong xe thuoc loai {model.LoaiXe}).";
            }
        }

        return null;
    }

    private static readonly Dictionary<string, string> CategorySlugToLoaiXe = new(StringComparer.OrdinalIgnoreCase)
    {
        ["xe-so"] = "XeSo",
        ["xe-tay-ga"] = "TayGa",
        ["xe-con-tay"] = "ConTay",
        ["xe-dien"] = "XeDien",
    };

    // Suy ra loại xe (XeSo/TayGa/ConTay/XeDien) từ danh mục hoặc danh mục cha gần nhất.
    // Trả null nếu danh mục không thuộc nhóm loại xe nào (không ràng buộc).
    private async Task<string?> GetCategoryVehicleTypeAsync(int categoryId)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(c => new { c.MaDanhMuc, c.MaDanhMucCha, c.Slug })
            .ToListAsync();

        var current = categories.FirstOrDefault(c => c.MaDanhMuc == categoryId);
        var guard = 0;
        while (current is not null && guard++ < 50)
        {
            if (!string.IsNullOrWhiteSpace(current.Slug)
                && CategorySlugToLoaiXe.TryGetValue(current.Slug.Trim(), out var loaiXe))
            {
                return loaiXe;
            }
            if (!current.MaDanhMucCha.HasValue)
            {
                break;
            }
            current = categories.FirstOrDefault(c => c.MaDanhMuc == current.MaDanhMucCha.Value);
        }

        return null;
    }

    private async Task<string?> ValidatePartCompatibilityRequestAsync(
        int partId,
        int? brandId,
        int? modelId,
        short? yearFrom,
        short? yearTo,
        bool appliesToAllVehicles)
    {
        var part = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.MaSanPham == partId);
        if (part is null)
        {
            return "Khong tim thay phu tung.";
        }

        var isPart = IsPartProductType(part.LoaiSanPham) || await CategoryBelongsToProductTypeAsync(part.MaDanhMuc, "PhuTung");
        if (!isPart)
        {
            return "San pham nay khong thuoc nhom phu tung.";
        }

        if (yearFrom.HasValue && (yearFrom.Value < 1950 || yearFrom.Value > 2100))
        {
            return "Nam bat dau khong hop le.";
        }

        if (yearTo.HasValue && (yearTo.Value < 1950 || yearTo.Value > 2100))
        {
            return "Nam ket thuc khong hop le.";
        }

        if (yearFrom.HasValue && yearTo.HasValue && yearFrom.Value > yearTo.Value)
        {
            return "Nam bat dau khong duoc lon hon nam ket thuc.";
        }

        if (appliesToAllVehicles)
        {
            return null;
        }

        if (!brandId.HasValue && !modelId.HasValue)
        {
            return "Vui long chon hang xe hoac dong xe tuong thich.";
        }

        if (brandId.HasValue && !await _dbContext.Brands.AnyAsync(b => b.MaHangXe == brandId.Value))
        {
            return "Hang xe khong hop le.";
        }

        if (modelId.HasValue)
        {
            var model = await _dbContext.VehicleModels.AsNoTracking().FirstOrDefaultAsync(m => m.MaDongXe == modelId.Value);
            if (model is null)
            {
                return "Dong xe khong hop le.";
            }
            if (brandId.HasValue && model.MaHangXe != brandId.Value)
            {
                return "Dong xe khong thuoc hang xe da chon.";
            }
        }

        return null;
    }

    private async Task<(int? BrandId, int? ModelId)> NormalizePartCompatibilityTargetAsync(int? brandId, int? modelId, bool appliesToAllVehicles)
    {
        if (appliesToAllVehicles)
        {
            return (null, null);
        }

        if (modelId.HasValue && !brandId.HasValue)
        {
            brandId = await _dbContext.VehicleModels
                .AsNoTracking()
                .Where(m => m.MaDongXe == modelId.Value)
                .Select(m => (int?)m.MaHangXe)
                .FirstOrDefaultAsync();
        }

        return (brandId, modelId);
    }

    private async Task NormalizePartProductTypeAsync(int partId)
    {
        var part = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == partId);
        if (part is null || IsPartProductType(part.LoaiSanPham))
        {
            return;
        }

        if (await CategoryBelongsToProductTypeAsync(part.MaDanhMuc, "PhuTung"))
        {
            part.LoaiSanPham = "PhuTung";
            part.MaHangXe = null;
            part.MaDongXe = null;
            part.NgayCapNhat = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task<bool> CategoryBelongsToProductTypeAsync(int categoryId, string productType)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(c => new { c.MaDanhMuc, c.MaDanhMucCha, c.TenDanhMuc, c.Slug })
            .ToListAsync();

        var rootSlugs = productType == "XeMay"
            ? new[] { "xe-may" }
            : new[] { "phu-tung", "phu-kien" };

        var rootNames = productType == "XeMay"
            ? new[] { "xe may" }
            : new[] { "phu tung", "phu kien" };

        var rootIds = categories
            .Where(c => c.MaDanhMucCha == null
                && (rootSlugs.Contains((c.Slug ?? string.Empty).Trim().ToLowerInvariant())
                    || rootNames.Contains(NormalizeText(c.TenDanhMuc))))
            .Select(c => c.MaDanhMuc)
            .ToHashSet();

        // Không tìm thấy danh mục gốc cho loại sản phẩm -> coi như không hợp lệ (thay vì bỏ qua âm thầm).
        if (rootIds.Count == 0)
        {
            return false;
        }

        var current = categories.FirstOrDefault(c => c.MaDanhMuc == categoryId);
        while (current is not null)
        {
            if (rootIds.Contains(current.MaDanhMuc))
            {
                return true;
            }
            if (!current.MaDanhMucCha.HasValue)
            {
                return false;
            }
            current = categories.FirstOrDefault(c => c.MaDanhMuc == current.MaDanhMucCha.Value);
        }

        return false;
    }

    private static string NormalizeProductType(string? productType)
    {
        // Nhận diện mọi biến thể "phụ tùng/phụ kiện" -> 'PhuTung'.
        // Tránh việc giá trị hợp lệ như 'PhuKien'/'PhuTungXeMay' bị ép sai thành 'XeMay'.
        return IsPartProductType(productType) ? "PhuTung" : "XeMay";
    }

    private static bool IsPartProductType(string? productType)
    {
        return string.Equals(productType, "PhuTung", StringComparison.OrdinalIgnoreCase)
            || string.Equals(productType, "PhuKien", StringComparison.OrdinalIgnoreCase)
            || string.Equals(productType, "PhuTungXeMay", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray();
        return new string(chars).Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd');
    }

    private async Task InsertInventoryAuditLogAsync(Product product, ProductVariant? variant, string type, int delta, int before, int after, string reason)
    {
        int? variantId = variant?.MaBienSanPham;
        string? sku = variant?.SKU;
        string? variantName = variant?.TenBienThe;
        var userId = GetCurrentUserId();

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.TONKHO_DIEUCHINH_LOG
                (MaSanPham, MaBienSanPham, MaSanPhamKinhDoanh, SKU, TenSanPham, TenBienThe, LoaiGiaoDich, SoLuongThayDoi, TonTruoc, TonSau, LyDo, MaNguoiDung, NgayTao)
            VALUES
                ({product.MaSanPham}, {variantId}, {product.MaSanPhamKinhDoanh}, {sku}, {product.TenSanPham}, {variantName}, {type}, {delta}, {before}, {after}, {reason}, {userId}, SYSDATETIME())
            """);
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }

    private Task ApplyStockMovementAsync(
        int maSanPham,
        int? maBienSanPham,
        string loaiBienDong,
        int soLuongThayDoi,
        string? lyDo,
        string? loaiThamChieu,
        int? maThamChieu)
    {
        var userId = GetCurrentUserId();
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            EXEC dbo.sp_TONKHO_ApDungBienDong
                @MaSanPham = {maSanPham},
                @MaBienSanPham = {maBienSanPham},
                @LoaiBienDong = {loaiBienDong},
                @SoLuongThayDoi = {soLuongThayDoi},
                @LyDo = {lyDo},
                @LoaiThamChieu = {loaiThamChieu},
                @MaThamChieu = {maThamChieu},
                @MaNguoiThucHien = {userId}
            """);
    }

    // ===== San pham ban kem / lien quan =====

    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelatedProducts(int id)
    {
        await CatalogSchema.EnsureRelatedTableAsync(_dbContext);

        var rels = await _dbContext.SanPhamLienQuans.AsNoTracking()
            .Where(r => r.MaSanPham == id)
            .OrderBy(r => r.ThuTuHienThi).ThenBy(r => r.MaLienQuan)
            .ToListAsync();

        var relatedIds = rels.Select(r => r.MaSanPhamLienQuan).Distinct().ToList();
        var products = await _dbContext.Products.AsNoTracking()
            .Where(p => relatedIds.Contains(p.MaSanPham))
            .ToDictionaryAsync(p => p.MaSanPham);

        // Giá & tồn tổng hợp từ biến thể (giá thật nằm ở BIENSANPHAM).
        var variantAgg = await _dbContext.ProductVariants.AsNoTracking()
            .Where(v => relatedIds.Contains(v.MaSanPham))
            .GroupBy(v => v.MaSanPham)
            .Select(g => new
            {
                MaSanPham = g.Key,
                TongTon = g.Sum(x => x.SoLuongTon ?? 0),
                ListPrice = g.Min(x => (decimal?)x.GiaGoc) ?? 0,
                SalePrice = g.Min(x => (decimal?)(x.GiaKhuyenMai ?? x.GiaGoc))
            })
            .ToDictionaryAsync(x => x.MaSanPham);

        var items = rels.Select(r =>
        {
            products.TryGetValue(r.MaSanPhamLienQuan, out var p);
            variantAgg.TryGetValue(r.MaSanPhamLienQuan, out var agg);
            return new
            {
                id = r.MaLienQuan,
                relatedProductId = r.MaSanPhamLienQuan,
                relationType = r.LoaiLienQuan,
                note = r.GhiChu,
                sortOrder = r.ThuTuHienThi,
                relatedProductCode = p?.MaSanPhamKinhDoanh,
                relatedProductName = p?.TenSanPham,
                stockTotal = agg?.TongTon ?? 0,
                listPrice = agg?.ListPrice ?? 0,
                salePrice = agg?.SalePrice
            };
        }).ToList();

        return Ok(new { items });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:int}/related")]
    public async Task<IActionResult> CreateRelatedItem(int id, [FromBody] SaveRelatedItemRequest request)
    {
        await CatalogSchema.EnsureRelatedTableAsync(_dbContext);

        if (request.RelatedProductId == id)
        {
            return BadRequest(new { message = "Khong the chon chinh san pham hien tai." });
        }
        if (!await _dbContext.Products.AnyAsync(p => p.MaSanPham == request.RelatedProductId))
        {
            return BadRequest(new { message = "San pham lien quan khong ton tai." });
        }
        if (await _dbContext.SanPhamLienQuans.AnyAsync(r => r.MaSanPham == id && r.MaSanPhamLienQuan == request.RelatedProductId))
        {
            return BadRequest(new { message = "San pham nay da duoc cau hinh ban kem." });
        }

        var entity = new SanPhamLienQuan
        {
            MaSanPham = id,
            MaSanPhamLienQuan = request.RelatedProductId,
            LoaiLienQuan = NormalizeRelationType(request.RelationType),
            GhiChu = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            ThuTuHienThi = request.SortOrder,
            DangHoatDong = true,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        _dbContext.SanPhamLienQuans.Add(entity);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "SanPhamLienQuan", entity.MaLienQuan.ToString(), "Create", null, new { entity.MaSanPham, entity.MaSanPhamLienQuan, entity.LoaiLienQuan });
        return CreatedAtAction(nameof(GetRelatedProducts), new { id }, new { id = entity.MaLienQuan });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}/related/{relatedId:int}")]
    public async Task<IActionResult> UpdateRelatedItem(int id, int relatedId, [FromBody] SaveRelatedItemRequest request)
    {
        await CatalogSchema.EnsureRelatedTableAsync(_dbContext);

        var entity = await _dbContext.SanPhamLienQuans.FirstOrDefaultAsync(r => r.MaLienQuan == relatedId && r.MaSanPham == id);
        if (entity is null) return NotFound();

        entity.LoaiLienQuan = NormalizeRelationType(request.RelationType);
        entity.GhiChu = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        entity.ThuTuHienThi = request.SortOrder;
        entity.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "SanPhamLienQuan", entity.MaLienQuan.ToString(), "Update", null, new { entity.LoaiLienQuan, entity.GhiChu, entity.ThuTuHienThi });
        return Ok(new { id = entity.MaLienQuan });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}/related/{relatedId:int}")]
    public async Task<IActionResult> DeleteRelatedItem(int id, int relatedId)
    {
        await CatalogSchema.EnsureRelatedTableAsync(_dbContext);

        var entity = await _dbContext.SanPhamLienQuans.FirstOrDefaultAsync(r => r.MaLienQuan == relatedId && r.MaSanPham == id);
        if (entity is null) return NotFound();
        _dbContext.SanPhamLienQuans.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "SanPhamLienQuan", relatedId.ToString(), "Delete", new { entity.MaSanPham, entity.MaSanPhamLienQuan }, null);
        return NoContent();
    }

    private static string NormalizeRelationType(string? value)
    {
        var v = (value ?? "").Trim();
        return v is "Accessory" or "Bundle" or "Alternative" ? v : "Accessory";
    }

    // ===== Khuyen mai dang ap dung (doc tu VOUCHER) =====

    [HttpGet("{id:int}/promotions")]
    public async Task<IActionResult> GetApplicableVouchers(int id)
    {
        if (!await _dbContext.Products.AnyAsync(p => p.MaSanPham == id))
        {
            return NotFound();
        }

        try
        {
            var rows = await _dbContext.Database.SqlQueryRaw<PromotionRow>("""
                SELECT MaVoucher AS Id, MaVoucherCode AS Code, PhamViApDung AS ScopeType,
                       LoaiGiamGia AS DiscountType, GiaTriGiam AS DiscountValue, GiaTriGiamToiDa AS MaxDiscount,
                       GiaTriDonToiThieu AS MinOrderValue, NgayBatDau AS StartAt, NgayKetThuc AS EndAt,
                       SoLanDaDung AS UsedCount, GioiHanSuDung AS UsageLimit
                FROM dbo.VOUCHER
                WHERE DangHoatDong = 1
                  AND (NgayKetThuc IS NULL OR NgayKetThuc >= SYSUTCDATETIME())
                ORDER BY NgayKetThuc
                """).ToListAsync();

            var items = rows.Select(r => new
            {
                id = r.Id,
                code = r.Code,
                scopeType = string.IsNullOrWhiteSpace(r.ScopeType) ? "All" : r.ScopeType,
                refId = (int?)null,
                discountType = r.DiscountType,
                discountValue = r.DiscountValue,
                maxDiscount = r.MaxDiscount,
                minOrderValue = r.MinOrderValue,
                startAt = r.StartAt,
                endAt = r.EndAt,
                usedCount = r.UsedCount,
                usageLimit = r.UsageLimit
            });
            return Ok(new { items });
        }
        catch
        {
            // Bang VOUCHER chua san sang -> tra danh sach rong, khong lam vo trang quan tri.
            return Ok(new { items = Array.Empty<object>() });
        }
    }

    // ===== Tuoi ton kho theo bien the =====

    [HttpGet("{id:int}/inventory-aging")]
    public async Task<IActionResult> GetInventoryAging(int id)
    {
        if (!await _dbContext.Products.AnyAsync(p => p.MaSanPham == id))
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var variants = await _dbContext.ProductVariants.AsNoTracking()
            .Where(v => v.MaSanPham == id)
            .OrderBy(v => v.SKU)
            .ToListAsync();

        var items = variants.Select(v =>
        {
            var onHand = v.SoLuongTon ?? 0;
            var daysInStock = (int)Math.Max(0, (now - v.NgayTao).TotalDays);
            string aging = onHand <= 0
                ? "Hết hàng"
                : daysInStock >= 180 ? "Tồn chậm"
                : daysInStock >= 90 ? "Cần theo dõi"
                : "Bình thường";
            return new
            {
                skuId = v.MaBienSanPham,
                skuCode = v.SKU,
                variantName = v.TenBienThe,
                onHand,
                reserved = 0,
                available = onHand,
                firstStockAt = (DateTime?)v.NgayTao,
                lastStockInAt = (DateTime?)v.NgayCapNhat,
                lastSoldAt = (DateTime?)null,
                daysInStock,
                daysSinceLastSale = 0,
                agingStatus = aging
            };
        }).ToList();

        return Ok(new { items });
    }

    // ===== Ma vach (in tem) =====

    [HttpGet("{id:int}/barcodes")]
    public async Task<IActionResult> GetBarcodes(int id)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.MaSanPham == id);
        if (product is null)
        {
            return NotFound();
        }

        var items = await _dbContext.ProductVariants.AsNoTracking()
            .Where(v => v.MaSanPham == id)
            .OrderBy(v => v.SKU)
            .Select(v => new
            {
                skuId = v.MaBienSanPham,
                skuCode = v.SKU,
                productName = product.TenSanPham,
                variantName = v.TenBienThe,
                barcode = v.SKU,
                price = v.GiaKhuyenMai ?? v.GiaGoc
            })
            .ToListAsync();

        return Ok(new { items });
    }
}

public class SaveRelatedItemRequest
{
    public int RelatedProductId { get; set; }
    public string? RelationType { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}

public class PromotionRow
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? ScopeType { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int UsedCount { get; set; }
    public int? UsageLimit { get; set; }
}

public class VariantRequest
{
    public string? TenBienThe { get; set; }
    public string? Sku { get; set; }
    public string? PhienBan { get; set; }
    public string? MauSac { get; set; }
    public decimal? GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
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
    public string? AnhChinhUrl { get; set; }
    public string? TrangThaiSanPham { get; set; }
    public bool? DangHoatDong { get; set; }
}

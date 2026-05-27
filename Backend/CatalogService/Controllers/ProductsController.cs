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
        await _auditLog.WriteAsync(this, "Product", product.MaSanPham.ToString(), "Create", null, new
        {
            product.MaSanPham,
            product.MaSanPhamKinhDoanh,
            product.TenSanPham,
            product.LoaiSanPham,
            product.MaDanhMuc,
            product.MaHangXe,
            product.MaDongXe,
            product.GiaGoc,
            product.GiaKhuyenMai,
            product.TrangThaiSanPham,
            product.DangHoatDong
        });

        if (product.SoLuongTon > 0)
        {
            await EnsureInventoryAuditTableAsync();
            await InsertInventoryAuditLogAsync(product, null, "Initial", product.SoLuongTon, 0, product.SoLuongTon, "Ton kho ban dau khi tao san pham");
        }

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
            product.GiaGoc,
            product.GiaKhuyenMai,
            product.AnhChinhUrl,
            product.TrangThaiSanPham,
            product.DangHoatDong
        };

        var nextType = string.IsNullOrWhiteSpace(request.LoaiSanPham) ? product.LoaiSanPham : request.LoaiSanPham;
        var nextCategoryId = request.MaDanhMuc ?? product.MaDanhMuc;
        var nextBrandId = NormalizeProductType(nextType) == "PhuTung" ? null : request.MaHangXe ?? product.MaHangXe;
        var nextModelId = NormalizeProductType(nextType) == "PhuTung" ? null : request.MaDongXe ?? product.MaDongXe;

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
            if (request.MaHangXe.HasValue) product.MaHangXe = request.MaHangXe;
            if (request.MaDongXe.HasValue) product.MaDongXe = request.MaDongXe;
        }
        if (request.MoTaNgan != null) product.MoTaNgan = request.MoTaNgan;
        if (request.GiaGoc.HasValue) product.GiaGoc = request.GiaGoc.Value;
        if (request.GiaKhuyenMai.HasValue) product.GiaKhuyenMai = request.GiaKhuyenMai;
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
            product.GiaGoc,
            product.GiaKhuyenMai,
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
                giaGhiDe = v.GiaGhiDe,
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
            GiaGhiDe = request.GiaGhiDe,
            SoLuongTon = request.SoLuongTon ?? 0,
            TrangThai = request.TrangThai ?? "Available",
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductVariant", variant.MaBienSanPham.ToString(), "Create", null, variant);

        if ((variant.SoLuongTon ?? 0) > 0)
        {
            var product = await _dbContext.Products.FirstAsync(p => p.MaSanPham == productId);
            await EnsureInventoryAuditTableAsync();
            await InsertInventoryAuditLogAsync(product, variant, "Initial", variant.SoLuongTon ?? 0, 0, variant.SoLuongTon ?? 0, "Ton kho ban dau khi tao bien the");
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
            variant.GiaGhiDe,
            variant.TrangThai
        };

        if (request.TenBienThe != null) variant.TenBienThe = request.TenBienThe.Trim();
        if (request.Sku != null) variant.SKU = request.Sku.Trim();
        if (request.PhienBan != null) variant.PhienBan = request.PhienBan.Trim();
        if (request.MauSac != null) variant.MauSac = request.MauSac.Trim();
        if (request.GiaGhiDe.HasValue) variant.GiaGhiDe = request.GiaGhiDe;
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
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == productId);
            if (product != null)
            {
                product.AnhChinhUrl = existingImg.UrlAnh;
                product.NgayCapNhat = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
            await _auditLog.WriteAsync(this, "ProductImage", existingImg.MaAnhSanPham.ToString(), "SetMain", null, new { existingImg.MaAnhSanPham, existingImg.UrlAnh, existingImg.MaBienSanPham });
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

        if (isMain)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == productId);
            if (product != null)
            {
                product.AnhChinhUrl = image.UrlAnh;
                product.NgayCapNhat = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync();
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
        var shouldReplaceProductMainImage = product?.AnhChinhUrl == image.UrlAnh;

        _dbContext.ProductImages.Remove(image);

        if (shouldReplaceProductMainImage && product != null)
        {
            var replacementImage = await _dbContext.ProductImages
                .Where(i => i.MaSanPham == productId && i.MaAnhSanPham != imageId)
                .OrderByDescending(i => i.LaAnhChinh)
                .ThenBy(i => i.MaBienSanPham.HasValue)
                .ThenBy(i => i.ThuTuHienThi)
                .FirstOrDefaultAsync();

            product.AnhChinhUrl = replacementImage?.UrlAnh;
            product.NgayCapNhat = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
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
            && !string.Equals(productType, "PhuTung", StringComparison.OrdinalIgnoreCase))
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
                .Select(m => new { m.MaHangXe })
                .FirstOrDefaultAsync();
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
            .Select(c => new { c.MaDanhMuc, c.MaDanhMucCha, c.TenDanhMuc })
            .ToListAsync();

        var rootNames = productType == "XeMay"
            ? new[] { "xe may" }
            : new[] { "phu tung", "phu kien" };

        var rootIds = categories
            .Where(c => c.MaDanhMucCha == null && rootNames.Contains(NormalizeText(c.TenDanhMuc)))
            .Select(c => c.MaDanhMuc)
            .ToHashSet();

        if (rootIds.Count == 0)
        {
            return true;
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
        return string.Equals(productType, "PhuTung", StringComparison.OrdinalIgnoreCase)
            ? "PhuTung"
            : "XeMay";
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

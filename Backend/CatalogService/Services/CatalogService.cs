using CatalogService.Data;
using CatalogService.DTOs.Brands;
using CatalogService.DTOs.Categories;
using CatalogService.DTOs.Common;
using CatalogService.DTOs.ProductImages;
using CatalogService.DTOs.Products;
using CatalogService.DTOs.ProductVariants;
using CatalogService.DTOs.VehicleModels;
using CatalogService.Entities;
using CatalogService.Repositories.Brands;
using CatalogService.Repositories.Categories;
using CatalogService.Repositories.ProductImages;
using CatalogService.Repositories.Products;
using CatalogService.Repositories.ProductVariants;
using CatalogService.Repositories.VehicleModels;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IVehicleModelRepository _vehicleModelRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly CatalogDbContext _dbContext;

    public CatalogService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IVehicleModelRepository vehicleModelRepository,
        IProductImageRepository productImageRepository,
        IProductVariantRepository productVariantRepository,
        CatalogDbContext dbContext)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _vehicleModelRepository = vehicleModelRepository;
        _productImageRepository = productImageRepository;
        _productVariantRepository = productVariantRepository;
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(bool activeOnly = true)
    {
        var categories = activeOnly
            ? await _categoryRepository.GetActiveAsync()
            : await _categoryRepository.GetAllAsync();

        return categories.Select(MapCategory).ToList();
    }

    public async Task<List<BrandDto>> GetBrandsAsync(bool activeOnly = true)
    {
        var brands = activeOnly
            ? await _brandRepository.GetActiveAsync()
            : await _brandRepository.GetAllAsync();

        return brands.Select(MapBrand).ToList();
    }

    public async Task<List<VehicleModelDto>> GetVehicleModelsAsync(int? maHangXe = null, bool activeOnly = true)
    {
        var vehicleModels = maHangXe.HasValue
            ? await _vehicleModelRepository.GetByBrandIdAsync(maHangXe.Value, activeOnly)
            : activeOnly
                ? await _vehicleModelRepository.GetActiveAsync()
                : await _vehicleModelRepository.GetAllAsync();

        return vehicleModels.Select(MapVehicleModel).ToList();
    }

    public async Task<PagedResultDto<ProductListItemDto>> GetProductsAsync(ProductSearchDto search)
    {
        var products = await _productRepository.GetProductsAsync(search);
        var totalItems = await _productRepository.CountProductsAsync(search);
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = ProductSearchDto.NormalizePageSize(search.PageSize);

        var productIds = products.Select(p => p.MaSanPham).ToList();
        var imageMap = await _productImageRepository.GetPrimaryImageUrlsAsync(productIds);
        var brandNameMap = await GetBrandNameMapAsync(products.Where(p => p.MaHangXe.HasValue).Select(p => p.MaHangXe!.Value));
        var categoryNameMap = await GetCategoryNameMapAsync(products.Select(p => p.MaDanhMuc));
        var variantSummaryMap = await GetVariantPriceSummaryMapAsync(productIds);
        var reviewSummaryMap = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.MaSanPham) && r.TrangThai == "Approved")
            .GroupBy(r => r.MaSanPham)
            .Select(g => new
            {
                MaSanPham = g.Key,
                TongDanhGia = g.Count(),
                DiemTrungBinh = g.Average(r => r.Diem)
            })
            .ToDictionaryAsync(
                item => item.MaSanPham,
                item => new ReviewSummary(item.TongDanhGia, item.DiemTrungBinh));

        return new PagedResultDto<ProductListItemDto>
        {
            Items = products.Select(p => MapProductListItem(p, imageMap, reviewSummaryMap, brandNameMap, categoryNameMap, variantSummaryMap)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int maSanPham)
    {
        var product = await _productRepository.GetByIdAsync(maSanPham);
        return product is null ? null : await MapProductDetailAsync(product);
    }

    private async Task<ProductDetailDto> MapProductDetailAsync(Product product)
    {
        var variants = await _productVariantRepository.GetByProductIdAsync(product.MaSanPham);
        var images = await _productImageRepository.GetByProductIdAsync(product.MaSanPham);
        var brandNameMap = await GetBrandNameMapAsync(product.MaHangXe.HasValue ? new[] { product.MaHangXe.Value } : Array.Empty<int>());
        var categoryNameMap = await GetCategoryNameMapAsync(new[] { product.MaDanhMuc });
        var priceSummary = SummarizeVariants(variants.Select(ToVariantPriceRow));

        return new ProductDetailDto
        {
            MaSanPham = product.MaSanPham,
            TenSanPham = product.TenSanPham,
            Slug = product.Slug,
            MaDanhMuc = product.MaDanhMuc,
            TenDanhMuc = categoryNameMap.TryGetValue(product.MaDanhMuc, out var tenDanhMuc) ? tenDanhMuc : null,
            MaHangXe = product.MaHangXe,
            TenHangXe = product.MaHangXe.HasValue && brandNameMap.TryGetValue(product.MaHangXe.Value, out var tenHangXe) ? tenHangXe : null,
            MaDongXe = product.MaDongXe,
            MoTaNgan = product.MoTaNgan,
            MoTa = product.MoTa,
            GiaThapNhat = priceSummary.GiaThapNhat,
            GiaCaoNhat = priceSummary.GiaCaoNhat,
            GiaBan = priceSummary.GiaThapNhat,
            TyLeGiam = priceSummary.TyLeGiam,
            TongTon = priceSummary.TongTon,
            DangHoatDong = product.DangHoatDong,
            BienThe = variants.Select(MapProductVariant).ToList(),
            Anh = images.Select(MapProductImage).ToList()
        };
    }

    private static ProductListItemDto MapProductListItem(
        Product product,
        Dictionary<int, string> imageMap,
        IReadOnlyDictionary<int, ReviewSummary> reviewSummaryMap,
        IReadOnlyDictionary<int, string> brandNameMap,
        IReadOnlyDictionary<int, string> categoryNameMap,
        IReadOnlyDictionary<int, VariantPriceSummary> variantSummaryMap)
    {
        imageMap.TryGetValue(product.MaSanPham, out var anhChinhUrl);
        reviewSummaryMap.TryGetValue(product.MaSanPham, out var reviewSummary);
        variantSummaryMap.TryGetValue(product.MaSanPham, out var priceSummary);

        return new ProductListItemDto
        {
            MaSanPham = product.MaSanPham,
            MaSanPhamKinhDoanh = product.MaSanPhamKinhDoanh,
            TenSanPham = product.TenSanPham,
            Slug = product.Slug,
            MaDanhMuc = product.MaDanhMuc,
            TenDanhMuc = categoryNameMap.TryGetValue(product.MaDanhMuc, out var tenDanhMuc) ? tenDanhMuc : null,
            MaHangXe = product.MaHangXe,
            TenHangXe = product.MaHangXe.HasValue && brandNameMap.TryGetValue(product.MaHangXe.Value, out var tenHangXe) ? tenHangXe : null,
            MaDongXe = product.MaDongXe,
            LoaiSanPham = product.LoaiSanPham,
            GiaThapNhat = priceSummary.GiaThapNhat,
            GiaGocThapNhat = priceSummary.GiaGocThapNhat,
            GiaBan = priceSummary.GiaThapNhat,
            TyLeGiam = priceSummary.TyLeGiam,
            TongTon = priceSummary.TongTon,
            SoBienThe = priceSummary.SoBienThe,
            TrangThaiSanPham = product.TrangThaiSanPham,
            AnhChinhUrl = anhChinhUrl ?? product.AnhChinhUrl,
            DiemTrungBinh = reviewSummary.AverageRating,
            TongDanhGia = reviewSummary.TotalReviews
        };
    }

    private static CategoryDto MapCategory(Category category)
    {
        return new CategoryDto
        {
            MaDanhMuc = category.MaDanhMuc,
            MaDanhMucCha = category.MaDanhMucCha,
            TenDanhMuc = category.TenDanhMuc,
            Slug = category.Slug,
            MoTa = category.MoTa,
            AnhDaiDienUrl = category.AnhDaiDienUrl,
            ThuTuHienThi = category.ThuTuHienThi,
            DangHoatDong = category.DangHoatDong
        };
    }

    private static BrandDto MapBrand(Brand brand)
    {
        return new BrandDto
        {
            MaHangXe = brand.MaHangXe,
            TenHang = brand.TenHang,
            Slug = brand.Slug,
            LogoUrl = brand.LogoUrl,
            DangHoatDong = brand.DangHoatDong
        };
    }

    private static VehicleModelDto MapVehicleModel(VehicleModel vehicleModel)
    {
        return new VehicleModelDto
        {
            MaDongXe = vehicleModel.MaDongXe,
            MaHangXe = vehicleModel.MaHangXe,
            TenDongXe = vehicleModel.TenDongXe,
            Slug = vehicleModel.Slug,
            LoaiXe = vehicleModel.LoaiXe,
            DangHoatDong = vehicleModel.DangHoatDong
        };
    }

    private static ProductVariantDto MapProductVariant(ProductVariant variant)
    {
        return new ProductVariantDto
        {
            MaBienSanPham = variant.MaBienSanPham,
            MaSanPham = variant.MaSanPham,
            TenBienThe = variant.TenBienThe,
            SKU = variant.SKU,
            GiaGoc = variant.GiaGoc,
            GiaKhuyenMai = variant.GiaKhuyenMai,
            GiaBan = VariantSellPrice(variant.GiaGoc, variant.GiaKhuyenMai),
            TyLeGiam = VariantDiscount(variant.GiaGoc, variant.GiaKhuyenMai),
            SoLuongTon = variant.SoLuongTon,
            TrangThai = variant.TrangThai,
            PhienBan = variant.PhienBan,
            MauSac = variant.MauSac
        };
    }

    private static ProductImageDto MapProductImage(ProductImage image)
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

    private async Task<Dictionary<int, string>> GetBrandNameMapAsync(IEnumerable<int> brandIds)
    {
        var ids = brandIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, string>();

        return await _dbContext.Brands
            .AsNoTracking()
            .Where(b => ids.Contains(b.MaHangXe))
            .ToDictionaryAsync(b => b.MaHangXe, b => b.TenHang);
    }

    private async Task<Dictionary<int, string>> GetCategoryNameMapAsync(IEnumerable<int> categoryIds)
    {
        var ids = categoryIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, string>();

        return await _dbContext.Categories
            .AsNoTracking()
            .Where(c => ids.Contains(c.MaDanhMuc))
            .ToDictionaryAsync(c => c.MaDanhMuc, c => c.TenDanhMuc);
    }

    // Giá hiệu lực của biến thể = GiaKhuyenMai ?? GiaGoc.
    private static decimal VariantSellPrice(decimal giaGoc, decimal? giaKhuyenMai)
        => giaKhuyenMai ?? giaGoc;

    private static decimal? VariantDiscount(decimal giaGoc, decimal? giaKhuyenMai)
    {
        if (!giaKhuyenMai.HasValue || giaGoc <= 0 || giaKhuyenMai.Value >= giaGoc)
        {
            return null;
        }

        return Math.Round((giaGoc - giaKhuyenMai.Value) * 100m / giaGoc, 1, MidpointRounding.AwayFromZero);
    }

    private static readonly HashSet<string> InactiveVariantStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "inactive", "hidden", "discontinued", "stopped", "off", "ngung ban", "ngừng bán"
    };

    private static bool IsSelling(string? trangThai)
        => string.IsNullOrWhiteSpace(trangThai) || !InactiveVariantStatuses.Contains(trangThai.Trim());

    private static VariantPriceRow ToVariantPriceRow(ProductVariant variant)
        => new(variant.GiaGoc, variant.GiaKhuyenMai, variant.SoLuongTon, variant.TrangThai);

    // Tổng hợp giá "Từ {thấp nhất}" từ các biến thể đang bán (fallback toàn bộ nếu không có biến thể nào đang bán).
    private static VariantPriceSummary SummarizeVariants(IEnumerable<VariantPriceRow> rows)
    {
        var all = rows.ToList();
        if (all.Count == 0)
        {
            return new VariantPriceSummary(0m, 0m, 0m, null, 0, 0);
        }

        var selling = all.Where(r => IsSelling(r.TrangThai)).ToList();
        var pool = selling.Count > 0 ? selling : all;

        var cheapest = pool
            .OrderBy(r => VariantSellPrice(r.GiaGoc, r.GiaKhuyenMai))
            .First();
        var minSell = VariantSellPrice(cheapest.GiaGoc, cheapest.GiaKhuyenMai);
        var maxSell = pool.Max(r => VariantSellPrice(r.GiaGoc, r.GiaKhuyenMai));

        return new VariantPriceSummary(
            GiaThapNhat: minSell,
            GiaGocThapNhat: cheapest.GiaGoc,
            GiaCaoNhat: maxSell,
            TyLeGiam: VariantDiscount(cheapest.GiaGoc, cheapest.GiaKhuyenMai),
            TongTon: all.Sum(r => r.SoLuongTon ?? 0),
            SoBienThe: all.Count);
    }

    private async Task<Dictionary<int, VariantPriceSummary>> GetVariantPriceSummaryMapAsync(IReadOnlyCollection<int> productIds)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<int, VariantPriceSummary>();
        }

        var rows = await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => productIds.Contains(v.MaSanPham))
            .Select(v => new { v.MaSanPham, v.GiaGoc, v.GiaKhuyenMai, v.SoLuongTon, v.TrangThai })
            .ToListAsync();

        return rows
            .GroupBy(r => r.MaSanPham)
            .ToDictionary(
                g => g.Key,
                g => SummarizeVariants(g.Select(r => new VariantPriceRow(r.GiaGoc, r.GiaKhuyenMai, r.SoLuongTon, r.TrangThai))));
    }

    private readonly record struct ReviewSummary(int TotalReviews, double AverageRating);

    private readonly record struct VariantPriceRow(decimal GiaGoc, decimal? GiaKhuyenMai, int? SoLuongTon, string? TrangThai);

    private readonly record struct VariantPriceSummary(
        decimal GiaThapNhat,
        decimal GiaGocThapNhat,
        decimal GiaCaoNhat,
        decimal? TyLeGiam,
        int TongTon,
        int SoBienThe);
}

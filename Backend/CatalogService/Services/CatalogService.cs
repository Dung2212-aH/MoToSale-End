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

namespace CatalogService.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IVehicleModelRepository _vehicleModelRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductVariantRepository _productVariantRepository;

    public CatalogService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IVehicleModelRepository vehicleModelRepository,
        IProductImageRepository productImageRepository,
        IProductVariantRepository productVariantRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _vehicleModelRepository = vehicleModelRepository;
        _productImageRepository = productImageRepository;
        _productVariantRepository = productVariantRepository;
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

        return new PagedResultDto<ProductListItemDto>
        {
            Items = products.Select(MapProductListItem).ToList(),
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

        return new ProductDetailDto
        {
            MaSanPham = product.MaSanPham,
            TenSanPham = product.TenSanPham,
            Slug = product.Slug,
            MaDanhMuc = product.MaDanhMuc,
            MaHangXe = product.MaHangXe,
            MaDongXe = product.MaDongXe,
            MoTaNgan = product.MoTaNgan,
            MoTa = product.MoTa,
            GiaGoc = product.GiaGoc,
            GiaKhuyenMai = product.GiaKhuyenMai,
            GiaBan = GetSalePrice(product),
            TyLeGiam = GetDiscountPercent(product),
            SoLuongTon = product.SoLuongTon,
            AnhChinhUrl = product.AnhChinhUrl,
            DangHoatDong = product.DangHoatDong,
            BienThe = variants.Select(MapProductVariant).ToList(),
            Anh = images.Select(MapProductImage).ToList()
        };
    }

    private static ProductListItemDto MapProductListItem(Product product)
    {
        return new ProductListItemDto
        {
            MaSanPham = product.MaSanPham,
            MaSanPhamKinhDoanh = product.MaSanPhamKinhDoanh,
            TenSanPham = product.TenSanPham,
            Slug = product.Slug,
            MaDanhMuc = product.MaDanhMuc,
            MaHangXe = product.MaHangXe,
            MaDongXe = product.MaDongXe,
            LoaiSanPham = product.LoaiSanPham,
            GiaGoc = product.GiaGoc,
            GiaKhuyenMai = product.GiaKhuyenMai,
            GiaBan = GetSalePrice(product),
            TyLeGiam = GetDiscountPercent(product),
            SoLuongTon = product.SoLuongTon,
            AnhChinhUrl = product.AnhChinhUrl,
            TrangThaiSanPham = product.TrangThaiSanPham
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
            GiaGhiDe = variant.GiaGhiDe,
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

    private static decimal GetSalePrice(Product product)
    {
        return product.GiaKhuyenMai ?? product.GiaGoc;
    }

    private static int? GetDiscountPercent(Product product)
    {
        if (!product.GiaKhuyenMai.HasValue || product.GiaGoc <= 0 || product.GiaKhuyenMai.Value >= product.GiaGoc)
        {
            return null;
        }

        return (int)Math.Round((product.GiaGoc - product.GiaKhuyenMai.Value) * 100 / product.GiaGoc);
    }
}

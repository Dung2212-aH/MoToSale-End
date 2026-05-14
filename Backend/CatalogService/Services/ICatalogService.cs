using CatalogService.DTOs.Brands;
using CatalogService.DTOs.Categories;
using CatalogService.DTOs.Common;
using CatalogService.DTOs.Products;
using CatalogService.DTOs.VehicleModels;

namespace CatalogService.Services;

public interface ICatalogService
{
    // Lay danh sach danh muc.
    Task<List<CategoryDto>> GetCategoriesAsync(bool activeOnly = true);

    // Lay danh sach hang xe.
    Task<List<BrandDto>> GetBrandsAsync(bool activeOnly = true);

    // Lay danh sach dong xe, co the loc theo hang xe.
    Task<List<VehicleModelDto>> GetVehicleModelsAsync(int? maHangXe = null, bool activeOnly = true);

    // Tim kiem va phan trang danh sach san pham.
    Task<PagedResultDto<ProductListItemDto>> GetProductsAsync(ProductSearchDto search);

    // Lay chi tiet san pham theo ma.
    Task<ProductDetailDto?> GetProductByIdAsync(int maSanPham);
}

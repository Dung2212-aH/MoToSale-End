using CatalogService.Entities;

namespace CatalogService.Repositories.ProductVariants;

public interface IProductVariantRepository
{
    // Lay danh sach bien the theo san pham.
    Task<List<ProductVariant>> GetByProductIdAsync(int maSanPham);
}

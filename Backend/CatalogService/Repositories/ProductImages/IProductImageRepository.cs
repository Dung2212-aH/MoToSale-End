using CatalogService.Entities;

namespace CatalogService.Repositories.ProductImages;

public interface IProductImageRepository
{
    // Lay danh sach anh theo san pham.
    Task<List<ProductImage>> GetByProductIdAsync(int maSanPham);
    Task<Dictionary<int, string>> GetPrimaryImageUrlsAsync(IEnumerable<int> productIds);
}

using CatalogService.DTOs.Products;
using CatalogService.Entities;

namespace CatalogService.Repositories.Products;

public interface IProductRepository
{
    IQueryable<Product> QueryProducts(ProductSearchDto search);
    Task<int> CountProductsAsync(ProductSearchDto search);
    Task<List<Product>> GetProductsAsync(ProductSearchDto search);
    Task<Product?> GetByIdAsync(int maSanPham);
}

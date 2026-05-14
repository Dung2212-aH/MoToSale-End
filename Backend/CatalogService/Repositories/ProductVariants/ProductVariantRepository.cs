using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories.ProductVariants;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductVariantRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductVariant>> GetByProductIdAsync(int maSanPham)
    {
        return await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => v.MaSanPham == maSanPham)
            .OrderBy(v => v.MaBienSanPham)
            .ToListAsync();
    }

}

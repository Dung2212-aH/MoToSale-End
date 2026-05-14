using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories.ProductImages;

public class ProductImageRepository : IProductImageRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductImageRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProductImage>> GetByProductIdAsync(int maSanPham)
    {
        return await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => i.MaSanPham == maSanPham)
            .OrderByDescending(i => i.LaAnhChinh)
            .ThenBy(i => i.ThuTuHienThi)
            .ThenBy(i => i.MaAnhSanPham)
            .ToListAsync();
    }

}

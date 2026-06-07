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

    public async Task<Dictionary<int, string>> GetPrimaryImageUrlsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        if (!ids.Any()) return new Dictionary<int, string>();

        var images = await _dbContext.ProductImages
            .AsNoTracking()
            .Where(i => ids.Contains(i.MaSanPham))
            .OrderByDescending(i => i.MaBienSanPham != null)
            .ThenByDescending(i => i.LaAnhChinh)
            .ThenBy(i => i.ThuTuHienThi)
            .ThenBy(i => i.MaAnhSanPham)
            .ToListAsync();

        return images
            .GroupBy(i => i.MaSanPham)
            .ToDictionary(g => g.Key, g => g.First().UrlAnh);
    }

}

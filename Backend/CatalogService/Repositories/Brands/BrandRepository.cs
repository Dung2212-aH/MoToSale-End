using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories.Brands;

public class BrandRepository : IBrandRepository
{
    private readonly CatalogDbContext _dbContext;

    public BrandRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Brand>> GetAllAsync()
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .OrderBy(b => b.TenHang)
            .ToListAsync();
    }

    public async Task<List<Brand>> GetActiveAsync()
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .Where(b => b.DangHoatDong)
            .OrderBy(b => b.TenHang)
            .ToListAsync();
    }

}

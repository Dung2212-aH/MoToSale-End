using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories.VehicleModels;

public class VehicleModelRepository : IVehicleModelRepository
{
    private readonly CatalogDbContext _dbContext;

    public VehicleModelRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<VehicleModel>> GetAllAsync()
    {
        return await _dbContext.VehicleModels
            .AsNoTracking()
            .OrderBy(v => v.MaHangXe)
            .ThenBy(v => v.TenDongXe)
            .ToListAsync();
    }

    public async Task<List<VehicleModel>> GetActiveAsync()
    {
        return await _dbContext.VehicleModels
            .AsNoTracking()
            .Where(v => v.DangHoatDong)
            .OrderBy(v => v.MaHangXe)
            .ThenBy(v => v.TenDongXe)
            .ToListAsync();
    }

    public async Task<List<VehicleModel>> GetByBrandIdAsync(int maHangXe, bool activeOnly = true)
    {
        var query = _dbContext.VehicleModels
            .AsNoTracking()
            .Where(v => v.MaHangXe == maHangXe);

        if (activeOnly)
        {
            query = query.Where(v => v.DangHoatDong);
        }

        return await query
            .OrderBy(v => v.TenDongXe)
            .ToListAsync();
    }

}

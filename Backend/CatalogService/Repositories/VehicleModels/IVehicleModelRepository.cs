using CatalogService.Entities;

namespace CatalogService.Repositories.VehicleModels;

public interface IVehicleModelRepository
{
    // Lay tat ca dong xe.
    Task<List<VehicleModel>> GetAllAsync();

    // Lay cac dong xe dang hoat dong.
    Task<List<VehicleModel>> GetActiveAsync();

    // Lay danh sach dong xe theo hang xe.
    Task<List<VehicleModel>> GetByBrandIdAsync(int maHangXe, bool activeOnly = true);
}

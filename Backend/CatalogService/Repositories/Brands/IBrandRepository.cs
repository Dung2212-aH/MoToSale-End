using CatalogService.Entities;

namespace CatalogService.Repositories.Brands;

public interface IBrandRepository
{
    // Lay tat ca hang xe.
    Task<List<Brand>> GetAllAsync();

    // Lay cac hang xe dang hoat dong.
    Task<List<Brand>> GetActiveAsync();
}

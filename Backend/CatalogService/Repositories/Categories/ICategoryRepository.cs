using CatalogService.Entities;

namespace CatalogService.Repositories.Categories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> GetActiveAsync();
}

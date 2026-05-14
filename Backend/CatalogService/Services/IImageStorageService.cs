using Microsoft.AspNetCore.Http;

namespace CatalogService.Services;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}

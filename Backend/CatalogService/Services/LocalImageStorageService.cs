using Microsoft.AspNetCore.Http;

namespace CatalogService.Services;

public class LocalImageStorageService : IImageStorageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private const int MaxDimension = 4000;
    private const string UrlPrefix = "/uploads/";

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalImageStorageService> _logger;

    public LocalImageStorageService(IWebHostEnvironment environment, ILogger<LocalImageStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("File anh khong hop le.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("Dung luong anh khong duoc vuot qua 5MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException("Chi ho tro anh JPG, PNG hoac WEBP.");
        }

        // Buffer once so we can both inspect headers and write to disk without re-reading the request stream.
        using var buffer = new MemoryStream((int)file.Length);
        await file.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.GetBuffer().AsMemory(0, (int)buffer.Length);

        if (!ImageHeaderReader.TryReadDimensions(bytes.Span, out var width, out var height))
        {
            throw new InvalidOperationException("Khong doc duoc kich thuoc anh. File co the bi loi.");
        }

        if (width > MaxDimension || height > MaxDimension)
        {
            throw new InvalidOperationException($"Kich thuoc anh khong duoc vuot qua {MaxDimension}x{MaxDimension} pixel.");
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var safeFolder = folder
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);
        var uploadFolder = Path.Combine(webRoot, "uploads", safeFolder);

        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await stream.WriteAsync(bytes, cancellationToken);
        }

        return $"{UrlPrefix}{safeFolder.Replace(Path.DirectorySeparatorChar, '/')}/{fileName}";
    }

    public bool DeleteImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (!url.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase)) return false;

        var relative = url[UrlPrefix.Length..]
            .Replace('\\', '/')
            .TrimStart('/');

        // Reject any traversal attempts before touching the filesystem.
        if (relative.Length == 0) return false;
        var segments = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (segment == "." || segment == "..") return false;
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
        var target = Path.GetFullPath(Path.Combine(uploadsRoot, Path.Combine(segments)));

        // Defence in depth — ensure the resolved path is still inside uploads/.
        if (!target.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            if (File.Exists(target))
            {
                File.Delete(target);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image file {Path}", target);
        }

        return false;
    }
}

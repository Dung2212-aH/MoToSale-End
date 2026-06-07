using System.Globalization;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services;

/// <summary>
/// Reads operational settings from the shared dbo.HETHONG_CAUHINH key-value table
/// (managed by CatalogService but stored in the same database).
/// </summary>
public class SystemConfigService : ISystemConfigService
{
    private readonly OrderDbContext _dbContext;

    public SystemConfigService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, string?>> GetAllAsync()
    {
        if (await _dbContext.Database
                .SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.HETHONG_CAUHINH', N'U') IS NULL THEN 0 ELSE 1 END AS Value")
                .FirstAsync() == 0)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        var rows = await _dbContext.Database
            .SqlQueryRaw<ConfigRow>("SELECT [Key], [Value] FROM dbo.HETHONG_CAUHINH")
            .ToListAsync();

        return rows
            .GroupBy(r => r.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> GetStringAsync(string key, string? defaultValue = null)
    {
        var all = await GetAllAsync();
        return all.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue)
    {
        var raw = await GetStringAsync(key);
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue)
    {
        var raw = await GetStringAsync(key);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }

    private sealed class ConfigRow
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
    }
}

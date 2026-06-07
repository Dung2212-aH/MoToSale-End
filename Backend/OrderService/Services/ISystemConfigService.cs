namespace OrderService.Services;

public interface ISystemConfigService
{
    Task<IReadOnlyDictionary<string, string?>> GetAllAsync();
    Task<string?> GetStringAsync(string key, string? defaultValue = null);
    Task<decimal> GetDecimalAsync(string key, decimal defaultValue);
    Task<int> GetIntAsync(string key, int defaultValue);
}

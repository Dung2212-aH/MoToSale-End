using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using MoToSale.Common;
using MoToSale.Common.Auth;
using MoToSale.Entities.Catalog;
using MoToSale.Entities.SystemConfig;
using MoToSale.Repository.EFCore;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
[Route("api/operations")]
public class OperationsController : ControllerBase
{
    private readonly IRepository<Store> _stores;
    private readonly IRepository<Setting> _settings;

    public OperationsController(IRepository<Store> stores, IRepository<Setting> settings)
    {
        _stores = stores;
        _settings = settings;
    }

    private static string StoreTypeName(int t) => t switch { 2 => "Warehouse", 3 => "Online", _ => "Showroom" };
    private static int StoreTypeValue(string? s) => s switch { "Warehouse" => 2, "Online" => 3, _ => 1 };

    // ===== Warehouse / showroom (mapped to Store) =====
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var stores = await _stores.GetAllAsync();
        var items = stores.OrderBy(s => s.Id).Select(s => new
        {
            id = s.Id,
            name = s.Name,
            type = StoreTypeName(s.Type),
            addressLine = s.AddressLine,
            phone = s.Phone,
            isActive = s.Status != (int)EntityStatus.Inactive,
            maKho = s.Id,
            tenKho = s.Name,
            loaiKho = StoreTypeName(s.Type),
            diaChi = s.AddressLine,
            hotline = s.Phone,
            dangHoatDong = s.Status != (int)EntityStatus.Inactive,
        });
        return Ok(new { items });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPost("warehouses")]
    public async Task<IActionResult> SaveWarehouse([FromBody] WarehouseRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.ResolvedName)) return BadRequest(new { message = "Warehouse/showroom name is required." });
        Store store;
        if (r.ResolvedId.HasValue)
        {
            store = await _stores.GetByIdAsync(r.ResolvedId.Value) ?? throw new InvalidOperationException();
            store.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            store = new Store { Code = $"KHO{DateTime.UtcNow:yyyyMMddHHmmss}", Slug = "", CreatedDate = DateTime.UtcNow };
            _stores.Add(store);
        }
        store.Name = r.ResolvedName.Trim();
        store.Type = StoreTypeValue(r.ResolvedType);
        store.AddressLine = r.ResolvedAddressLine ?? store.AddressLine ?? "";
        store.Phone = r.ResolvedPhone;
        store.Status = r.ResolvedIsActive ? (int)EntityStatus.Active : (int)EntityStatus.Inactive;
        await _stores.SaveChangesAsync();
        return Ok(new { id = store.Id });
    }

    // ===== System settings (key-value) =====
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var list = await _settings.GetAllAsync();
        var items = list.OrderBy(s => s.Key).Select(s => new { key = s.Key, value = s.Value, description = s.Description, moTa = s.Description });
        return Ok(new { items });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPut("settings")]
    public async Task<IActionResult> SaveSettings([FromBody] SettingsRequest request)
    {
        var all = await _settings.GetAllAsync();
        foreach (var item in request.Items ?? new())
        {
            if (string.IsNullOrWhiteSpace(item.Key)) continue;
            var existing = all.FirstOrDefault(s => s.Key == item.Key.Trim());
            if (existing is null)
            {
                _settings.Add(new Setting { Key = item.Key.Trim(), Value = item.Value, Description = item.ResolvedDescription, CreatedDate = DateTime.UtcNow });
            }
            else
            {
                existing.Value = item.Value; existing.Description = item.ResolvedDescription; existing.UpdatedDate = DateTime.UtcNow;
                _settings.Update(existing);
            }
        }
        await _settings.SaveChangesAsync();
        return Ok(new { message = "Settings saved successfully." });
    }
}

public class WarehouseRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? AddressLine { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("maKho")]
    public int? LegacyId { get; set; }

    [JsonPropertyName("tenKho")]
    public string? LegacyName { get; set; }

    [JsonPropertyName("loaiKho")]
    public string? LegacyType { get; set; }

    [JsonPropertyName("diaChi")]
    public string? LegacyAddressLine { get; set; }

    [JsonPropertyName("hotline")]
    public string? LegacyPhone { get; set; }

    [JsonPropertyName("dangHoatDong")]
    public bool? LegacyIsActive { get; set; }

    [JsonIgnore]
    public int? ResolvedId => Id ?? LegacyId;

    [JsonIgnore]
    public string? ResolvedName => Name ?? LegacyName;

    [JsonIgnore]
    public string? ResolvedType => Type ?? LegacyType;

    [JsonIgnore]
    public string? ResolvedAddressLine => AddressLine ?? LegacyAddressLine;

    [JsonIgnore]
    public string? ResolvedPhone => Phone ?? LegacyPhone;

    [JsonIgnore]
    public bool ResolvedIsActive => LegacyIsActive ?? IsActive;
}

public class SettingsRequest { public List<SettingItem> Items { get; set; } = new(); }
public class SettingItem
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public string? Description { get; set; }

    [JsonPropertyName("moTa")]
    public string? LegacyDescription { get; set; }

    [JsonIgnore]
    public string? ResolvedDescription => Description ?? LegacyDescription;
}

using MoToSale.Common;

namespace MoToSale.Entities.Catalog;

public class Store : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Type { get; set; } = (int)StoreType.Showroom; // StoreType
    public string AddressLine { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? OpeningHours { get; set; }
    public bool IsDefault { get; set; }
}

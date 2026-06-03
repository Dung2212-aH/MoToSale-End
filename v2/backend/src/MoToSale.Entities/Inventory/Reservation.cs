using MoToSale.Common;

namespace MoToSale.Entities.Inventory;

/// <summary>Giữ chỗ tồn kho. StoreId null = giữ ở mức tổng (pool), gán cửa hàng khi phân bổ.</summary>
public class Reservation : BaseEntity
{
    public int OrderId { get; set; }
    public int OrderLineId { get; set; }
    public int SkuId { get; set; }
    public int? StoreId { get; set; }
    public int Qty { get; set; }
    public string ReservationStatus { get; set; } = Common.ReservationStatus.Active;
    public DateTime ExpiresAt { get; set; }
}

using MoToSale.DTO.Common;

namespace MoToSale.DTO.Inventory;

public class InventorySearchRequest : PagingRequest
{
    public int? StoreId { get; set; }
    public bool? LowStockOnly { get; set; }
    public string? StockStatus { get; set; } // InStock | LowStock | OutOfStock
    public bool? HasHold { get; set; }
    public string? SortBy { get; set; }       // available | onHand | reserved | product
    public string? SortDirection { get; set; } // asc | desc
}

public record InventorySummary(int TotalSkus, int OutOfStock, int LowStock, int Holding, int TotalOnHand, int TotalReserved);

public record InventoryListResult(
    IReadOnlyList<InventoryItemDto> Items, int Page, int PageSize, int TotalItems, int TotalPages,
    InventorySummary Summary, DateTime? LastSyncAt);

public record InventoryItemDto(
    int StoreId, string StoreName, int SkuId, string SkuCode, string ProductName,
    int OnHand, int Reserved, int Available, int ReorderPoint, DateTime? UpdatedAt);

public record StockDocumentLineRequest(int SkuId, int Qty, string? Note);

public record CreateStockDocumentRequest(int Type, int StoreId, int? ToStoreId, string? Note, List<StockDocumentLineRequest> Lines, string? Reason = null);

public record StockDocumentLineDto(int Id, int SkuId, string SkuCode, string ProductName, int Qty, string? Note);

public record StockDocumentDto(
    int Id, string Code, int Type, string Status, int StoreId, string StoreName,
    int? ToStoreId, string? ToStoreName, string? Note, DateTime CreatedDate, DateTime? ApprovedAt, int LineCount);

public record StockDocumentDetail(StockDocumentDto Document, IEnumerable<StockDocumentLineDto> Lines);

public record GoodsReceiptDto(
    int Id, string Code, int PurchaseOrderId, string PurchaseOrderCode, string SupplierName,
    int StoreId, string StoreName, string? Note, DateTime ReceivedAt, int LineCount);

public record GoodsReceiptLineDto(int Id, int SkuId, string SkuCode, string ProductName, int Qty, decimal UnitCost);

public record GoodsReceiptDetail(GoodsReceiptDto Receipt, IEnumerable<GoodsReceiptLineDto> Lines);

public record StockMovementDto(
    int Id, int StoreId, int SkuId, int Type, int QtyDelta, int BalanceAfter,
    string? RefType, int? RefId, string? Reason, DateTime OccurredAt,
    string StoreName, string SkuCode, string ProductCode, string ProductName, int BalanceBefore);

/// <summary>Điều chỉnh tồn trực tiếp (không qua phiếu). Type: Import | Export | Adjust.</summary>
public record AdjustStockRequest(int StoreId, int SkuId, string TransactionType, int Qty, string Reason);

public record UpdateThresholdRequest(int StoreId, int SkuId, int ReorderPoint);

public record HoldDto(
    int Id, int OrderId, string? OrderCode, int SkuId, string SkuCode, string ProductName,
    int? StoreId, int Qty, string Status, DateTime ExpiresAt);

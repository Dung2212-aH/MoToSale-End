using MoToSale.DTO.Common;
using MoToSale.DTO.Inventory;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Inventory;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Inventory;

public interface IInventoryRepository : IRepository<InventoryItem>
{
    Task<InventoryItem?> GetItemAsync(int storeId, int skuId);
    Task<InventoryItem> GetOrCreateItemAsync(int storeId, int skuId);
    void AddMovement(StockMovement movement);
    Task<int> GetOnHandTotalAsync(int skuId);
    Task<Dictionary<int, int>> GetOnHandBySkusAsync(IEnumerable<int> skuIds);
    Task<InventorySummary> GetSummaryAsync(int? storeId);
    Task<DateTime?> GetLastUpdatedAtAsync(int? storeId);
    Task<List<StoreStockDto>> GetStoreStockAsync(int skuId);
    Task<int> SyncFromLedgerAsync();
    Task<List<InventoryItem>> GetAllForExportAsync(int? storeId);
    Task<PagingResponse<InventoryItemDto>> SearchAsync(InventorySearchRequest request);
    Task<int> GetTotalAvailableAsync(int skuId);
    Task<List<StockMovementDto>> GetMovementsAsync(int? skuId, int? storeId, int take = 200);
}

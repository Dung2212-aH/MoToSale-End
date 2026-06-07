using MoToSale.Common;
using MoToSale.DTO.Common;
using MoToSale.DTO.Inventory;
using MoToSale.Entities.Inventory;
using MoToSale.Repository.Inventory;

namespace MoToSale.Services.Inventory;

public class InventoryService : IInventoryService
{
    private static readonly Dictionary<string, string> ReceiptReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OpeningBalance"] = "Tồn đầu kỳ",
        ["Supplement"] = "Nhập bù",
        ["Gift"] = "Hàng tặng",
        ["Other"] = "Khác",
    };

    private readonly IInventoryRepository _inv;
    private readonly IStockDocumentRepository _docs;
    private readonly IReservationRepository _reservations;

    public InventoryService(IInventoryRepository inv, IStockDocumentRepository docs, IReservationRepository reservations)
    {
        _inv = inv;
        _docs = docs;
        _reservations = reservations;
    }

    public async Task<InventoryListResult> GetInventoryAsync(InventorySearchRequest request)
    {
        var page = await _inv.SearchAsync(request);
        var summary = await _inv.GetSummaryAsync(request.StoreId);
        var lastSyncAt = await _inv.GetLastUpdatedAtAsync(request.StoreId);
        return new InventoryListResult(page.Items, page.Page, page.PageSize, page.TotalItems, page.TotalPages, summary, lastSyncAt);
    }

    public Task<List<StockMovementDto>> GetMovementsAsync(int? skuId, int? storeId) => _inv.GetMovementsAsync(skuId, storeId);

    public Task<PagingResponse<StockDocumentDto>> SearchDocumentsAsync(PagingRequest request, string? status, int? type) => _docs.SearchAsync(request, status, type);

    public Task<StockDocumentDetail?> GetDocumentAsync(int id) => _docs.GetDetailAsync(id);

    public async Task<int> CreateDocumentAsync(CreateStockDocumentRequest r, int? userId)
    {
        if (r.Lines is null || r.Lines.Count == 0)
        {
            throw new InventoryException("Phiếu kho phải có ít nhất một dòng.");
        }

        if (r.Type == (int)StockDocumentType.Transfer)
        {
            if (!r.ToStoreId.HasValue) throw new InventoryException("Phiếu chuyển kho cần cửa hàng nhận.");
            if (r.ToStoreId == r.StoreId) throw new InventoryException("Cửa hàng nhận phải khác cửa hàng xuất.");
        }

        if (r.Type == (int)StockDocumentType.Receipt && (string.IsNullOrWhiteSpace(r.Reason) || !ReceiptReasons.TryGetValue(r.Reason, out _)))
        {
            throw new InventoryException("Nhập kho khác cần chọn lý do hợp lệ.");
        }

        var now = DateTime.UtcNow;
        var doc = new StockDocument
        {
            Code = $"PK{r.Type}{now:yyyyMMddHHmmss}",
            Type = r.Type,
            DocStatus = StockDocumentStatus.Draft,
            StoreId = r.StoreId,
            ToStoreId = r.Type == (int)StockDocumentType.Transfer ? r.ToStoreId : null,
            Note = r.Type == (int)StockDocumentType.Receipt
                ? $"Lý do nhập kho khác: {ReceiptReasons[r.Reason!]}. {r.Note}".Trim()
                : r.Note,
            CreatedBy = userId,
            CreatedDate = now,
            Lines = r.Lines.Select(l => new StockDocumentLine { SkuId = l.SkuId, Qty = l.Qty, Note = l.Note }).ToList(),
        };

        _docs.Add(doc);
        await _docs.SaveChangesAsync();
        return doc.Id;
    }

    public async Task ApproveDocumentAsync(int id, int? userId)
    {
        var doc = await _docs.GetWithLinesAsync(id) ?? throw new InventoryException("Không tìm thấy phiếu kho.");
        if (doc.DocStatus != StockDocumentStatus.Draft)
        {
            throw new InventoryException("Chỉ phiếu ở trạng thái Nháp mới được duyệt.");
        }

        var now = DateTime.UtcNow;

        foreach (var line in doc.Lines)
        {
            if (line.Qty <= 0) throw new InventoryException("Số lượng dòng phiếu phải lớn hơn 0.");

            switch ((StockDocumentType)doc.Type)
            {
                case StockDocumentType.Receipt:
                    await ApplyAsync(doc.StoreId, line.SkuId, StockMovementType.Receipt, +line.Qty, doc.Id, doc.Code, userId, now);
                    break;
                case StockDocumentType.Issue:
                    await ApplyAsync(doc.StoreId, line.SkuId, StockMovementType.Issue, -line.Qty, doc.Id, doc.Code, userId, now);
                    break;
                case StockDocumentType.Adjustment:
                case StockDocumentType.Stocktake:
                    var item = await _inv.GetOrCreateItemAsync(doc.StoreId, line.SkuId);
                    var delta = line.Qty - item.OnHand; // line.Qty = tồn thực tế đếm được
                    var type = delta >= 0 ? StockMovementType.AdjustIn : StockMovementType.AdjustOut;
                    await ApplyAsync(doc.StoreId, line.SkuId, type, delta, doc.Id, doc.Code, userId, now);
                    break;
                case StockDocumentType.Transfer:
                    await ApplyAsync(doc.StoreId, line.SkuId, StockMovementType.TransferOut, -line.Qty, doc.Id, doc.Code, userId, now);
                    await ApplyAsync(doc.ToStoreId!.Value, line.SkuId, StockMovementType.TransferIn, +line.Qty, doc.Id, doc.Code, userId, now);
                    break;
            }
        }

        doc.DocStatus = StockDocumentStatus.Approved;
        doc.ApprovedBy = userId;
        doc.ApprovedAt = now;
        doc.UpdatedDate = now;
        await _docs.SaveChangesAsync();
    }

    public async Task CancelDocumentAsync(int id)
    {
        var doc = await _docs.GetByIdAsync(id) ?? throw new InventoryException("Không tìm thấy phiếu kho.");
        if (doc.DocStatus != StockDocumentStatus.Draft)
        {
            throw new InventoryException("Chỉ phiếu Nháp mới được hủy.");
        }

        doc.DocStatus = StockDocumentStatus.Cancelled;
        doc.UpdatedDate = DateTime.UtcNow;
        _docs.Update(doc);
        await _docs.SaveChangesAsync();
    }

    public async Task AdjustStockAsync(AdjustStockRequest r, int? userId)
    {
        if (r.Qty <= 0) throw new InventoryException("Số lượng phải lớn hơn 0.");
        if (string.IsNullOrWhiteSpace(r.Reason)) throw new InventoryException("Lý do điều chỉnh là bắt buộc.");
        var now = DateTime.UtcNow;

        switch (r.TransactionType)
        {
            case "Import":
                await ApplyAsync(r.StoreId, r.SkuId, StockMovementType.Receipt, +r.Qty, 0, r.Reason, userId, now);
                break;
            case "Export":
                await ApplyAsync(r.StoreId, r.SkuId, StockMovementType.Issue, -r.Qty, 0, r.Reason, userId, now);
                break;
            case "Adjust":
                var item = await _inv.GetOrCreateItemAsync(r.StoreId, r.SkuId);
                var delta = r.Qty - item.OnHand; // r.Qty = tồn thực tế
                await ApplyAsync(r.StoreId, r.SkuId, delta >= 0 ? StockMovementType.AdjustIn : StockMovementType.AdjustOut, delta, 0, r.Reason, userId, now);
                break;
            default:
                throw new InventoryException("Loại điều chỉnh không hợp lệ.");
        }
        await _inv.SaveChangesAsync();
    }

    public async Task UpdateThresholdAsync(UpdateThresholdRequest r)
    {
        if (r.StoreId <= 0) throw new InventoryException("Cửa hàng/kho là bắt buộc.");
        if (r.SkuId <= 0) throw new InventoryException("SKU là bắt buộc.");
        if (r.ReorderPoint < 0) throw new InventoryException("Ngưỡng phải >= 0.");
        var item = await _inv.GetOrCreateItemAsync(r.StoreId, r.SkuId);
        item.ReorderPoint = r.ReorderPoint;
        item.UpdatedDate = DateTime.UtcNow;
        await _inv.SaveChangesAsync();
    }

    public Task<List<HoldDto>> GetHoldsAsync() => _reservations.GetHoldsAsync();

    public Task<int> SyncAsync() => _inv.SyncFromLedgerAsync();

    public async Task<List<InventoryItemDto>> ExportAsync(int? storeId)
    {
        var page = await _inv.SearchAsync(new InventorySearchRequest { StoreId = storeId, Page = 1, PageSize = 100000 });
        return page.Items.ToList();
    }

    /// <summary>Áp một biến động vào InventoryItem + ghi sổ cái. KHÔNG tự SaveChanges (gộp 1 lần ở caller).</summary>
    private async Task ApplyAsync(int storeId, int skuId, StockMovementType type, int qtyDelta, int docId, string reason, int? userId, DateTime now)
    {
        var item = await _inv.GetOrCreateItemAsync(storeId, skuId);
        var balanceAfter = item.OnHand + qtyDelta;
        if (balanceAfter < 0)
        {
            throw new InventoryException($"Tồn kho không đủ tại cửa hàng #{storeId} cho SKU #{skuId}.");
        }

        item.OnHand = balanceAfter;
        item.UpdatedDate = now;

        _inv.AddMovement(new StockMovement
        {
            StoreId = storeId,
            SkuId = skuId,
            Type = (int)type,
            QtyDelta = qtyDelta,
            BalanceAfter = balanceAfter,
            RefType = "StockDocument",
            RefId = docId,
            Reason = reason,
            PerformedBy = userId,
            OccurredAt = now,
            CreatedDate = now,
        });
    }
}

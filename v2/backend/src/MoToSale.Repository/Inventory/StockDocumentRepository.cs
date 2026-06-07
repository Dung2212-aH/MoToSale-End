using Microsoft.EntityFrameworkCore;
using MoToSale.DTO.Common;
using MoToSale.DTO.Inventory;
using MoToSale.Entities.Inventory;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Inventory;

public class StockDocumentRepository : Repository<StockDocument>, IStockDocumentRepository
{
    public StockDocumentRepository(AppDbContext context) : base(context) { }

    public Task<StockDocument?> GetWithLinesAsync(int id) =>
        Set.Include(d => d.Lines).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<PagingResponse<StockDocumentDto>> SearchAsync(PagingRequest r, string? status, int? type)
    {
        var query =
            from d in Set.AsNoTracking()
            join st in Context.Stores.AsNoTracking() on d.StoreId equals st.Id
            join tst in Context.Stores.AsNoTracking() on d.ToStoreId equals tst.Id into toStores
            from tst in toStores.DefaultIfEmpty()
            select new { d, StoreName = st.Name, ToStoreName = tst != null ? tst.Name : null };

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.d.DocStatus == status);
        if (type.HasValue) query = query.Where(x => x.d.Type == type);

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(x => x.d.Id)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new StockDocumentDto(
                x.d.Id, x.d.Code, x.d.Type, x.d.DocStatus, x.d.StoreId, x.StoreName,
                x.d.ToStoreId, x.ToStoreName, x.d.Note, x.d.CreatedDate, x.d.ApprovedAt, x.d.Lines.Count))
            .ToListAsync();

        return new PagingResponse<StockDocumentDto> { Items = rows, Page = r.Page, PageSize = r.PageSize, TotalItems = total };
    }

    public async Task<StockDocumentDetail?> GetDetailAsync(int id)
    {
        var header = await (
            from d in Set.AsNoTracking()
            join st in Context.Stores.AsNoTracking() on d.StoreId equals st.Id
            join tst in Context.Stores.AsNoTracking() on d.ToStoreId equals tst.Id into toStores
            from tst in toStores.DefaultIfEmpty()
            where d.Id == id
            select new StockDocumentDto(
                d.Id, d.Code, d.Type, d.DocStatus, d.StoreId, st.Name,
                d.ToStoreId, tst != null ? tst.Name : null, d.Note, d.CreatedDate, d.ApprovedAt, d.Lines.Count))
            .FirstOrDefaultAsync();

        if (header is null) return null;

        var lines = await (
            from l in Context.StockDocumentLines.AsNoTracking()
            join s in Context.Skus.AsNoTracking() on l.SkuId equals s.Id
            join p in Context.Products.AsNoTracking() on s.ProductId equals p.Id
            where l.StockDocumentId == id
            select new StockDocumentLineDto(l.Id, l.SkuId, s.SkuCode, p.Name, l.Qty, l.Note))
            .ToListAsync();

        return new StockDocumentDetail(header, lines);
    }
}

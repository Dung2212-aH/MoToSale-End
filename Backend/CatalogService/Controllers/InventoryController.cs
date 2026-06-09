using System.Security.Claims;
using System.Text;
using CatalogService.Data;
using CatalogService.Entities;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private const int DefaultLowStockThreshold = 5;
    private readonly CatalogDbContext _db;
    private readonly IAuditLogService _auditLog;

    public InventoryController(CatalogDbContext db, IAuditLogService auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] InventorySearchRequest request)
    {
        await EnsureSupportTablesAsync();

        var rows = await LoadInventoryRowsAsync();
        var filtered = ApplyFilters(rows, request).ToList();
        var ordered = ApplySort(filtered, request.SortBy, request.SortDirection).ToList();
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var summary = BuildSummary(rows);
        var lastSync = await GetLastSyncAsync();

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalItems = filtered.Count,
            totalPages = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            summary,
            lastSyncAt = lastSync
        });
    }

    [HttpGet("holds")]
    public async Task<IActionResult> GetHolds([FromQuery] int? productId, [FromQuery] int? variantId, [FromQuery] string? status)
    {
        var rows = await _db.Database.SqlQueryRaw<InventoryHoldRow>(
            """
            SELECT
                g.MaGiuCho,
                g.MaDonHang,
                d.MaDonHangKinhDoanh,
                g.MaSanPham,
                sp.MaSanPhamKinhDoanh,
                g.MaBienSanPham,
                bt.SKU,
                sp.TenSanPham,
                bt.TenBienThe,
                g.SoLuong,
                g.TrangThai,
                g.HetHanLuc,
                g.NgayTao,
                g.NgayCapNhat,
                g.GhiChu
            FROM dbo.TONKHO_GIUCHO g
            INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = g.MaSanPham
            LEFT JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = g.MaBienSanPham
            LEFT JOIN dbo.DONHANG d ON d.MaDonHang = g.MaDonHang
            ORDER BY g.NgayTao DESC
            """
        ).ToListAsync();

        if (productId.HasValue)
        {
            rows = rows.Where(x => x.MaSanPham == productId.Value).ToList();
        }

        if (variantId.HasValue)
        {
            rows = rows.Where(x => x.MaBienSanPham == variantId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            rows = rows.Where(x => string.Equals(x.TrangThai, status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(new { items = rows });
    }

    [HttpGet("adjustments")]
    public async Task<IActionResult> GetAdjustments([FromQuery] int? productId, [FromQuery] int? variantId, [FromQuery] string? type)
    {
        await EnsureSupportTablesAsync();

        var rows = await _db.Database.SqlQueryRaw<InventoryAdjustmentRow>(
            """
            SELECT TOP 200
                a.Id,
                a.MaSanPham,
                a.MaBienSanPham,
                a.MaSanPhamKinhDoanh,
                a.SKU,
                a.TenSanPham,
                a.TenBienThe,
                a.LoaiGiaoDich,
                a.SoLuongThayDoi,
                a.TonTruoc,
                a.TonSau,
                a.LyDo,
                a.MaNguoiDung,
                a.NgayTao
            FROM dbo.TONKHO_DIEUCHINH_LOG a
            ORDER BY a.NgayTao DESC, a.Id DESC
            """
        ).ToListAsync();

        if (productId.HasValue)
        {
            rows = rows.Where(x => x.MaSanPham == productId.Value).ToList();
        }

        if (variantId.HasValue)
        {
            rows = rows.Where(x => x.MaBienSanPham == variantId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            rows = rows.Where(x => string.Equals(x.LoaiGiaoDich, type, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(new { items = rows });
    }

    [HttpPut("threshold")]
    public async Task<IActionResult> UpdateThreshold([FromBody] InventoryThresholdRequest request)
    {
        if (request.MaSanPham <= 0)
        {
            return BadRequest(new { message = "MaSanPham khong hop le." });
        }

        if (request.MucCanhBaoTonThap < 0)
        {
            return BadRequest(new { message = "Nguong canh bao phai lon hon hoac bang 0." });
        }

        await EnsureSupportTablesAsync();

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            MERGE dbo.TONKHO_NGUONG_CANHBAO AS target
            USING (SELECT {request.MaSanPham} AS MaSanPham, {request.MaBienSanPham} AS MaBienSanPham) AS source
            ON target.MaSanPham = source.MaSanPham
               AND ISNULL(target.MaBienSanPham, -1) = ISNULL(source.MaBienSanPham, -1)
            WHEN MATCHED THEN
                UPDATE SET MucCanhBaoTonThap = {request.MucCanhBaoTonThap}, NgayCapNhat = SYSDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (MaSanPham, MaBienSanPham, MucCanhBaoTonThap, NgayCapNhat)
                VALUES ({request.MaSanPham}, {request.MaBienSanPham}, {request.MucCanhBaoTonThap}, SYSDATETIME());
            """);
        await _auditLog.WriteAsync(this, "InventoryThreshold", $"{request.MaSanPham}:{request.MaBienSanPham}", "Update", null, request);

        return Ok(new { message = "Cap nhat nguong ton thap thanh cong." });
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] InventoryAdjustRequest request)
    {
        if (request.MaSanPham <= 0)
        {
            return BadRequest(new { message = "MaSanPham khong hop le." });
        }

        if (request.SoLuong <= 0)
        {
            return BadRequest(new { message = "So luong phai lon hon 0." });
        }

        if (string.IsNullOrWhiteSpace(request.LyDo))
        {
            return BadRequest(new { message = "Ly do dieu chinh la bat buoc." });
        }

        var type = NormalizeAdjustmentType(request.LoaiGiaoDich);
        if (type is null)
        {
            return BadRequest(new { message = "Loai giao dich khong hop le." });
        }

        await EnsureSupportTablesAsync();

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        if (request.MaBienSanPham.HasValue)
        {
            var variant = await _db.ProductVariants.FirstOrDefaultAsync(x =>
                x.MaSanPham == request.MaSanPham && x.MaBienSanPham == request.MaBienSanPham.Value);
            if (variant is null)
            {
                return NotFound(new { message = "Khong tim thay bien the." });
            }

            var product = await _db.Products.FirstAsync(x => x.MaSanPham == request.MaSanPham);
            var before = variant.SoLuongTon ?? 0;
            var after = CalculateNewStock(before, request.SoLuong, type);
            if (after < 0)
            {
                return BadRequest(new { message = "Ton kho sau dieu chinh khong duoc am." });
            }

            variant.SoLuongTon = after;
            variant.NgayCapNhat = now;
            await _db.SaveChangesAsync();
            await InsertAdjustmentLogAsync(product, variant, type, after - before, before, after, request.LyDo, userId);
            await _auditLog.WriteAsync(this, "Inventory", $"{request.MaSanPham}:{request.MaBienSanPham}", "Adjust", new { Stock = before }, new { Stock = after, Type = type, Quantity = request.SoLuong }, request.LyDo);
        }
        else
        {
            var product = await _db.Products.FirstOrDefaultAsync(x => x.MaSanPham == request.MaSanPham);
            if (product is null)
            {
                return NotFound(new { message = "Khong tim thay san pham." });
            }

            var before = product.SoLuongTon;
            var after = CalculateNewStock(before, request.SoLuong, type);
            if (after < 0)
            {
                return BadRequest(new { message = "Ton kho sau dieu chinh khong duoc am." });
            }

            product.SoLuongTon = after;
            product.NgayCapNhat = now;
            await _db.SaveChangesAsync();
            await InsertAdjustmentLogAsync(product, null, type, after - before, before, after, request.LyDo, userId);
            await _auditLog.WriteAsync(this, "Inventory", request.MaSanPham.ToString(), "Adjust", new { Stock = before }, new { Stock = after, Type = type, Quantity = request.SoLuong }, request.LyDo);
        }

        await _db.Database.ExecuteSqlRawAsync("EXEC sp_SANPHAM_DongBoTatCaSoLuongTon");
        await transaction.CommitAsync();

        return Ok(new { message = "Dieu chinh ton kho thanh cong." });
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetStockDocuments([FromQuery] string? status, [FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        await EnsureSupportTablesAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _db.Database.SqlQueryRaw<InventoryDocumentRow>(
            """
            SELECT
                p.MaPhieuKho,
                p.MaPhieu,
                p.LoaiPhieu,
                p.TrangThai,
                p.GhiChu,
                p.MaNguoiTao,
                p.MaNguoiDuyet,
                p.NgayTao,
                p.NgayDuyet,
                COUNT(ct.MaChiTietPhieuKho) AS SoDong,
                ISNULL(SUM(ABS(ct.SoLuongThayDoi)), 0) AS TongSoLuong
            FROM dbo.TONKHO_PHIEU p
            LEFT JOIN dbo.TONKHO_PHIEU_CHITIET ct ON ct.MaPhieuKho = p.MaPhieuKho
            GROUP BY p.MaPhieuKho, p.MaPhieu, p.LoaiPhieu, p.TrangThai, p.GhiChu, p.MaNguoiTao, p.MaNguoiDuyet, p.NgayTao, p.NgayDuyet
            ORDER BY p.NgayTao DESC, p.MaPhieuKho DESC
            """
        ).ToListAsync();

        if (!string.IsNullOrWhiteSpace(status))
        {
            rows = rows.Where(x => string.Equals(x.TrangThai, status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            rows = rows.Where(x => string.Equals(x.LoaiPhieu, NormalizeAdjustmentType(type), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = rows.Count;
        var items = rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpGet("documents/{id:int}")]
    public async Task<IActionResult> GetStockDocumentById(int id)
    {
        await EnsureSupportTablesAsync();
        var document = (await _db.Database.SqlQueryRaw<InventoryDocumentRow>(
            $"""
            SELECT
                p.MaPhieuKho,
                p.MaPhieu,
                p.LoaiPhieu,
                p.TrangThai,
                p.GhiChu,
                p.MaNguoiTao,
                p.MaNguoiDuyet,
                p.NgayTao,
                p.NgayDuyet,
                COUNT(ct.MaChiTietPhieuKho) AS SoDong,
                ISNULL(SUM(ABS(ct.SoLuongThayDoi)), 0) AS TongSoLuong
            FROM dbo.TONKHO_PHIEU p
            LEFT JOIN dbo.TONKHO_PHIEU_CHITIET ct ON ct.MaPhieuKho = p.MaPhieuKho
            WHERE p.MaPhieuKho = {id}
            GROUP BY p.MaPhieuKho, p.MaPhieu, p.LoaiPhieu, p.TrangThai, p.GhiChu, p.MaNguoiTao, p.MaNguoiDuyet, p.NgayTao, p.NgayDuyet
            """
        ).ToListAsync()).FirstOrDefault();

        if (document is null)
        {
            return NotFound(new { message = "Khong tim thay phieu kho." });
        }

        var details = await _db.Database.SqlQueryRaw<InventoryDocumentDetailRow>(
            $"""
            SELECT
                MaChiTietPhieuKho,
                MaPhieuKho,
                MaSanPham,
                MaBienSanPham,
                MaSanPhamKinhDoanh,
                SKU,
                TenSanPham,
                TenBienThe,
                TonTruoc,
                SoLuongThayDoi,
                TonSau,
                GhiChu
            FROM dbo.TONKHO_PHIEU_CHITIET
            WHERE MaPhieuKho = {id}
            ORDER BY MaChiTietPhieuKho
            """
        ).ToListAsync();

        return Ok(new { document, details });
    }

    [HttpPost("documents")]
    public async Task<IActionResult> CreateStockDocument([FromBody] InventoryDocumentRequest request)
    {
        var type = NormalizeAdjustmentType(request.LoaiPhieu);
        if (type is null)
        {
            return BadRequest(new { message = "Loai phieu kho khong hop le." });
        }

        if (request.Items.Count == 0)
        {
            return BadRequest(new { message = "Phieu kho phai co it nhat mot dong hang." });
        }

        await EnsureSupportTablesAsync();
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();
        var code = $"PK-{now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.TONKHO_PHIEU (MaPhieu, LoaiPhieu, TrangThai, GhiChu, MaNguoiTao, NgayTao, NgayCapNhat)
            VALUES ({code}, {type}, N'Draft', {TrimToNull(request.GhiChu)}, {userId}, {now}, {now})
            """);

        var documentId = (await _db.Database.SqlQueryRaw<InventoryDocumentIdRow>(
            $"SELECT MaPhieuKho FROM dbo.TONKHO_PHIEU WHERE MaPhieu = N'{code.Replace("'", "''")}'"
        ).ToListAsync()).First().MaPhieuKho;

        foreach (var item in request.Items)
        {
            if (item.MaSanPham <= 0 || item.SoLuong <= 0)
            {
                return BadRequest(new { message = "Dong phieu kho co san pham hoac so luong khong hop le." });
            }

            var product = await _db.Products.FirstOrDefaultAsync(x => x.MaSanPham == item.MaSanPham);
            if (product is null)
            {
                return NotFound(new { message = $"Khong tim thay san pham #{item.MaSanPham}." });
            }

            ProductVariant? variant = null;
            int before;
            if (item.MaBienSanPham.HasValue)
            {
                variant = await _db.ProductVariants.FirstOrDefaultAsync(x => x.MaSanPham == item.MaSanPham && x.MaBienSanPham == item.MaBienSanPham.Value);
                if (variant is null)
                {
                    return NotFound(new { message = $"Khong tim thay bien the #{item.MaBienSanPham}." });
                }

                before = variant.SoLuongTon ?? 0;
            }
            else
            {
                before = product.SoLuongTon;
            }

            var after = CalculateNewStock(before, item.SoLuong, type);
            if (after < 0)
            {
                return BadRequest(new { message = "Ton kho du kien sau duyet khong duoc am." });
            }

            await _db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO dbo.TONKHO_PHIEU_CHITIET
                    (MaPhieuKho, MaSanPham, MaBienSanPham, MaSanPhamKinhDoanh, SKU, TenSanPham, TenBienThe, TonTruoc, SoLuongThayDoi, TonSau, GhiChu)
                VALUES
                    ({documentId}, {product.MaSanPham}, {item.MaBienSanPham}, {product.MaSanPhamKinhDoanh}, {variant?.SKU}, {product.TenSanPham}, {variant?.TenBienThe}, {before}, {after - before}, {after}, {TrimToNull(item.GhiChu)})
                """);
        }

        await _auditLog.WriteAsync(this, "InventoryDocument", documentId.ToString(), "Create", null, new { code, Type = type, request.Items.Count }, request.GhiChu);
        await transaction.CommitAsync();

        return CreatedAtAction(nameof(GetStockDocumentById), new { id = documentId }, new { id = documentId, maPhieu = code, trangThai = "Draft" });
    }

    [HttpPost("documents/{id:int}/approve")]
    public async Task<IActionResult> ApproveStockDocument(int id)
    {
        await EnsureSupportTablesAsync();
        var documents = await _db.Database.SqlQueryRaw<InventoryDocumentRow>($"SELECT MaPhieuKho, MaPhieu, LoaiPhieu, TrangThai, GhiChu, MaNguoiTao, MaNguoiDuyet, NgayTao, NgayDuyet, 0 AS SoDong, 0 AS TongSoLuong FROM dbo.TONKHO_PHIEU WHERE MaPhieuKho = {id}").ToListAsync();
        var document = documents.FirstOrDefault();
        if (document is null)
        {
            return NotFound(new { message = "Khong tim thay phieu kho." });
        }

        if (!string.Equals(document.TrangThai, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Chi phieu kho dang nhap moi duoc duyet." });
        }

        var details = await _db.Database.SqlQueryRaw<InventoryDocumentDetailRow>($"SELECT MaChiTietPhieuKho, MaPhieuKho, MaSanPham, MaBienSanPham, MaSanPhamKinhDoanh, SKU, TenSanPham, TenBienThe, TonTruoc, SoLuongThayDoi, TonSau, GhiChu FROM dbo.TONKHO_PHIEU_CHITIET WHERE MaPhieuKho = {id}").ToListAsync();
        if (details.Count == 0)
        {
            return BadRequest(new { message = "Phieu kho chua co dong hang." });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        foreach (var detail in details)
        {
            var product = await _db.Products.FirstAsync(x => x.MaSanPham == detail.MaSanPham);
            ProductVariant? variant = null;
            int before;
            if (detail.MaBienSanPham.HasValue)
            {
                variant = await _db.ProductVariants.FirstAsync(x => x.MaBienSanPham == detail.MaBienSanPham.Value);
                before = variant.SoLuongTon ?? 0;
            }
            else
            {
                before = product.SoLuongTon;
            }

            var after = before + detail.SoLuongThayDoi;
            if (document.LoaiPhieu.Equals("Adjust", StringComparison.OrdinalIgnoreCase))
            {
                after = detail.TonSau;
            }

            if (after < 0)
            {
                return BadRequest(new { message = $"Ton kho sau duyet cua {detail.TenSanPham} khong duoc am." });
            }

            if (variant is not null)
            {
                variant.SoLuongTon = after;
                variant.NgayCapNhat = now;
            }
            else
            {
                product.SoLuongTon = after;
                product.NgayCapNhat = now;
            }

            await _db.SaveChangesAsync();
            await InsertAdjustmentLogAsync(product, variant, document.LoaiPhieu, after - before, before, after, document.MaPhieu, userId);
        }

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE dbo.TONKHO_PHIEU
            SET TrangThai = N'Approved', MaNguoiDuyet = {userId}, NgayDuyet = {now}, NgayCapNhat = {now}
            WHERE MaPhieuKho = {id}
            """);
        await _db.Database.ExecuteSqlRawAsync("EXEC sp_SANPHAM_DongBoTatCaSoLuongTon");
        await _auditLog.WriteAsync(this, "InventoryDocument", id.ToString(), "Approve", new { document.TrangThai }, new { TrangThai = "Approved" }, document.MaPhieu);
        await transaction.CommitAsync();

        return Ok(new { message = "Duyet phieu kho thanh cong." });
    }

    [HttpPost("documents/{id:int}/cancel")]
    public async Task<IActionResult> CancelStockDocument(int id, [FromBody] InventoryDocumentCancelRequest request)
    {
        await EnsureSupportTablesAsync();
        var documents = await _db.Database.SqlQueryRaw<InventoryDocumentRow>($"SELECT MaPhieuKho, MaPhieu, LoaiPhieu, TrangThai, GhiChu, MaNguoiTao, MaNguoiDuyet, NgayTao, NgayDuyet, 0 AS SoDong, 0 AS TongSoLuong FROM dbo.TONKHO_PHIEU WHERE MaPhieuKho = {id}").ToListAsync();
        var document = documents.FirstOrDefault();
        if (document is null)
        {
            return NotFound(new { message = "Khong tim thay phieu kho." });
        }

        if (!string.Equals(document.TrangThai, "Draft", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Chi phieu kho dang nhap moi duoc huy." });
        }

        var now = DateTime.UtcNow;
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE dbo.TONKHO_PHIEU
            SET TrangThai = N'Cancelled', GhiChu = CONCAT(ISNULL(GhiChu, N''), N' | Huy: ', {TrimToNull(request.LyDo)}), NgayCapNhat = {now}
            WHERE MaPhieuKho = {id}
            """);
        await _auditLog.WriteAsync(this, "InventoryDocument", id.ToString(), "Cancel", new { document.TrangThai }, new { TrangThai = "Cancelled" }, request.LyDo);
        return Ok(new { message = "Huy phieu kho thanh cong." });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        await EnsureSupportTablesAsync();
        await _db.Database.ExecuteSqlRawAsync("EXEC sp_SANPHAM_DongBoTatCaSoLuongTon");
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            MERGE dbo.TONKHO_META AS target
            USING (SELECT N'LastSyncAt' AS [Key]) AS source
            ON target.[Key] = source.[Key]
            WHEN MATCHED THEN UPDATE SET [Value] = {DateTime.UtcNow.ToString("O")}
            WHEN NOT MATCHED THEN INSERT ([Key], [Value]) VALUES (N'LastSyncAt', {DateTime.UtcNow.ToString("O")});
            """);
        await _auditLog.WriteAsync(this, "Inventory", "All", "Sync", null, new { SyncedAt = DateTime.UtcNow });
        return Ok(new { message = "Dong bo ton kho thanh cong." });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] InventorySearchRequest request)
    {
        await EnsureSupportTablesAsync();
        var rows = ApplySort(ApplyFilters(await LoadInventoryRowsAsync(), request), request.SortBy, request.SortDirection).ToList();
        var csv = new StringBuilder();
        csv.AppendLine("Mã sản phẩm\tMã SP\tMã biến thể\tSKU\tTên sản phẩm\tTên biến thể\tTồn thực tế\tĐang giữ chỗ\tTồn khả dụng\tNgưỡng tồn thấp\tTrạng thái tồn\tNgày cập nhật");
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join("\t", new[]
            {
                row.MaSanPham.ToString(),
                EscapeTsv(row.MaSanPhamKinhDoanh),
                row.MaBienSanPham?.ToString() ?? "",
                EscapeTsv(row.SKU),
                EscapeTsv(FixMojibake(row.TenSanPham)),
                EscapeTsv(FixMojibake(row.TenBienThe)),
                row.TonKhoThucTe.ToString(),
                row.SoLuongDangGiu.ToString(),
                row.TonKhoKhaDung.ToString(),
                row.MucCanhBaoTonThap.ToString(),
                EscapeTsv(LocalizeStockStatus(row.TrangThaiTon)),
                EscapeTsv(row.NgayCapNhat.ToString("dd/MM/yyyy HH:mm:ss"))
            }));
        }

        var unicode = Encoding.Unicode;
        return File(unicode.GetPreamble().Concat(unicode.GetBytes(csv.ToString())).ToArray(), "text/tab-separated-values; charset=utf-16", $"inventory-{DateTime.UtcNow:yyyyMMddHHmmss}.xls");
    }

    private async Task<List<InventoryRow>> LoadInventoryRowsAsync()
    {
        var rows = await _db.Database.SqlQueryRaw<InventoryRow>(
            """
            SELECT
                tk.MaSanPham,
                sp.MaSanPhamKinhDoanh,
                tk.MaBienSanPham,
                bt.SKU,
                tk.TenSanPham,
                tk.TenBienThe,
                tk.TonKhoThucTe,
                tk.SoLuongDangGiu,
                tk.TonKhoKhaDung,
                ISNULL(cb.MucCanhBaoTonThap, 5) AS MucCanhBaoTonThap,
                CASE
                    WHEN tk.TonKhoKhaDung <= 0 THEN 'OutOfStock'
                    WHEN tk.TonKhoKhaDung <= ISNULL(cb.MucCanhBaoTonThap, 5) THEN 'LowStock'
                    ELSE 'InStock'
                END AS TrangThaiTon,
                CASE
                    WHEN tk.MaBienSanPham IS NULL THEN sp.NgayCapNhat
                    ELSE bt.NgayCapNhat
                END AS NgayCapNhat
            FROM dbo.v_TONKHO_KHADUNG tk
            INNER JOIN dbo.SANPHAM sp ON sp.MaSanPham = tk.MaSanPham
            LEFT JOIN dbo.BIENSANPHAM bt ON bt.MaBienSanPham = tk.MaBienSanPham
            LEFT JOIN dbo.TONKHO_NGUONG_CANHBAO cb
                ON cb.MaSanPham = tk.MaSanPham
               AND ISNULL(cb.MaBienSanPham, -1) = ISNULL(tk.MaBienSanPham, -1)
            """
        ).ToListAsync();

        return rows;
    }

    private static IEnumerable<InventoryRow> ApplyFilters(IEnumerable<InventoryRow> rows, InventorySearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            rows = rows.Where(x =>
                Contains(x.TenSanPham, search) ||
                Contains(x.TenBienThe, search) ||
                Contains(x.MaSanPhamKinhDoanh, search) ||
                Contains(x.SKU, search));
        }

        if (!string.IsNullOrWhiteSpace(request.StockStatus))
        {
            rows = rows.Where(x => string.Equals(x.TrangThaiTon, request.StockStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (request.HasHold == true)
        {
            rows = rows.Where(x => x.SoLuongDangGiu > 0);
        }

        if (request.LowStockOnly == true)
        {
            rows = rows.Where(x => x.TrangThaiTon is "LowStock" or "OutOfStock");
        }

        return rows;
    }

    private static IEnumerable<InventoryRow> ApplySort(IEnumerable<InventoryRow> rows, string? sortBy, string? direction)
    {
        var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "actualstock" or "tonkhothucte" => desc ? rows.OrderByDescending(x => x.TonKhoThucTe) : rows.OrderBy(x => x.TonKhoThucTe),
            "reserved" or "soluongdanggiu" => desc ? rows.OrderByDescending(x => x.SoLuongDangGiu) : rows.OrderBy(x => x.SoLuongDangGiu),
            "available" or "tonkhokhadung" => desc ? rows.OrderByDescending(x => x.TonKhoKhaDung) : rows.OrderBy(x => x.TonKhoKhaDung),
            "updated" or "ngaycapnhat" => desc ? rows.OrderByDescending(x => x.NgayCapNhat) : rows.OrderBy(x => x.NgayCapNhat),
            _ => desc ? rows.OrderByDescending(x => x.TenSanPham).ThenByDescending(x => x.TenBienThe) : rows.OrderBy(x => x.TenSanPham).ThenBy(x => x.TenBienThe)
        };
    }

    private static object BuildSummary(IEnumerable<InventoryRow> rows)
    {
        var list = rows.ToList();
        return new
        {
            totalSkus = list.Count,
            outOfStock = list.Count(x => x.TrangThaiTon == "OutOfStock"),
            lowStock = list.Count(x => x.TrangThaiTon == "LowStock"),
            holding = list.Count(x => x.SoLuongDangGiu > 0),
            totalActualStock = list.Sum(x => x.TonKhoThucTe),
            totalReserved = list.Sum(x => x.SoLuongDangGiu),
            totalAvailable = list.Sum(x => x.TonKhoKhaDung)
        };
    }

    private async Task<DateTime?> GetLastSyncAsync()
    {
        var values = await _db.Database.SqlQueryRaw<InventoryMetaRow>(
            "SELECT [Key], [Value] FROM dbo.TONKHO_META WHERE [Key] = 'LastSyncAt'"
        ).ToListAsync();

        return DateTime.TryParse(values.FirstOrDefault()?.Value, out var value) ? value : null;
    }

    private async Task InsertAdjustmentLogAsync(Product product, ProductVariant? variant, string type, int delta, int before, int after, string reason, int? userId)
    {
        int? variantId = variant?.MaBienSanPham;
        string? sku = variant?.SKU;
        string? variantName = variant?.TenBienThe;
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO dbo.TONKHO_DIEUCHINH_LOG
                (MaSanPham, MaBienSanPham, MaSanPhamKinhDoanh, SKU, TenSanPham, TenBienThe, LoaiGiaoDich, SoLuongThayDoi, TonTruoc, TonSau, LyDo, MaNguoiDung, NgayTao)
            VALUES
                ({product.MaSanPham}, {variantId}, {product.MaSanPhamKinhDoanh}, {sku}, {product.TenSanPham}, {variantName}, {type}, {delta}, {before}, {after}, {reason.Trim()}, {userId}, SYSDATETIME())
            """);
    }

    private async Task EnsureSupportTablesAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.TONKHO_NGUONG_CANHBAO', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_NGUONG_CANHBAO (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaSanPham INT NOT NULL,
                    MaBienSanPham INT NULL,
                    MucCanhBaoTonThap INT NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
                CREATE UNIQUE INDEX UX_TONKHO_NGUONG_CANHBAO_Target
                    ON dbo.TONKHO_NGUONG_CANHBAO (MaSanPham, MaBienSanPham)
                    WHERE MaBienSanPham IS NOT NULL;
                CREATE UNIQUE INDEX UX_TONKHO_NGUONG_CANHBAO_Product
                    ON dbo.TONKHO_NGUONG_CANHBAO (MaSanPham)
                    WHERE MaBienSanPham IS NULL;
            END;

            IF OBJECT_ID(N'dbo.TONKHO_DIEUCHINH_LOG', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_DIEUCHINH_LOG (
                    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaSanPham INT NOT NULL,
                    MaBienSanPham INT NULL,
                    MaSanPhamKinhDoanh NVARCHAR(50) NOT NULL,
                    SKU NVARCHAR(80) NULL,
                    TenSanPham NVARCHAR(255) NOT NULL,
                    TenBienThe NVARCHAR(180) NULL,
                    LoaiGiaoDich VARCHAR(20) NOT NULL,
                    SoLuongThayDoi INT NOT NULL,
                    TonTruoc INT NOT NULL,
                    TonSau INT NOT NULL,
                    LyDo NVARCHAR(500) NOT NULL,
                    MaNguoiDung INT NULL,
                    NgayTao DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_TONKHO_DIEUCHINH_LOG_Target
                    ON dbo.TONKHO_DIEUCHINH_LOG (MaSanPham, MaBienSanPham, NgayTao DESC);
            END;

            IF OBJECT_ID(N'dbo.TONKHO_META', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_META (
                    [Key] NVARCHAR(100) NOT NULL PRIMARY KEY,
                    [Value] NVARCHAR(500) NULL
                );
            END;

            IF OBJECT_ID(N'dbo.TONKHO_PHIEU', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_PHIEU (
                    MaPhieuKho INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaPhieu NVARCHAR(40) NOT NULL UNIQUE,
                    LoaiPhieu VARCHAR(20) NOT NULL,
                    TrangThai VARCHAR(20) NOT NULL,
                    GhiChu NVARCHAR(1000) NULL,
                    MaNguoiTao INT NULL,
                    MaNguoiDuyet INT NULL,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayDuyet DATETIME2(0) NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
                CREATE INDEX IX_TONKHO_PHIEU_Status_Time
                    ON dbo.TONKHO_PHIEU (TrangThai, NgayTao DESC);
            END;

            IF OBJECT_ID(N'dbo.TONKHO_PHIEU_CHITIET', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TONKHO_PHIEU_CHITIET (
                    MaChiTietPhieuKho INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaPhieuKho INT NOT NULL,
                    MaSanPham INT NOT NULL,
                    MaBienSanPham INT NULL,
                    MaSanPhamKinhDoanh NVARCHAR(50) NOT NULL,
                    SKU NVARCHAR(80) NULL,
                    TenSanPham NVARCHAR(255) NOT NULL,
                    TenBienThe NVARCHAR(180) NULL,
                    TonTruoc INT NOT NULL,
                    SoLuongThayDoi INT NOT NULL,
                    TonSau INT NOT NULL,
                    GhiChu NVARCHAR(500) NULL,
                    CONSTRAINT FK_TONKHO_PHIEU_CHITIET_PHIEU
                        FOREIGN KEY (MaPhieuKho) REFERENCES dbo.TONKHO_PHIEU (MaPhieuKho)
                );
                CREATE INDEX IX_TONKHO_PHIEU_CHITIET_Phieu
                    ON dbo.TONKHO_PHIEU_CHITIET (MaPhieuKho);
            END;
            """);
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }

    private static int CalculateNewStock(int current, int quantity, string type) => type switch
    {
        "Import" => current + quantity,
        "Export" => current - quantity,
        "Adjust" => quantity,
        _ => current
    };

    private static string? NormalizeAdjustmentType(string? type)
    {
        return type?.Trim().ToLowerInvariant() switch
        {
            "import" or "nhapkho" => "Import",
            "export" or "xuatkho" => "Export",
            "adjust" or "dieuchinh" => "Adjust",
            _ => null
        };
    }

    private static bool Contains(string? value, string search)
    {
        return value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string EscapeTsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);
    }

    private static string LocalizeStockStatus(string? status)
    {
        return status switch
        {
            "OutOfStock" => "Hết hàng",
            "LowStock" => "Sắp hết",
            "InStock" => "Còn hàng",
            _ => status ?? ""
        };
    }

    private static string? FixMojibake(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !LooksLikeMojibake(value))
        {
            return value;
        }

        try
        {
            var bytes = value.Select(ch => ch <= byte.MaxValue ? (byte)ch : (byte)'?').ToArray();
            var decoded = Encoding.UTF8.GetString(bytes);
            if (decoded.Contains('\uFFFD') ||
                decoded.Count(ch => ch == '?') > value.Count(ch => ch == '?') ||
                CountMojibakeMarkers(decoded) >= CountMojibakeMarkers(value))
            {
                return value;
            }

            return decoded;
        }
        catch
        {
            return value;
        }
    }

    private static bool LooksLikeMojibake(string value)
    {
        return value.Contains("áº", StringComparison.Ordinal) ||
               value.Contains("á»", StringComparison.Ordinal) ||
               value.Contains("Ã", StringComparison.Ordinal) ||
               value.Contains("Â", StringComparison.Ordinal) ||
               value.Contains("Ä", StringComparison.Ordinal) ||
               value.Contains("Æ", StringComparison.Ordinal) ||
               value.Contains("â€", StringComparison.Ordinal);
    }

    private static int CountMojibakeMarkers(string value)
    {
        var markers = new[] { "áº", "á»", "Ã", "Â", "Ä", "Æ", "â€" };
        return markers.Sum(marker => value.Split(marker, StringSplitOptions.None).Length - 1);
    }
}

public class InventorySearchRequest
{
    public string? Search { get; set; }
    public string? StockStatus { get; set; }
    public bool? HasHold { get; set; }
    public bool? LowStockOnly { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class InventoryThresholdRequest
{
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public int MucCanhBaoTonThap { get; set; }
}

public class InventoryAdjustRequest
{
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public string LyDo { get; set; } = string.Empty;
}

public class InventoryDocumentRequest
{
    public string LoaiPhieu { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public List<InventoryDocumentItemRequest> Items { get; set; } = new();
}

public class InventoryDocumentItemRequest
{
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public int SoLuong { get; set; }
    public string? GhiChu { get; set; }
}

public class InventoryDocumentCancelRequest
{
    public string? LyDo { get; set; }
}

public class InventoryRow
{
    public int MaSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = "";
    public int? MaBienSanPham { get; set; }
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? TenBienThe { get; set; }
    public int TonKhoThucTe { get; set; }
    public int SoLuongDangGiu { get; set; }
    public int TonKhoKhaDung { get; set; }
    public int MucCanhBaoTonThap { get; set; } = 5;
    public string TrangThaiTon { get; set; } = "InStock";
    public DateTime NgayCapNhat { get; set; }
}

public class InventoryHoldRow
{
    public int MaGiuCho { get; set; }
    public int MaDonHang { get; set; }
    public string? MaDonHangKinhDoanh { get; set; }
    public int MaSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = "";
    public int? MaBienSanPham { get; set; }
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? TenBienThe { get; set; }
    public int SoLuong { get; set; }
    public string TrangThai { get; set; } = "";
    public DateTime HetHanLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public string? GhiChu { get; set; }
}

public class InventoryAdjustmentRow
{
    public int Id { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = "";
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? TenBienThe { get; set; }
    public string LoaiGiaoDich { get; set; } = "";
    public int SoLuongThayDoi { get; set; }
    public int TonTruoc { get; set; }
    public int TonSau { get; set; }
    public string LyDo { get; set; } = "";
    public int? MaNguoiDung { get; set; }
    public DateTime NgayTao { get; set; }
}

public class InventoryMetaRow
{
    public string Key { get; set; } = "";
    public string? Value { get; set; }
}

public class InventoryDocumentIdRow
{
    public int MaPhieuKho { get; set; }
}

public class InventoryDocumentRow
{
    public int MaPhieuKho { get; set; }
    public string MaPhieu { get; set; } = "";
    public string LoaiPhieu { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public string? GhiChu { get; set; }
    public int? MaNguoiTao { get; set; }
    public int? MaNguoiDuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public int SoDong { get; set; }
    public int TongSoLuong { get; set; }
}

public class InventoryDocumentDetailRow
{
    public int MaChiTietPhieuKho { get; set; }
    public int MaPhieuKho { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = "";
    public string? SKU { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? TenBienThe { get; set; }
    public int TonTruoc { get; set; }
    public int SoLuongThayDoi { get; set; }
    public int TonSau { get; set; }
    public string? GhiChu { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/advanced-operations")]
public class AdvancedOperationsController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IAuditLogService _audit;

    public AdvancedOperationsController(OrderDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    private static string GenerateCode(string prefix) => $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    private static string? TrimToNull(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    // ===== Sales returns =====

    [HttpGet("returns")]
    public async Task<IActionResult> GetReturns([FromQuery] string? status)
    {
        var orderCodes = await _db.Orders.AsNoTracking().ToDictionaryAsync(x => x.MaDonHang, x => x.MaDonHangKinhDoanh);
        var variantSku = await _db.ProductVariants.AsNoTracking().ToDictionaryAsync(x => x.MaBienSanPham, x => x.SKU);

        var query = _db.PhieuTraHangs.AsNoTracking().Include(x => x.ChiTiet).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.TrangThai == status);
        var rows = await query.OrderByDescending(x => x.MaPhieuTra).ToListAsync();

        var items = rows.Select(r => new
        {
            id = r.MaPhieuTra,
            code = r.MaPhieuTraKinhDoanh,
            orderId = r.MaDonHang,
            orderCode = orderCodes.GetValueOrDefault(r.MaDonHang),
            returnStatus = r.TrangThai,
            reason = r.LyDo,
            note = r.GhiChu,
            refundAmount = r.SoTienHoan,
            maxRefundAmount = r.ChiTiet.Sum(l => l.ThanhTien),
            createdDate = r.NgayTao,
            approvedAt = r.NgayDuyet,
            lines = r.ChiTiet.Select(l => new
            {
                id = l.MaChiTietTra,
                orderLineId = l.MaChiTietDonHang,
                skuId = l.MaBienSanPham,
                skuCode = variantSku.GetValueOrDefault(l.MaBienSanPham),
                productName = (string?)null,
                qty = l.SoLuong,
                unitPrice = l.DonGia,
                lineTotal = l.ThanhTien,
                itemCondition = l.TinhTrangHang
            })
        });
        return Ok(new { items });
    }

    [HttpGet("returns/{id:int}")]
    public async Task<IActionResult> GetReturn(int id)
    {
        var r = await _db.PhieuTraHangs.AsNoTracking().Include(x => x.ChiTiet).FirstOrDefaultAsync(x => x.MaPhieuTra == id);
        if (r is null) return NotFound();
        var orderCode = await _db.Orders.Where(o => o.MaDonHang == r.MaDonHang).Select(o => o.MaDonHangKinhDoanh).FirstOrDefaultAsync();
        return Ok(new
        {
            id = r.MaPhieuTra,
            code = r.MaPhieuTraKinhDoanh,
            orderId = r.MaDonHang,
            orderCode,
            returnStatus = r.TrangThai,
            reason = r.LyDo,
            refundAmount = r.SoTienHoan,
            maxRefundAmount = r.ChiTiet.Sum(l => l.ThanhTien),
            lines = r.ChiTiet.Select(l => new { id = l.MaChiTietTra, skuId = l.MaBienSanPham, qty = l.SoLuong, lineTotal = l.ThanhTien, itemCondition = l.TinhTrangHang })
        });
    }

    [HttpPost("returns")]
    public async Task<IActionResult> CreateReturn([FromBody] CreateReturnRequest req)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.MaDonHang == req.OrderId);
        if (order is null) return BadRequest(new { message = "Don hang khong ton tai." });
        if (req.Lines is null || req.Lines.Count == 0) return BadRequest(new { message = "Vui long chon san pham tra." });

        var orderLines = await _db.OrderItems.Where(i => i.MaDonHang == req.OrderId).ToListAsync();
        var lines = new List<ChiTietTraHang>();
        foreach (var l in req.Lines)
        {
            var ol = orderLines.FirstOrDefault(x => x.MaChiTietDonHang == l.OrderLineId);
            if (ol is null || l.Qty <= 0) continue;
            if (l.Qty > ol.SoLuong) return BadRequest(new { message = "So luong tra vuot qua so luong da mua." });
            lines.Add(new ChiTietTraHang
            {
                MaChiTietDonHang = ol.MaChiTietDonHang,
                MaBienSanPham = ol.MaBienSanPham ?? 0,
                SoLuong = l.Qty,
                DonGia = ol.DonGia,
                ThanhTien = ol.DonGia * l.Qty,
                TinhTrangHang = string.IsNullOrWhiteSpace(l.ItemCondition) ? "Resellable" : l.ItemCondition!,
                NgayTao = DateTime.UtcNow
            });
        }
        if (lines.Count == 0) return BadRequest(new { message = "Khong co dong tra hop le." });

        var entity = new PhieuTraHang
        {
            MaPhieuTraKinhDoanh = GenerateCode("RT"),
            MaDonHang = req.OrderId,
            TrangThai = "Draft",
            LyDo = TrimToNull(req.Reason) ?? "",
            GhiChu = TrimToNull(req.Note),
            SoTienHoan = 0,
            MaNguoiTao = this.GetCurrentUserId(),
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow,
            ChiTiet = lines
        };
        _db.PhieuTraHangs.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuTraHang", entity.MaPhieuTra.ToString(), "Create", null, new { entity.MaPhieuTraKinhDoanh });
        return Ok(new { id = entity.MaPhieuTra });
    }

    [HttpPost("returns/{id:int}/approve")]
    public async Task<IActionResult> ApproveReturn(int id, [FromBody] ApproveReturnRequest req)
    {
        var entity = await _db.PhieuTraHangs.Include(x => x.ChiTiet).FirstOrDefaultAsync(x => x.MaPhieuTra == id);
        if (entity is null) return NotFound();
        if (entity.TrangThai != "Draft") return BadRequest(new { message = "Chi duyet duoc phieu cho duyet." });

        var maxRefund = entity.ChiTiet.Sum(l => l.ThanhTien);
        var refundAmount = Math.Max(0, Math.Min(req.RefundAmount, maxRefund));

        // Nhap lai ton kho cho hang con ban duoc
        foreach (var line in entity.ChiTiet.Where(l => l.TinhTrangHang == "Resellable" && l.MaBienSanPham > 0))
            await IncreaseStockAsync(line.MaBienSanPham, line.SoLuong);

        entity.TrangThai = "Approved";
        entity.SoTienHoan = refundAmount;
        entity.MaNguoiDuyet = this.GetCurrentUserId();
        entity.NgayDuyet = DateTime.UtcNow;
        entity.NgayCapNhat = DateTime.UtcNow;

        if (refundAmount > 0)
        {
            _db.PhieuHoanTiens.Add(new PhieuHoanTien
            {
                MaHoanTienKinhDoanh = GenerateCode("RF"),
                MaDonHang = entity.MaDonHang,
                MaPhieuTra = entity.MaPhieuTra,
                SoTien = refundAmount,
                PhuongThuc = string.IsNullOrWhiteSpace(req.RefundMethod) ? "Cash" : req.RefundMethod!,
                TrangThai = "Paid",
                LyDo = TrimToNull(req.Note) ?? "Hoan tien tra hang",
                MaGiaoDich = TrimToNull(req.TransactionRef),
                MaNguoiGhi = this.GetCurrentUserId(),
                NgayHoan = DateTime.UtcNow,
                NgayTao = DateTime.UtcNow
            });
            _db.GiaoDichTienMats.Add(new GiaoDichTienMat
            {
                MaGiaoDichKinhDoanh = GenerateCode("CT"),
                LoaiGiaoDich = "Payment",
                DanhMuc = "Refund",
                SoTien = refundAmount,
                PhuongThuc = string.IsNullOrWhiteSpace(req.RefundMethod) ? "Cash" : req.RefundMethod!,
                LoaiThamChieu = "SalesReturn",
                MaThamChieu = entity.MaPhieuTra,
                GhiChu = $"Hoan tien phieu tra {entity.MaPhieuTraKinhDoanh}",
                MaNguoiGhi = this.GetCurrentUserId(),
                NgayGiaoDich = DateTime.UtcNow,
                NgayTao = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuTraHang", id.ToString(), "Approve", null, new { refundAmount });
        return Ok(new { id });
    }

    [HttpPost("returns/{id:int}/reject")]
    public async Task<IActionResult> RejectReturn(int id, [FromBody] RejectReturnRequest req)
    {
        var entity = await _db.PhieuTraHangs.FirstOrDefaultAsync(x => x.MaPhieuTra == id);
        if (entity is null) return NotFound();
        if (entity.TrangThai != "Draft") return BadRequest(new { message = "Chi tu choi duoc phieu cho duyet." });
        entity.TrangThai = "Rejected";
        entity.GhiChu = TrimToNull(req.Note) ?? entity.GhiChu;
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuTraHang", id.ToString(), "Reject");
        return Ok(new { id });
    }

    // ===== Refunds =====

    [HttpGet("refunds")]
    public async Task<IActionResult> GetRefunds([FromQuery] int? orderId)
    {
        var orderCodes = await _db.Orders.AsNoTracking().ToDictionaryAsync(x => x.MaDonHang, x => x.MaDonHangKinhDoanh);
        var query = _db.PhieuHoanTiens.AsNoTracking().AsQueryable();
        if (orderId.HasValue) query = query.Where(x => x.MaDonHang == orderId.Value);
        var rows = await query.OrderByDescending(x => x.MaHoanTien).ToListAsync();
        var items = rows.Select(x => new
        {
            id = x.MaHoanTien,
            code = x.MaHoanTienKinhDoanh,
            orderId = x.MaDonHang,
            orderCode = orderCodes.GetValueOrDefault(x.MaDonHang),
            salesReturnId = x.MaPhieuTra,
            amount = x.SoTien,
            method = x.PhuongThuc,
            refundStatus = x.TrangThai,
            reason = x.LyDo,
            transactionRef = x.MaGiaoDich,
            refundedAt = x.NgayHoan
        });
        return Ok(new { items });
    }

    // ===== Receivables (cong no khach hang) =====

    [HttpGet("receivables")]
    public async Task<IActionResult> GetReceivables()
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.TrangThaiDonHang != "Cancelled")
            .Select(o => new { o.MaDonHang, o.MaDonHangKinhDoanh, o.HoTenNhanHang, o.TongThanhToan, o.TienDatCoc, o.TrangThaiThanhToan })
            .ToListAsync();

        var paidByOrder = await _db.Payments.AsNoTracking().Where(p => p.TrangThai != "Cancelled")
            .GroupBy(p => p.MaDonHang).Select(g => new { OrderId = g.Key, Total = g.Sum(x => x.SoTien) }).ToDictionaryAsync(x => x.OrderId, x => x.Total);
        var refundByOrder = await _db.PhieuHoanTiens.AsNoTracking()
            .GroupBy(p => p.MaDonHang).Select(g => new { OrderId = g.Key, Total = g.Sum(x => x.SoTien) }).ToDictionaryAsync(x => x.OrderId, x => x.Total);
        var returnByOrder = await _db.PhieuTraHangs.AsNoTracking().Where(r => r.TrangThai == "Approved")
            .GroupBy(r => r.MaDonHang).Select(g => new { OrderId = g.Key, Total = g.Sum(x => x.SoTienHoan) }).ToDictionaryAsync(x => x.OrderId, x => x.Total);

        var items = orders.Select(o =>
        {
            var totalPaid = paidByOrder.GetValueOrDefault(o.MaDonHang, 0m);
            var totalRefunded = refundByOrder.GetValueOrDefault(o.MaDonHang, 0m);
            var returned = returnByOrder.GetValueOrDefault(o.MaDonHang, 0m);
            var adjustedTotal = Math.Max(0, o.TongThanhToan - returned);
            var netPaid = totalPaid - totalRefunded;
            var outstanding = Math.Max(0, adjustedTotal - netPaid);
            return new
            {
                orderId = o.MaDonHang,
                orderCode = o.MaDonHangKinhDoanh,
                customerName = o.HoTenNhanHang,
                grandTotal = o.TongThanhToan,
                adjustedTotal,
                depositRequired = o.TienDatCoc,
                totalPaid,
                totalRefunded,
                netPaid,
                outstanding,
                paymentStatus = o.TrangThaiThanhToan
            };
        }).ToList();

        return Ok(new { items });
    }

    // ===== Staff shifts (CALAMVIEC) =====

    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? staffUserId)
    {
        var names = await _db.Users.AsNoTracking().ToDictionaryAsync(x => x.MaNguoiDung, x => x.HoTen);
        var query = _db.CaLamViecs.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(x => x.BatDau >= from.Value);
        if (to.HasValue) query = query.Where(x => x.BatDau <= to.Value);
        if (staffUserId.HasValue) query = query.Where(x => x.MaNhanVien == staffUserId.Value);
        var rows = await query.OrderByDescending(x => x.BatDau).Take(500).ToListAsync();
        var items = rows.Select(x => new
        {
            id = x.MaCa,
            staffUserId = x.MaNhanVien,
            staffName = names.GetValueOrDefault(x.MaNhanVien),
            startsAt = x.BatDau,
            endsAt = x.KetThuc,
            shiftStatus = x.TrangThai,
            note = x.GhiChu
        });
        return Ok(new { items });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("shifts")]
    public async Task<IActionResult> CreateShift([FromBody] ShiftRequest req)
    {
        if (req.StartsAt >= req.EndsAt) return BadRequest(new { message = "Thoi gian bat dau phai truoc ket thuc." });
        if (await HasOverlapAsync(req.StaffUserId, req.StartsAt, req.EndsAt, null))
            return BadRequest(new { message = "Ca lam viec bi trung lich voi ca khac." });
        var entity = new CaLamViec
        {
            MaNhanVien = req.StaffUserId,
            BatDau = req.StartsAt,
            KetThuc = req.EndsAt,
            TrangThai = "Scheduled",
            GhiChu = TrimToNull(req.Note),
            MaNguoiPhanCong = this.GetCurrentUserId(),
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        _db.CaLamViecs.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "CaLamViec", entity.MaCa.ToString(), "Create");
        return Ok(new { id = entity.MaCa });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("shifts/{id:int}")]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] ShiftRequest req)
    {
        var entity = await _db.CaLamViecs.FirstOrDefaultAsync(x => x.MaCa == id);
        if (entity is null) return NotFound();
        if (req.StartsAt >= req.EndsAt) return BadRequest(new { message = "Thoi gian bat dau phai truoc ket thuc." });
        if (await HasOverlapAsync(entity.MaNhanVien, req.StartsAt, req.EndsAt, id))
            return BadRequest(new { message = "Ca lam viec bi trung lich voi ca khac." });
        entity.BatDau = req.StartsAt;
        entity.KetThuc = req.EndsAt;
        entity.TrangThai = req.ShiftStatus is "Completed" or "Cancelled" ? req.ShiftStatus : "Scheduled";
        entity.GhiChu = TrimToNull(req.Note);
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "CaLamViec", id.ToString(), "Update");
        return Ok(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("shifts/{id:int}")]
    public async Task<IActionResult> DeleteShift(int id)
    {
        var entity = await _db.CaLamViecs.FirstOrDefaultAsync(x => x.MaCa == id);
        if (entity is null) return NotFound();
        entity.TrangThai = "Cancelled";
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "CaLamViec", id.ToString(), "Cancel");
        return Ok(new { id });
    }

    // ===== Helpers =====

    private async Task<bool> HasOverlapAsync(int staffId, DateTime start, DateTime end, int? excludeId)
    {
        return await _db.CaLamViecs.AnyAsync(x => x.MaNhanVien == staffId && x.TrangThai != "Cancelled"
            && (excludeId == null || x.MaCa != excludeId)
            && start < x.KetThuc && end > x.BatDau);
    }

    private async Task IncreaseStockAsync(int maBienSanPham, int delta)
    {
        var variant = await _db.ProductVariants.AsNoTracking().FirstAsync(x => x.MaBienSanPham == maBienSanPham);
        int? userId = null;
        int? refId = null;
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            EXEC dbo.sp_TONKHO_ApDungBienDong
                @MaSanPham = {variant.MaSanPham},
                @MaBienSanPham = {maBienSanPham},
                @LoaiBienDong = {"NghiepVu"},
                @SoLuongThayDoi = {delta},
                @LyDo = {"Dieu chinh ton kho tu nghiep vu mo rong"},
                @LoaiThamChieu = {"AdvancedOperations"},
                @MaThamChieu = {refId},
                @MaNguoiThucHien = {userId}
            """);
    }
}

// ===== Request DTOs =====
public class CreateReturnRequest
{
    public int OrderId { get; set; }
    public string? Reason { get; set; }
    public string? Note { get; set; }
    public List<ReturnLineRequest> Lines { get; set; } = new();
}
public class ReturnLineRequest { public int OrderLineId { get; set; } public int Qty { get; set; } public string? ItemCondition { get; set; } }
public class ApproveReturnRequest { public decimal RefundAmount { get; set; } public string? RefundMethod { get; set; } public string? TransactionRef { get; set; } public string? Note { get; set; } }
public class RejectReturnRequest { public string? Note { get; set; } }
public class ShiftRequest { public int StaffUserId { get; set; } public DateTime StartsAt { get; set; } public DateTime EndsAt { get; set; } public string? ShiftStatus { get; set; } public string? Note { get; set; } }

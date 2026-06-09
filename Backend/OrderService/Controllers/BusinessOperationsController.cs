using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/business-operations")]
public class BusinessOperationsController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IAuditLogService _audit;

    public BusinessOperationsController(OrderDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    private bool IsAdmin => User.IsInRole("Admin");
    private static string GenerateCode(string prefix) => $"{prefix}{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    // ===== Lookups & summary =====

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups()
    {
        var stores = await _db.CuaHangs.AsNoTracking().OrderBy(x => x.MaCuaHang)
            .Select(x => new { id = x.MaCuaHang, code = x.MaCuaHangKinhDoanh, name = x.TenCuaHang }).ToListAsync();
        var skus = await _db.ProductVariants.AsNoTracking()
            .Join(_db.Products.AsNoTracking(), v => v.MaSanPham, p => p.MaSanPham, (v, p) => new { id = v.MaBienSanPham, maSanPham = v.MaSanPham, skuCode = v.SKU, productName = p.TenSanPham })
            .OrderBy(x => x.productName).Take(1000).ToListAsync();
        var suppliers = await _db.NhaCungCaps.AsNoTracking().Where(x => x.TrangThai == 1).OrderBy(x => x.TenNhaCungCap)
            .Select(x => new { id = x.MaNhaCungCap, code = x.MaNhaCungCapKinhDoanh, name = x.TenNhaCungCap }).ToListAsync();
        var users = await _db.Users.AsNoTracking().OrderBy(x => x.HoTen)
            .Select(x => new { id = x.MaNguoiDung, fullName = x.HoTen, email = x.Email }).Take(1000).ToListAsync();

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.TrangThaiDonHang == "Confirmed")
            .OrderByDescending(o => o.MaDonHang).Take(200)
            .Select(o => new
            {
                id = o.MaDonHang,
                code = o.MaDonHangKinhDoanh,
                grandTotal = o.TongThanhToan,
                lines = _db.OrderItems.Where(i => i.MaDonHang == o.MaDonHang)
                    .Select(i => new { orderLineId = i.MaChiTietDonHang, productNameSnapshot = i.TenSanPhamSnapshot, skuCodeSnapshot = i.SKUSnapshot, qty = i.SoLuong })
                    .ToList()
            }).ToListAsync();

        return Ok(new { stores, skus, suppliers, customers = users, staff = users, orders });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetOperationsSummary()
    {
        var suppliers = await _db.NhaCungCaps.CountAsync(x => x.TrangThai == 1);
        var pendingPurchases = await _db.DonNhapHangs.CountAsync(x => x.TrangThai == "Draft" || x.TrangThai == "Approved" || x.TrangThai == "PartiallyReceived");
        var purchaseValue = IsAdmin ? await _db.DonNhapHangs.Where(x => x.TrangThai != "Cancelled").SumAsync(x => (decimal?)x.TongTien) ?? 0 : 0;
        var cashIn = IsAdmin ? await _db.GiaoDichTienMats.Where(x => x.LoaiGiaoDich == "Receipt").SumAsync(x => (decimal?)x.SoTien) ?? 0 : 0;
        var cashOut = IsAdmin ? await _db.GiaoDichTienMats.Where(x => x.LoaiGiaoDich == "Payment").SumAsync(x => (decimal?)x.SoTien) ?? 0 : 0;
        var openRepairs = await _db.PhieuSuaChuas.CountAsync(x => x.TrangThai != "Completed" && x.TrangThai != "Delivered" && x.TrangThai != "Cancelled");
        var openInteractions = await _db.TuongTacKhachHangs.CountAsync(x => x.TrangThai == "Open");

        return Ok(new { suppliers, pendingPurchases, purchaseValue, cashIn, cashOut, openRepairs, openInteractions });
    }

    // ===== Suppliers =====

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var items = await _db.NhaCungCaps.AsNoTracking().OrderBy(x => x.TenNhaCungCap)
            .Select(x => new
            {
                id = x.MaNhaCungCap,
                code = x.MaNhaCungCapKinhDoanh,
                name = x.TenNhaCungCap,
                taxCode = x.MaSoThue,
                contactName = x.NguoiLienHe,
                phone = x.SoDienThoai,
                email = x.Email,
                address = x.DiaChi,
                note = x.GhiChu,
                status = x.TrangThai
            }).ToListAsync();
        return Ok(new { items });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("suppliers")]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest(new { message = "Ten nha cung cap la bat buoc." });
        var entity = new NhaCungCap
        {
            MaNhaCungCapKinhDoanh = string.IsNullOrWhiteSpace(req.Code) ? GenerateCode("NCC") : req.Code!.Trim(),
            TenNhaCungCap = req.Name!.Trim(),
            MaSoThue = TrimToNull(req.TaxCode),
            NguoiLienHe = TrimToNull(req.ContactName),
            SoDienThoai = TrimToNull(req.Phone),
            Email = TrimToNull(req.Email),
            DiaChi = TrimToNull(req.Address),
            GhiChu = TrimToNull(req.Note),
            TrangThai = req.Status ?? 1,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        _db.NhaCungCaps.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "NhaCungCap", entity.MaNhaCungCap.ToString(), "Create", null, new { entity.MaNhaCungCapKinhDoanh, entity.TenNhaCungCap });
        return Ok(new { id = entity.MaNhaCungCap });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("suppliers/{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierRequest req)
    {
        var entity = await _db.NhaCungCaps.FirstOrDefaultAsync(x => x.MaNhaCungCap == id);
        if (entity is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(req.Code)) entity.MaNhaCungCapKinhDoanh = req.Code!.Trim();
        if (!string.IsNullOrWhiteSpace(req.Name)) entity.TenNhaCungCap = req.Name!.Trim();
        entity.MaSoThue = TrimToNull(req.TaxCode);
        entity.NguoiLienHe = TrimToNull(req.ContactName);
        entity.SoDienThoai = TrimToNull(req.Phone);
        entity.Email = TrimToNull(req.Email);
        entity.DiaChi = TrimToNull(req.Address);
        entity.GhiChu = TrimToNull(req.Note);
        if (req.Status.HasValue) entity.TrangThai = req.Status.Value;
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "NhaCungCap", id.ToString(), "Update", null, new { entity.TenNhaCungCap, entity.TrangThai });
        return Ok(new { id });
    }

    // ===== Purchases =====

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchases()
    {
        var supplierNames = await _db.NhaCungCaps.AsNoTracking().ToDictionaryAsync(x => x.MaNhaCungCap, x => x.TenNhaCungCap);
        var storeNames = await _db.CuaHangs.AsNoTracking().ToDictionaryAsync(x => x.MaCuaHang, x => x.TenCuaHang);
        var skuCodes = await _db.ProductVariants.AsNoTracking()
            .Join(_db.Products.AsNoTracking(), v => v.MaSanPham, p => p.MaSanPham, (v, p) => new { v.MaBienSanPham, v.SKU, p.TenSanPham })
            .ToDictionaryAsync(x => x.MaBienSanPham, x => new { x.SKU, x.TenSanPham });

        var purchases = await _db.DonNhapHangs.AsNoTracking().Include(x => x.ChiTiet)
            .OrderByDescending(x => x.MaDonNhap).ToListAsync();

        var items = purchases.Select(p => new
        {
            id = p.MaDonNhap,
            code = p.MaDonNhapKinhDoanh,
            supplierName = supplierNames.GetValueOrDefault(p.MaNhaCungCap),
            storeId = p.MaCuaHang,
            storeName = storeNames.GetValueOrDefault(p.MaCuaHang),
            purchaseStatus = p.TrangThai,
            totalAmount = p.TongTien,
            paidAmount = p.DaThanhToan,
            outstanding = p.TongTien - p.DaThanhToan,
            note = p.GhiChu,
            createdDate = p.NgayTao,
            lines = p.ChiTiet.Select(l => new
            {
                id = l.MaChiTietNhap,
                skuId = l.MaBienSanPham,
                skuCode = skuCodes.GetValueOrDefault(l.MaBienSanPham)?.SKU,
                productName = skuCodes.GetValueOrDefault(l.MaBienSanPham)?.TenSanPham,
                orderedQty = l.SoLuongDat,
                receivedQty = l.SoLuongNhan,
                unitCost = l.DonGiaNhap
            })
        });
        return Ok(new { items });
    }

    [HttpPost("purchases")]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest req)
    {
        if (req.Lines is null || req.Lines.Count == 0) return BadRequest(new { message = "Vui long them it nhat mot dong SKU." });
        var storeId = req.StoreId > 0 ? req.StoreId : await DefaultStoreIdAsync();
        var order = new DonNhapHang
        {
            MaDonNhapKinhDoanh = GenerateCode("PO"),
            MaNhaCungCap = req.SupplierId,
            MaCuaHang = storeId,
            TrangThai = "Draft",
            TongTien = req.Lines.Sum(l => l.Qty * l.UnitCost),
            DaThanhToan = 0,
            GhiChu = TrimToNull(req.Note),
            MaNguoiTao = this.GetCurrentUserId(),
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow,
            ChiTiet = req.Lines.Select(l => new ChiTietDonNhap
            {
                MaBienSanPham = l.SkuId,
                SoLuongDat = l.Qty,
                SoLuongNhan = 0,
                DonGiaNhap = l.UnitCost,
                NgayTao = DateTime.UtcNow
            }).ToList()
        };
        _db.DonNhapHangs.Add(order);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "DonNhapHang", order.MaDonNhap.ToString(), "Create", null, new { order.MaDonNhapKinhDoanh, order.TongTien });
        return Ok(new { id = order.MaDonNhap });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("purchases/{id:int}/approve")]
    public async Task<IActionResult> ApprovePurchase(int id)
    {
        var order = await _db.DonNhapHangs.FirstOrDefaultAsync(x => x.MaDonNhap == id);
        if (order is null) return NotFound();
        if (order.TrangThai != "Draft") return BadRequest(new { message = "Chi duyet duoc don o trang thai Nhap." });
        order.TrangThai = "Approved";
        order.MaNguoiDuyet = this.GetCurrentUserId();
        order.NgayDuyet = DateTime.UtcNow;
        order.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "DonNhapHang", id.ToString(), "Approve");
        return Ok(new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("purchases/{id:int}/cancel")]
    public async Task<IActionResult> CancelPurchase(int id)
    {
        var order = await _db.DonNhapHangs.FirstOrDefaultAsync(x => x.MaDonNhap == id);
        if (order is null) return NotFound();
        if (order.TrangThai is "Received" or "PartiallyReceived" || order.DaThanhToan > 0)
            return BadRequest(new { message = "Khong the huy don da nhan hang hoac da thanh toan." });
        order.TrangThai = "Cancelled";
        order.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "DonNhapHang", id.ToString(), "Cancel");
        return Ok(new { id });
    }

    [HttpPost("purchases/{id:int}/receive")]
    public async Task<IActionResult> ReceivePurchase(int id, [FromBody] ReceivePurchaseRequest req)
    {
        var order = await _db.DonNhapHangs.Include(x => x.ChiTiet).FirstOrDefaultAsync(x => x.MaDonNhap == id);
        if (order is null) return NotFound();
        if (order.TrangThai is not ("Approved" or "PartiallyReceived")) return BadRequest(new { message = "Chi nhan hang cho don da duyet." });
        if (req.Lines is null || req.Lines.Count == 0) return BadRequest(new { message = "Vui long nhap so luong nhan." });

        var receipt = new PhieuNhapKho
        {
            MaPhieuNhapKinhDoanh = GenerateCode("GR"),
            MaDonNhap = id,
            MaCuaHang = order.MaCuaHang,
            GhiChu = TrimToNull(req.Note),
            MaNguoiNhan = this.GetCurrentUserId(),
            NgayNhan = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow,
            ChiTiet = new List<ChiTietPhieuNhap>()
        };

        foreach (var line in req.Lines)
        {
            var poLine = order.ChiTiet.FirstOrDefault(c => c.MaChiTietNhap == line.PurchaseOrderLineId);
            if (poLine is null || line.Qty <= 0) continue;
            var remaining = poLine.SoLuongDat - poLine.SoLuongNhan;
            var qty = Math.Min(line.Qty, remaining);
            if (qty <= 0) continue;

            poLine.SoLuongNhan += qty;
            receipt.ChiTiet.Add(new ChiTietPhieuNhap { MaChiTietNhap = poLine.MaChiTietNhap, MaBienSanPham = poLine.MaBienSanPham, SoLuong = qty, DonGiaNhap = poLine.DonGiaNhap });
            await IncreaseStockAsync(poLine.MaBienSanPham, qty);
        }

        if (receipt.ChiTiet.Count == 0) return BadRequest(new { message = "Khong co dong nao hop le de nhan." });

        order.TrangThai = order.ChiTiet.All(c => c.SoLuongNhan >= c.SoLuongDat) ? "Received" : "PartiallyReceived";
        order.NgayCapNhat = DateTime.UtcNow;
        _db.PhieuNhapKhos.Add(receipt);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuNhapKho", receipt.MaPhieuNhap.ToString(), "Create", null, new { receipt.MaPhieuNhapKinhDoanh, order.TrangThai });
        return Ok(new { id = receipt.MaPhieuNhap });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("purchases/{id:int}/pay")]
    public async Task<IActionResult> PayPurchase(int id, [FromBody] PayPurchaseRequest req)
    {
        var order = await _db.DonNhapHangs.FirstOrDefaultAsync(x => x.MaDonNhap == id);
        if (order is null) return NotFound();
        if (req.Amount <= 0) return BadRequest(new { message = "So tien thanh toan khong hop le." });

        var cash = new GiaoDichTienMat
        {
            MaGiaoDichKinhDoanh = GenerateCode("CT"),
            LoaiGiaoDich = "Payment",
            DanhMuc = "SupplierPayment",
            SoTien = req.Amount,
            PhuongThuc = string.IsNullOrWhiteSpace(req.Method) ? "Cash" : req.Method!,
            LoaiThamChieu = "PurchaseOrder",
            MaThamChieu = id,
            GhiChu = TrimToNull(req.Note),
            MaNguoiGhi = this.GetCurrentUserId(),
            NgayGiaoDich = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow
        };
        _db.GiaoDichTienMats.Add(cash);
        order.DaThanhToan += req.Amount;
        order.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "DonNhapHang", id.ToString(), "Pay", null, new { req.Amount });
        return Ok(new { id = cash.MaGiaoDich });
    }

    // ===== Cash (SOQUY) =====

    [Authorize(Roles = "Admin")]
    [HttpGet("cash")]
    public async Task<IActionResult> GetCash()
    {
        var items = await _db.GiaoDichTienMats.AsNoTracking().OrderByDescending(x => x.MaGiaoDich)
            .Select(x => new
            {
                id = x.MaGiaoDich,
                code = x.MaGiaoDichKinhDoanh,
                transactionType = x.LoaiGiaoDich,
                category = x.DanhMuc,
                amount = x.SoTien,
                method = x.PhuongThuc,
                referenceType = x.LoaiThamChieu,
                referenceId = x.MaThamChieu,
                occurredAt = x.NgayGiaoDich,
                note = x.GhiChu
            }).ToListAsync();
        return Ok(new { items });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("cash")]
    public async Task<IActionResult> CreateCash([FromBody] CashRequest req)
    {
        if (req.TransactionType is not ("Receipt" or "Payment")) return BadRequest(new { message = "Loai giao dich khong hop le." });
        if (req.Amount <= 0) return BadRequest(new { message = "So tien khong hop le." });
        var entity = new GiaoDichTienMat
        {
            MaGiaoDichKinhDoanh = GenerateCode("CT"),
            LoaiGiaoDich = req.TransactionType,
            DanhMuc = string.IsNullOrWhiteSpace(req.Category) ? "Other" : req.Category!,
            SoTien = req.Amount,
            PhuongThuc = string.IsNullOrWhiteSpace(req.Method) ? "Cash" : req.Method!,
            LoaiThamChieu = TrimToNull(req.ReferenceType),
            MaThamChieu = req.ReferenceId,
            GhiChu = TrimToNull(req.Note),
            MaNguoiGhi = this.GetCurrentUserId(),
            NgayGiaoDich = req.OccurredAt ?? DateTime.UtcNow,
            NgayTao = DateTime.UtcNow
        };
        _db.GiaoDichTienMats.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "GiaoDichTienMat", entity.MaGiaoDich.ToString(), "Create", null, new { entity.LoaiGiaoDich, entity.SoTien });
        return Ok(new { id = entity.MaGiaoDich });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("cash/{id:int}/reverse")]
    public async Task<IActionResult> ReverseCash(int id)
    {
        var src = await _db.GiaoDichTienMats.FirstOrDefaultAsync(x => x.MaGiaoDich == id);
        if (src is null) return NotFound();
        if (src.LoaiThamChieu == "CashReversal") return BadRequest(new { message = "Khong the dao phieu dao." });
        var already = await _db.GiaoDichTienMats.AnyAsync(x => x.LoaiThamChieu == "CashReversal" && x.MaThamChieu == id);
        if (already) return BadRequest(new { message = "Phieu nay da duoc dao truoc do." });

        var reversal = new GiaoDichTienMat
        {
            MaGiaoDichKinhDoanh = GenerateCode("CT"),
            LoaiGiaoDich = src.LoaiGiaoDich == "Receipt" ? "Payment" : "Receipt",
            DanhMuc = src.DanhMuc,
            SoTien = src.SoTien,
            PhuongThuc = src.PhuongThuc,
            LoaiThamChieu = "CashReversal",
            MaThamChieu = id,
            GhiChu = $"Dao phieu {src.MaGiaoDichKinhDoanh}",
            MaNguoiGhi = this.GetCurrentUserId(),
            NgayGiaoDich = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow
        };
        _db.GiaoDichTienMats.Add(reversal);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "GiaoDichTienMat", reversal.MaGiaoDich.ToString(), "Reverse", null, new { source = id });
        return Ok(new { id = reversal.MaGiaoDich });
    }

    // ===== Repairs =====

    [HttpGet("repairs")]
    public async Task<IActionResult> GetRepairs()
    {
        var customerNames = await _db.Users.AsNoTracking().ToDictionaryAsync(x => x.MaNguoiDung, x => x.HoTen);
        var repairs = await _db.PhieuSuaChuas.AsNoTracking().Include(x => x.ChiTiet).Include(x => x.LichSu)
            .OrderByDescending(x => x.MaPhieuSua).ToListAsync();

        var items = repairs.Select(r => new
        {
            id = r.MaPhieuSua,
            code = r.MaPhieuSuaKinhDoanh,
            customerName = customerNames.GetValueOrDefault(r.MaKhachHang),
            storeId = r.MaCuaHang,
            vehicleDescription = r.MoTaXe,
            reportedIssue = r.MoTaLoi,
            repairStatus = r.TrangThai,
            total = r.ChiPhiCong + r.ChiPhiLinhKien,
            receivedAt = r.NgayTiepNhan,
            lines = r.ChiTiet.Select(l => new { id = l.MaChiTietSua, skuId = l.MaBienSanPham, description = l.MoTa, qty = l.SoLuong, unitPrice = l.DonGia }),
            histories = r.LichSu.OrderBy(h => h.MaLichSuSua).Select(h => new { id = h.MaLichSuSua, fromStatus = h.TrangThaiCu, toStatus = h.TrangThaiMoi, note = h.GhiChu, changedAt = h.ThoiGian })
        });
        return Ok(new { items });
    }

    [HttpPost("repairs")]
    public async Task<IActionResult> CreateRepair([FromBody] CreateRepairRequest req)
    {
        var storeId = req.StoreId > 0 ? req.StoreId : await DefaultStoreIdAsync();
        var lines = (req.Lines ?? new()).Select(l => new ChiTietSuaChua
        {
            MaBienSanPham = l.SkuId.HasValue && l.SkuId.Value > 0 ? l.SkuId : null,
            MoTa = string.IsNullOrWhiteSpace(l.Description) ? "Phu tung" : l.Description!.Trim(),
            SoLuong = l.Qty <= 0 ? 1 : l.Qty,
            DonGia = l.UnitPrice,
            NgayTao = DateTime.UtcNow
        }).ToList();

        var repair = new PhieuSuaChua
        {
            MaPhieuSuaKinhDoanh = GenerateCode("RO"),
            MaKhachHang = req.CustomerId,
            MaCuaHang = storeId,
            MaNhanVienPhuTrach = req.AssignedStaffId,
            MoTaXe = TrimToNull(req.VehicleDescription) ?? "",
            MoTaLoi = TrimToNull(req.ReportedIssue) ?? "",
            TrangThai = "Received",
            ChiPhiCong = req.LaborCost,
            ChiPhiLinhKien = lines.Sum(l => l.SoLuong * l.DonGia),
            DaXuatLinhKien = false,
            GhiChu = TrimToNull(req.Note),
            NgayTiepNhan = DateTime.UtcNow,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow,
            ChiTiet = lines,
            LichSu = new List<LichSuSuaChua> { new() { TrangThaiCu = null, TrangThaiMoi = "Received", GhiChu = "Tiep nhan", ThoiGian = DateTime.UtcNow } }
        };
        _db.PhieuSuaChuas.Add(repair);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuSuaChua", repair.MaPhieuSua.ToString(), "Create", null, new { repair.MaPhieuSuaKinhDoanh });
        return Ok(new { id = repair.MaPhieuSua });
    }

    [HttpPut("repairs/{id:int}/status")]
    public async Task<IActionResult> UpdateRepairStatus(int id, [FromBody] UpdateStatusRequest req)
    {
        var repair = await _db.PhieuSuaChuas.Include(x => x.ChiTiet).FirstOrDefaultAsync(x => x.MaPhieuSua == id);
        if (repair is null) return NotFound();
        var allowed = new[] { "Received", "Inspecting", "Quoted", "Repairing", "Completed", "Delivered", "Cancelled" };
        if (!allowed.Contains(req.Status)) return BadRequest(new { message = "Trang thai khong hop le." });

        var from = repair.TrangThai;
        if (req.Status == "Repairing" && !repair.DaXuatLinhKien)
        {
            foreach (var line in repair.ChiTiet.Where(l => l.MaBienSanPham.HasValue))
                await IncreaseStockAsync(line.MaBienSanPham!.Value, -line.SoLuong);
            repair.DaXuatLinhKien = true;
        }
        repair.TrangThai = req.Status!;
        if (req.Status == "Completed") repair.NgayHoanTat = DateTime.UtcNow;
        repair.NgayCapNhat = DateTime.UtcNow;
        repair.LichSu.Add(new LichSuSuaChua { MaPhieuSua = id, TrangThaiCu = from, TrangThaiMoi = req.Status!, GhiChu = TrimToNull(req.Note), ThoiGian = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "PhieuSuaChua", id.ToString(), "UpdateStatus", new { from }, new { to = req.Status });
        return Ok(new { id });
    }

    // ===== Interactions (CRM) =====

    [HttpGet("interactions")]
    public async Task<IActionResult> GetInteractions()
    {
        var names = await _db.Users.AsNoTracking().ToDictionaryAsync(x => x.MaNguoiDung, x => x.HoTen);
        var rows = await _db.TuongTacKhachHangs.AsNoTracking().OrderByDescending(x => x.MaTuongTac).ToListAsync();
        var items = rows.Select(x => new
        {
            id = x.MaTuongTac,
            customerId = x.MaKhachHang,
            customerName = names.GetValueOrDefault(x.MaKhachHang),
            assignedStaffId = x.MaNhanVienPhuTrach,
            interactionType = x.LoaiTuongTac,
            interactionStatus = x.TrangThai,
            subject = x.TieuDe,
            note = x.GhiChu,
            followUpAt = x.NgayHenFollowUp,
            completedAt = x.NgayHoanTat
        });
        return Ok(new { items });
    }

    [HttpPost("interactions")]
    public async Task<IActionResult> CreateInteraction([FromBody] InteractionRequest req)
    {
        var entity = new TuongTacKhachHang
        {
            MaKhachHang = req.CustomerId,
            MaNhanVienPhuTrach = req.AssignedStaffId,
            LoaiTuongTac = string.IsNullOrWhiteSpace(req.InteractionType) ? "Call" : req.InteractionType!,
            TrangThai = "Open",
            TieuDe = TrimToNull(req.Subject) ?? "",
            GhiChu = TrimToNull(req.Note),
            NgayHenFollowUp = req.FollowUpAt,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        _db.TuongTacKhachHangs.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(this, "TuongTacKhachHang", entity.MaTuongTac.ToString(), "Create");
        return Ok(new { id = entity.MaTuongTac });
    }

    [HttpPut("interactions/{id:int}")]
    public async Task<IActionResult> UpdateInteraction(int id, [FromBody] InteractionRequest req)
    {
        var entity = await _db.TuongTacKhachHangs.FirstOrDefaultAsync(x => x.MaTuongTac == id);
        if (entity is null) return NotFound();
        if (entity.TrangThai != "Open") return BadRequest(new { message = "Chi sua duoc lich dang mo." });
        entity.MaNhanVienPhuTrach = req.AssignedStaffId;
        if (!string.IsNullOrWhiteSpace(req.InteractionType)) entity.LoaiTuongTac = req.InteractionType!;
        entity.TieuDe = TrimToNull(req.Subject) ?? entity.TieuDe;
        entity.GhiChu = TrimToNull(req.Note);
        entity.NgayHenFollowUp = req.FollowUpAt;
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    [HttpPost("interactions/{id:int}/complete")]
    public async Task<IActionResult> CompleteInteraction(int id)
    {
        var entity = await _db.TuongTacKhachHangs.FirstOrDefaultAsync(x => x.MaTuongTac == id);
        if (entity is null) return NotFound();
        entity.TrangThai = "Completed";
        entity.NgayHoanTat = DateTime.UtcNow;
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    [HttpPost("interactions/{id:int}/cancel")]
    public async Task<IActionResult> CancelInteraction(int id)
    {
        var entity = await _db.TuongTacKhachHangs.FirstOrDefaultAsync(x => x.MaTuongTac == id);
        if (entity is null) return NotFound();
        if (entity.TrangThai != "Open") return BadRequest(new { message = "Chi huy duoc lich dang mo." });
        entity.TrangThai = "Cancelled";
        entity.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    // ===== Attendance =====

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance()
    {
        var names = await _db.Users.AsNoTracking().ToDictionaryAsync(x => x.MaNguoiDung, x => x.HoTen);
        var storeNames = await _db.CuaHangs.AsNoTracking().ToDictionaryAsync(x => x.MaCuaHang, x => x.TenCuaHang);
        var rows = await _db.ChamCongs.AsNoTracking().OrderByDescending(x => x.MaChamCong).Take(500).ToListAsync();
        var items = rows.Select(x => new
        {
            id = x.MaChamCong,
            staffUserId = x.MaNhanVien,
            staffName = names.GetValueOrDefault(x.MaNhanVien),
            storeId = x.MaCuaHang,
            storeName = storeNames.GetValueOrDefault(x.MaCuaHang),
            checkInAt = x.ThoiGianVao,
            checkOutAt = x.ThoiGianRa,
            note = x.GhiChu
        });
        return Ok(new { items });
    }

    [HttpPost("attendance/check-in")]
    public async Task<IActionResult> CheckIn([FromBody] AttendanceRequest req)
    {
        var me = this.GetCurrentUserId();
        var staffId = req.StaffUserId > 0 ? req.StaffUserId : me;
        if (!IsAdmin && staffId != me) return BadRequest(new { message = "Chi duoc check-in cho ban than." });
        var storeId = req.StoreId > 0 ? req.StoreId : await DefaultStoreIdAsync();

        var open = await _db.ChamCongs.AnyAsync(x => x.MaNhanVien == staffId && x.ThoiGianRa == null);
        if (open) return BadRequest(new { message = "Ban dang co ca chua check-out." });

        var entity = new ChamCong { MaNhanVien = staffId, MaCuaHang = storeId, ThoiGianVao = DateTime.UtcNow, GhiChu = TrimToNull(req.Note), NgayTao = DateTime.UtcNow };
        _db.ChamCongs.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new { id = entity.MaChamCong });
    }

    [HttpPost("attendance/{id:int}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        var entity = await _db.ChamCongs.FirstOrDefaultAsync(x => x.MaChamCong == id);
        if (entity is null) return NotFound();
        if (!IsAdmin && entity.MaNhanVien != this.GetCurrentUserId()) return BadRequest(new { message = "Chi duoc check-out ca cua ban than." });
        if (entity.ThoiGianRa != null) return BadRequest(new { message = "Ca nay da check-out." });
        entity.ThoiGianRa = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    // ===== Helpers =====

    private static string? TrimToNull(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private async Task<int> DefaultStoreIdAsync()
    {
        var id = await _db.CuaHangs.OrderBy(x => x.MaCuaHang).Select(x => x.MaCuaHang).FirstOrDefaultAsync();
        return id == 0 ? 1 : id;
    }

    private async Task IncreaseStockAsync(int maBienSanPham, int delta)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE dbo.BIENSANPHAM SET SoLuongTon = CASE WHEN ISNULL(SoLuongTon,0) + {delta} < 0 THEN 0 ELSE ISNULL(SoLuongTon,0) + {delta} END WHERE MaBienSanPham = {maBienSanPham}");
    }
}

// ===== Request DTOs =====
public class SupplierRequest
{
    public int? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? TaxCode { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    public int? Status { get; set; }
}

public class CreatePurchaseRequest
{
    public int SupplierId { get; set; }
    public int StoreId { get; set; }
    public string? Note { get; set; }
    public List<PurchaseLineRequest> Lines { get; set; } = new();
}
public class PurchaseLineRequest { public int SkuId { get; set; } public int Qty { get; set; } public decimal UnitCost { get; set; } }
public class ReceivePurchaseRequest { public string? Note { get; set; } public List<ReceiveLineRequest> Lines { get; set; } = new(); }
public class ReceiveLineRequest { public int PurchaseOrderLineId { get; set; } public int Qty { get; set; } }
public class PayPurchaseRequest { public decimal Amount { get; set; } public string? Method { get; set; } public string? Note { get; set; } }

public class CashRequest
{
    public string TransactionType { get; set; } = "Receipt";
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Note { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class CreateRepairRequest
{
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public int? AssignedStaffId { get; set; }
    public string? VehicleDescription { get; set; }
    public string? ReportedIssue { get; set; }
    public decimal LaborCost { get; set; }
    public string? Note { get; set; }
    public List<RepairLineRequest> Lines { get; set; } = new();
}
public class RepairLineRequest { public int? SkuId { get; set; } public string? Description { get; set; } public int Qty { get; set; } public decimal UnitPrice { get; set; } }
public class UpdateStatusRequest { public string? Status { get; set; } public string? Note { get; set; } }

public class InteractionRequest
{
    public int CustomerId { get; set; }
    public int? AssignedStaffId { get; set; }
    public string? InteractionType { get; set; }
    public string? Subject { get; set; }
    public string? Note { get; set; }
    public DateTime? FollowUpAt { get; set; }
}

public class AttendanceRequest { public int StaffUserId { get; set; } public int StoreId { get; set; } public string? Note { get; set; } }

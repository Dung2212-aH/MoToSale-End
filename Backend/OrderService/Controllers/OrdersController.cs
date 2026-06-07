using OrderService.Data;
using OrderService.DTOs.Orders;
using OrderService.Entities;
using OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    // Simplified order lifecycle: an order is either waiting for the customer's payment,
    // confirmed (admin will use shippingStatus to track preparation/shipping/delivery), or cancelled.
    private static readonly HashSet<string> AllowedAdminOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "AwaitingPayment",
        "Confirmed",
        "Cancelled"
    };

    // Aligned with CK_DONHANG_PaymentStatus: order-level payment status can never be Pending or
    // Failed (those are per-transaction concepts tracked on THANHTOAN.TrangThai instead).
    private static readonly HashSet<string> AllowedAdminPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unpaid",
        "Paid",
        "PartiallyPaid",
        "Refunded",
        "Cancelled"
    };

    private static readonly HashSet<string> AllowedAdminShippingStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Preparing",
        "Shipping",
        "Delivered"
    };

    private static readonly HashSet<string> LockedOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cancelled"
    };

    private static readonly HashSet<string> InitialPaymentOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Checkout",
        "AwaitingPayment"
    };

    private static readonly HashSet<string> CancelBlockedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cancelled"
    };

    private static readonly HashSet<string> SuccessfulPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Paid",
        "PartiallyPaid",
        "Refunded"
    };

    private static readonly Dictionary<string, string[]> AllowedOrderTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AwaitingPayment"] = new[] { "Confirmed", "Cancelled" },
        ["Confirmed"] = new[] { "Cancelled" },
        ["Pending"] = new[] { "AwaitingPayment", "Confirmed", "Cancelled" },
        ["Checkout"] = new[] { "AwaitingPayment", "Confirmed", "Cancelled" }
    };

    private readonly IOrderService _orderService;
    private readonly OrderDbContext _dbContext;
    private readonly IAuditLogService _auditLog;

    public OrdersController(IOrderService orderService, OrderDbContext dbContext, IAuditLogService auditLog)
    {
        _orderService = orderService;
        _dbContext = dbContext;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderSearchDto search)
    {
        try
        {
            return Ok(await _orderService.GetOrdersAsync(search, this.GetCurrentUserId(), this.CanManageOrders()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            await EnsureOrderHistoryTableAsync();
            await BackfillOrderHistoryIfEmptyAsync(id);
            return Ok(await _orderService.GetOrderByIdAsync(id, this.GetCurrentUserId(), this.CanManageOrders()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateFromCart(CreateOrderFromCartRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(this.GetCurrentUserId(), request);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.MaDonHang }, order);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost("shipping-quote")]
    public async Task<IActionResult> GetShippingQuote(ShippingQuoteRequest request)
    {
        try
        {
            return Ok(await _orderService.GetShippingQuoteAsync(this.GetCurrentUserId(), request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelOrderRequest request)
    {
        try
        {
            await EnsureOrderHistoryTableAsync();
            var before = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.MaDonHang == id);
            var result = await _orderService.CancelOrderAsync(id, this.GetCurrentUserId(), this.CanManageOrders(), request);
            if (before is not null && this.CanManageOrders())
            {
                var now = DateTime.UtcNow;
                AddHistory(id, "OrderStatus", before.TrangThaiDonHang, result.TrangThaiDonHang, request.LyDoHuyDon, now);
                AddHistory(id, "PaymentStatus", before.TrangThaiThanhToan, result.TrangThaiThanhToan, null, now);
                AddHistory(id, "ShippingStatus", before.TrangThaiVanChuyen, result.TrangThaiVanChuyen, null, now);
                await _dbContext.SaveChangesAsync();
                await _auditLog.WriteAsync(this, "Order", id.ToString(), "Cancel", new { before.TrangThaiDonHang, before.TrangThaiThanhToan, before.TrangThaiVanChuyen }, new { result.TrangThaiDonHang, result.TrangThaiThanhToan, result.TrangThaiVanChuyen }, request.LyDoHuyDon);
                result = await _orderService.GetOrderByIdAsync(id, this.GetCurrentUserId(), true);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("installments")]
    public async Task<IActionResult> GetInstallmentSchedule([FromQuery] string? status = "Pending")
    {
        try
        {
            var query = _dbContext.InstallmentTerms
                .Include(t => t.Plan!)
                .ThenInclude(p => p.Order!)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                var normalized = status.Trim();
                query = query.Where(t => t.TrangThai == normalized);
            }

            var rows = await query
                .OrderBy(t => t.NgayDenHan)
                .Take(500)
                .Select(t => new
                {
                    maKyTraGop = t.MaKyTraGop,
                    maHoSoTraGop = t.MaHoSoTraGop,
                    kyThu = t.KyThu,
                    ngayDenHan = t.NgayDenHan,
                    soTienGoc = t.SoTienGoc,
                    soTienLai = t.SoTienLai,
                    tongTien = t.TongTien,
                    trangThai = t.TrangThai,
                    ngayThanhToan = t.NgayThanhToan,
                    maDonHang = t.Plan!.MaDonHang,
                    maDonHangKinhDoanh = t.Plan!.Order!.MaDonHangKinhDoanh,
                    hoTenNguoiVay = t.Plan!.HoTenNguoiVay,
                    soCCCD = t.Plan!.SoCCCD,
                    soDienThoai = t.Plan!.SoDienThoai,
                    soKy = t.Plan!.SoKy
                })
                .ToListAsync();

            return Ok(new { items = rows, count = rows.Count });
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("{id:int}/payment-info")]
    public async Task<IActionResult> GetPaymentInfo(int id)
    {
        try
        {
            return Ok(await _orderService.GetPaymentInfoAsync(id, this.GetCurrentUserId(), this.CanManageOrders()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost("{id:int}/request-refund")]
    public async Task<IActionResult> RequestRefund(int id, CreateRefundRequestDto request)
    {
        try
        {
            await EnsureOrderHistoryTableAsync();
            var before = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.MaDonHang == id);
            var result = await _orderService.RequestRefundAsync(id, this.GetCurrentUserId(), request);
            if (before is not null)
            {
                var now = DateTime.UtcNow;
                AddHistory(id, "OrderStatus", before.TrangThaiDonHang, result.TrangThaiDonHang, "Khach yeu cau hoan tien", now);
                await _dbContext.SaveChangesAsync();
                await _auditLog.WriteAsync(this, "Order", id.ToString(), "RequestRefund",
                    new { before.TrangThaiDonHang, before.TrangThaiThanhToan },
                    new { result.TrangThaiDonHang, result.TrangThaiThanhToan },
                    request.LyDo);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:int}/refunds/{refundId:int}/confirm")]
    public async Task<IActionResult> ConfirmRefund(int id, int refundId, ConfirmRefundRequest request)
    {
        try
        {
            await EnsureOrderHistoryTableAsync();
            var before = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.MaDonHang == id);
            var result = await _orderService.ConfirmRefundAsync(id, refundId, request);
            if (before is not null)
            {
                var now = DateTime.UtcNow;
                AddHistory(id, "PaymentStatus", before.TrangThaiThanhToan, result.TrangThaiThanhToan, "Admin xac nhan da hoan tien", now);
                await _dbContext.SaveChangesAsync();
                await _auditLog.WriteAsync(this, "Order", id.ToString(), "ConfirmRefund",
                    new { before.TrangThaiThanhToan, refundId },
                    new { result.TrangThaiThanhToan },
                    request.MaGiaoDichHoan ?? request.GhiChuAdmin);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("{id:int}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(int id, ConfirmOrderPaymentRequest request)
    {
        try
        {
            await EnsureOrderHistoryTableAsync();
            var before = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.MaDonHang == id);
            var result = await _orderService.ConfirmOrderPaymentAsync(id, request);

            if (before is not null)
            {
                var now = DateTime.UtcNow;
                var note = request.MaKyTraGop.HasValue
                    ? $"Xac nhan ky tra gop #{request.MaKyTraGop}"
                    : "Xac nhan da nhan thanh toan";
                AddHistory(id, "OrderStatus", before.TrangThaiDonHang, result.TrangThaiDonHang, note, now);
                AddHistory(id, "PaymentStatus", before.TrangThaiThanhToan, result.TrangThaiThanhToan, request.GhiChu ?? note, now);
                await _dbContext.SaveChangesAsync();
                await _auditLog.WriteAsync(this, "Order", id.ToString(), "ConfirmPayment",
                    new { before.TrangThaiDonHang, before.TrangThaiThanhToan },
                    new { result.TrangThaiDonHang, result.TrangThaiThanhToan },
                    request.MaGiaoDich ?? request.GhiChu);
                result = await _orderService.GetOrderByIdAsync(id, this.GetCurrentUserId(), true);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}/status")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var status = request.TrangThaiDonHang ?? request.Status;
        var paymentStatus = request.TrangThaiThanhToan ?? request.PaymentStatus;
        var shippingStatus = request.TrangThaiVanChuyen ?? request.ShippingStatus;
        if (string.IsNullOrWhiteSpace(status) && string.IsNullOrWhiteSpace(paymentStatus) && string.IsNullOrWhiteSpace(shippingStatus))
        {
            return BadRequest(new { message = "Trang thai don hang, thanh toan hoac van chuyen la bat buoc." });
        }

        string? normalizedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            normalizedStatus = AllowedAdminOrderStatuses.FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
            if (normalizedStatus is null)
            {
                return BadRequest(new { message = "Trang thai don hang khong hop le." });
            }
        }

        var order = await _dbContext.Orders
            .Include(o => o.InventoryHolds)
            .Include(o => o.Vouchers)
            .FirstOrDefaultAsync(o => o.MaDonHang == id);
        if (order is null)
        {
            return NotFound(new { message = "Khong tim thay don hang." });
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var now = DateTime.UtcNow;
        var oldOrderStatus = order.TrangThaiDonHang;
        var oldPaymentStatus = order.TrangThaiThanhToan;
        var oldShippingStatus = order.TrangThaiVanChuyen;

        if (normalizedStatus is not null)
        {
            var transitionError = ValidateOrderTransition(order.TrangThaiDonHang, normalizedStatus);
            if (transitionError is not null)
            {
                return BadRequest(new { message = transitionError });
            }

            if (normalizedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.LyDoHuyDon))
                {
                    return BadRequest(new { message = "Ly do huy don la bat buoc." });
                }

                await ApplyOrderCancellationAsync(order, request.LyDoHuyDon.Trim(), now);
            }

            order.TrangThaiDonHang = normalizedStatus;
        }

        if (!string.IsNullOrWhiteSpace(shippingStatus))
        {
            var normalizedShippingStatus = AllowedAdminShippingStatuses.FirstOrDefault(x => x.Equals(shippingStatus.Trim(), StringComparison.OrdinalIgnoreCase));
            if (normalizedShippingStatus is null)
            {
                return BadRequest(new { message = "Trang thai van chuyen khong hop le." });
            }

            if (LockedOrderStatuses.Contains(order.TrangThaiDonHang))
            {
                return BadRequest(new { message = "Don hang da khoa, khong the cap nhat van chuyen." });
            }

            order.TrangThaiVanChuyen = normalizedShippingStatus;
        }

        if (!string.IsNullOrWhiteSpace(request.GhiChuGiaoNhan))
        {
            order.GhiChuGiaoNhan = request.GhiChuGiaoNhan.Trim();
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            var normalizedPaymentStatus = AllowedAdminPaymentStatuses.FirstOrDefault(x => x.Equals(paymentStatus.Trim(), StringComparison.OrdinalIgnoreCase));
            if (normalizedPaymentStatus is null)
            {
                return BadRequest(new { message = "Trang thai thanh toan khong hop le." });
            }

            if (normalizedPaymentStatus.Equals("Refunded", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(request.GhiChuThanhToan ?? request.PaymentNote ?? request.LyDoHuyDon))
            {
                return BadRequest(new { message = "Ly do/ghi chu hoan tien la bat buoc." });
            }

            order.TrangThaiThanhToan = normalizedPaymentStatus;
            if (normalizedPaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) && order.NgayThanhToanThanhCong is null)
            {
                order.NgayThanhToanThanhCong = now;
            }

            // Both Paid and PartiallyPaid (e.g. deposit received, first instalment received) auto-
            // confirm an AwaitingPayment order so shipping can proceed.
            var triggersConfirm = normalizedPaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)
                || normalizedPaymentStatus.Equals("PartiallyPaid", StringComparison.OrdinalIgnoreCase);
            if (triggersConfirm && normalizedStatus is null && InitialPaymentOrderStatuses.Contains(order.TrangThaiDonHang))
            {
                var confirmError = await TryConfirmOrderAndDeductStockAsync(order, now);
                if (confirmError is not null)
                {
                    return BadRequest(new { message = confirmError });
                }
            }

            var paymentNote = request.GhiChuThanhToan ?? request.PaymentNote;
            if (!string.IsNullOrWhiteSpace(paymentNote))
            {
                order.GhiChu = AppendNote(order.GhiChu, paymentNote.Trim());
            }
        }

        await EnsureOrderHistoryTableAsync();
        AddHistory(id, "OrderStatus", oldOrderStatus, order.TrangThaiDonHang, request.LyDoHuyDon, now);
        AddHistory(id, "PaymentStatus", oldPaymentStatus, order.TrangThaiThanhToan, request.GhiChuThanhToan ?? request.PaymentNote, now);
        AddHistory(id, "ShippingStatus", oldShippingStatus, order.TrangThaiVanChuyen, request.GhiChuGiaoNhan, now);
        order.NgayCapNhat = now;
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(
            this,
            "Order",
            id.ToString(),
            "UpdateStatus",
            new { TrangThaiDonHang = oldOrderStatus, TrangThaiThanhToan = oldPaymentStatus, TrangThaiVanChuyen = oldShippingStatus },
            new { order.TrangThaiDonHang, order.TrangThaiThanhToan, order.TrangThaiVanChuyen },
            request.LyDoHuyDon ?? request.GhiChuThanhToan ?? request.PaymentNote ?? request.GhiChuGiaoNhan);
        await transaction.CommitAsync();

        return Ok(await _orderService.GetOrderByIdAsync(id, this.GetCurrentUserId(), true));
    }

    private static string? ValidateOrderTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (LockedOrderStatuses.Contains(currentStatus))
        {
            return "Don hang da khoa, khong the doi trang thai.";
        }

        if (nextStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && CancelBlockedStatuses.Contains(currentStatus))
        {
            return "Don hang hien tai khong the huy.";
        }

        return AllowedOrderTransitions.TryGetValue(currentStatus, out var allowed) &&
               allowed.Any(x => x.Equals(nextStatus, StringComparison.OrdinalIgnoreCase))
            ? null
            : "Chuyen trang thai don hang khong hop le.";
    }

    private async Task<string?> TryConfirmOrderAndDeductStockAsync(OrderService.Entities.Order order, DateTime now)
    {
        var activeHolds = order.InventoryHolds
            .Where(h => h.TrangThai == "Active" && h.HetHanLuc > now)
            .ToList();

        if (activeHolds.Count == 0)
        {
            return "Don hang khong con giu cho ton kho hieu luc. Vui long checkout lai hoac cap nhat ton kho thu cong truoc khi xac nhan.";
        }

        var variantUpdates = new List<(OrderService.Entities.ProductVariant Variant, int RequiredQuantity)>();
        var productUpdates = new List<(OrderService.Entities.Product Product, int RequiredQuantity)>();

        foreach (var group in activeHolds.Where(h => h.MaBienSanPham.HasValue).GroupBy(h => h.MaBienSanPham!.Value))
        {
            var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == group.Key);
            if (variant is null)
            {
                return "Bien the san pham trong don hang khong ton tai.";
            }

            var requiredQuantity = group.Sum(h => h.SoLuong);
            var stock = variant.SoLuongTon ?? 0;
            if (stock < requiredQuantity)
            {
                return "So luong ton kho bien the khong du de xac nhan don hang.";
            }

            variantUpdates.Add((variant, requiredQuantity));
        }

        foreach (var group in activeHolds.Where(h => !h.MaBienSanPham.HasValue).GroupBy(h => h.MaSanPham))
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == group.Key);
            if (product is null || product.SoLuongTon < group.Sum(h => h.SoLuong))
            {
                return product is null
                    ? "San pham trong don hang khong ton tai."
                    : "So luong ton kho san pham khong du de xac nhan don hang.";
            }

            productUpdates.Add((product, group.Sum(h => h.SoLuong)));
        }

        foreach (var (variant, requiredQuantity) in variantUpdates)
        {
            variant.SoLuongTon = (variant.SoLuongTon ?? 0) - requiredQuantity;
            variant.NgayCapNhat = now;
        }

        foreach (var (product, requiredQuantity) in productUpdates)
        {
            product.SoLuongTon -= requiredQuantity;
            product.NgayCapNhat = now;
        }

        foreach (var hold in activeHolds)
        {
            hold.TrangThai = "Confirmed";
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, "Da xac nhan thanh toan va tru ton kho");
        }

        order.TrangThaiDonHang = "Confirmed";
        order.NgayCapNhat = now;
        return null;
    }

    // Shipping is fully decoupled from orderStatus now (orderStatus ∈ AwaitingPayment/Confirmed/Cancelled).
    // Admin updates shippingStatus independently via the modal in OrderDetail.

    private async Task ApplyOrderCancellationAsync(OrderService.Entities.Order order, string reason, DateTime now)
    {
        order.NgayHuyDon ??= now;
        order.LyDoHuyDon = reason;
        if (!SuccessfulPaymentStatuses.Contains(order.TrangThaiThanhToan))
        {
            order.TrangThaiThanhToan = "Cancelled";
        }
        foreach (var hold in order.InventoryHolds.Where(h => h.TrangThai is "Active" or "Confirmed"))
        {
            if (hold.TrangThai.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                if (hold.MaBienSanPham.HasValue)
                {
                    var variant = await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == hold.MaBienSanPham.Value);
                    if (variant is not null)
                    {
                        variant.SoLuongTon = (variant.SoLuongTon ?? 0) + hold.SoLuong;
                        variant.NgayCapNhat = now;
                    }
                }
                else
                {
                    var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == hold.MaSanPham);
                    if (product is not null)
                    {
                        product.SoLuongTon += hold.SoLuong;
                        product.NgayCapNhat = now;
                    }
                }
            }

            hold.TrangThai = "Cancelled";
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, "Huy don, giai phong ton kho giu cho");
        }

        if (order.Vouchers.Any())
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC dbo.sp_Voucher_HuySuDungTheoDon @MaDonHang={order.MaDonHang}");
        }
    }

    private static string AppendNote(string? current, string note)
    {
        return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
    }

    private void AddHistory(int orderId, string eventType, string? oldValue, string? newValue, string? note, DateTime time)
    {
        if (string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        _dbContext.OrderHistories.Add(new OrderHistory
        {
            MaDonHang = orderId,
            LoaiSuKien = eventType,
            GiaTriCu = oldValue,
            GiaTriMoi = newValue,
            GhiChu = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            MaNguoiThucHien = this.GetCurrentUserId(),
            ThoiGian = time
        });
    }

    private async Task EnsureOrderHistoryTableAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[dbo].[DONHANG_LICHSU_TRANGTHAI]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DONHANG_LICHSU_TRANGTHAI](
                    [MaLichSuDonHang] [int] IDENTITY(1,1) NOT NULL,
                    [MaDonHang] [int] NOT NULL,
                    [LoaiSuKien] [varchar](30) NOT NULL,
                    [GiaTriCu] [varchar](50) NULL,
                    [GiaTriMoi] [varchar](50) NULL,
                    [GhiChu] [nvarchar](500) NULL,
                    [MaNguoiThucHien] [int] NULL,
                    [ThoiGian] [datetime2](0) NOT NULL,
                    CONSTRAINT [PK_DONHANG_LICHSU_TRANGTHAI] PRIMARY KEY CLUSTERED ([MaLichSuDonHang] ASC)
                );

                ALTER TABLE [dbo].[DONHANG_LICHSU_TRANGTHAI] WITH CHECK ADD CONSTRAINT [FK_DONHANG_LICHSU_TRANGTHAI_DONHANG]
                    FOREIGN KEY([MaDonHang]) REFERENCES [dbo].[DONHANG] ([MaDonHang]);

                CREATE INDEX [IX_DONHANG_LICHSU_TRANGTHAI_MaDonHang_ThoiGian]
                    ON [dbo].[DONHANG_LICHSU_TRANGTHAI]([MaDonHang], [ThoiGian]);
            END
            """);
    }

    private async Task BackfillOrderHistoryIfEmptyAsync(int orderId)
    {
        if (await _dbContext.OrderHistories.AnyAsync(h => h.MaDonHang == orderId))
        {
            return;
        }

        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.MaDonHang == orderId);
        if (order is null)
        {
            return;
        }

        if (!IsInitialOrderStatus(order.TrangThaiDonHang))
        {
            _dbContext.OrderHistories.Add(new OrderHistory
            {
                MaDonHang = order.MaDonHang,
                LoaiSuKien = "OrderStatus",
                GiaTriCu = null,
                GiaTriMoi = order.TrangThaiDonHang,
                GhiChu = "Khoi tao lich su tu trang thai hien tai cua don cu",
                MaNguoiThucHien = null,
                ThoiGian = order.NgayCapNhat
            });
        }

        if (!string.Equals(order.TrangThaiThanhToan, "Unpaid", StringComparison.OrdinalIgnoreCase))
        {
            _dbContext.OrderHistories.Add(new OrderHistory
            {
                MaDonHang = order.MaDonHang,
                LoaiSuKien = "PaymentStatus",
                GiaTriCu = null,
                GiaTriMoi = order.TrangThaiThanhToan,
                GhiChu = "Khoi tao lich su tu trang thai thanh toan hien tai cua don cu",
                MaNguoiThucHien = null,
                ThoiGian = order.NgayThanhToanThanhCong ?? order.NgayCapNhat
            });
        }

        if (!string.Equals(order.TrangThaiVanChuyen, "Preparing", StringComparison.OrdinalIgnoreCase))
        {
            _dbContext.OrderHistories.Add(new OrderHistory
            {
                MaDonHang = order.MaDonHang,
                LoaiSuKien = "ShippingStatus",
                GiaTriCu = null,
                GiaTriMoi = order.TrangThaiVanChuyen,
                GhiChu = "Khoi tao lich su tu trang thai van chuyen hien tai cua don cu",
                MaNguoiThucHien = null,
                ThoiGian = order.NgayCapNhat
            });
        }

        if (order.NgayHuyDon.HasValue)
        {
            _dbContext.OrderHistories.Add(new OrderHistory
            {
                MaDonHang = order.MaDonHang,
                LoaiSuKien = "OrderStatus",
                GiaTriCu = null,
                GiaTriMoi = "Cancelled",
                GhiChu = order.LyDoHuyDon,
                MaNguoiThucHien = null,
                ThoiGian = order.NgayHuyDon.Value
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    private static bool IsInitialOrderStatus(string status)
    {
        return status.Equals("AwaitingPayment", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Checkout", StringComparison.OrdinalIgnoreCase);
    }
}

public class UpdateOrderStatusRequest
{
    public string? TrangThaiDonHang { get; set; }
    public string? Status { get; set; }
    public string? TrangThaiVanChuyen { get; set; }
    public string? ShippingStatus { get; set; }
    public string? GhiChuGiaoNhan { get; set; }
    public string? GhiChuThanhToan { get; set; }
    public string? PaymentNote { get; set; }
    public string? LyDoHuyDon { get; set; }
    public string? TrangThaiThanhToan { get; set; }
    public string? PaymentStatus { get; set; }
}

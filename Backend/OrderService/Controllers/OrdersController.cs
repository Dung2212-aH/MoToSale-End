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
    private static readonly HashSet<string> AllowedAdminOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "AwaitingPayment",
        "Confirmed",
        "Processing",
        "Shipping",
        "Delivered",
        "Completed",
        "Cancelled"
    };

    private static readonly HashSet<string> AllowedAdminPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Unpaid",
        "Pending",
        "Paid",
        "PartiallyPaid",
        "Failed",
        "Refunded",
        "Cancelled"
    };

    private static readonly HashSet<string> AllowedAdminShippingStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "NotShipped",
        "Preparing",
        "Shipping",
        "Delivered",
        "PickupReady",
        "PickedUp",
        "Cancelled"
    };

    private static readonly HashSet<string> LockedOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Completed",
        "Cancelled"
    };

    private static readonly HashSet<string> CancelBlockedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Delivered",
        "Completed",
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
        ["Confirmed"] = new[] { "Processing", "Cancelled" },
        ["Processing"] = new[] { "Shipping", "Cancelled" },
        ["Shipping"] = new[] { "Delivered" },
        ["Delivered"] = new[] { "Completed" },
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
            else
            {
                ApplyShippingSyncForOrderStatus(order, normalizedStatus);
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

            var paymentNote = request.GhiChuThanhToan ?? request.PaymentNote;
            if (!string.IsNullOrWhiteSpace(paymentNote))
            {
                order.GhiChu = AppendNote(order.GhiChu, paymentNote.Trim());
            }
        }

        ReconcileShippingWithOrderStatus(order);
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

    private static void ApplyShippingSyncForOrderStatus(OrderService.Entities.Order order, string nextStatus)
    {
        if (nextStatus.Equals("Processing", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Preparing";
        }
        else if (nextStatus.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Shipping";
        }
        else if (nextStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Delivered";
        }
    }

    private static void ReconcileShippingWithOrderStatus(OrderService.Entities.Order order)
    {
        if (order.TrangThaiDonHang.Equals("Processing", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Preparing";
        }
        else if (order.TrangThaiDonHang.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Shipping";
        }
        else if (order.TrangThaiDonHang.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
                 order.TrangThaiDonHang.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Delivered";
        }
        else if (order.TrangThaiDonHang.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            order.TrangThaiVanChuyen = "Cancelled";
        }
    }

    private async Task ApplyOrderCancellationAsync(OrderService.Entities.Order order, string reason, DateTime now)
    {
        order.NgayHuyDon ??= now;
        order.LyDoHuyDon = reason;
        if (!SuccessfulPaymentStatuses.Contains(order.TrangThaiThanhToan))
        {
            order.TrangThaiThanhToan = "Cancelled";
        }
        order.TrangThaiVanChuyen = "Cancelled";

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

        if (!string.Equals(order.TrangThaiVanChuyen, "NotShipped", StringComparison.OrdinalIgnoreCase))
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

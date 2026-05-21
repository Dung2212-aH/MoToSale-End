using OrderService.Data;
using OrderService.DTOs.Orders;
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
        "Pending",
        "Checkout",
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
        "Cancelled"
    };

    private readonly IOrderService _orderService;
    private readonly OrderDbContext _dbContext;

    public OrdersController(IOrderService orderService, OrderDbContext dbContext)
    {
        _orderService = orderService;
        _dbContext = dbContext;
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
            return Ok(await _orderService.CancelOrderAsync(id, this.GetCurrentUserId(), this.CanManageOrders(), request));
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
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { message = "Trang thai don hang la bat buoc." });
        }

        var normalizedStatus = AllowedAdminOrderStatuses.FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedStatus is null)
        {
            return BadRequest(new { message = "Trang thai don hang khong hop le." });
        }

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.MaDonHang == id);
        if (order is null)
        {
            return NotFound(new { message = "Khong tim thay don hang." });
        }

        order.TrangThaiDonHang = normalizedStatus;
        if (!string.IsNullOrWhiteSpace(request.TrangThaiVanChuyen))
        {
            order.TrangThaiVanChuyen = request.TrangThaiVanChuyen.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.GhiChuGiaoNhan))
        {
            order.GhiChuGiaoNhan = request.GhiChuGiaoNhan.Trim();
        }

        var paymentStatus = request.TrangThaiThanhToan ?? request.PaymentStatus;
        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            var normalizedPaymentStatus = AllowedAdminPaymentStatuses.FirstOrDefault(x => x.Equals(paymentStatus.Trim(), StringComparison.OrdinalIgnoreCase));
            if (normalizedPaymentStatus is null)
            {
                return BadRequest(new { message = "Trang thai thanh toan khong hop le." });
            }

            order.TrangThaiThanhToan = normalizedPaymentStatus;
            if (normalizedPaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) && order.NgayThanhToanThanhCong is null)
            {
                order.NgayThanhToanThanhCong = DateTime.UtcNow;
            }
        }

        if (normalizedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            order.NgayHuyDon ??= DateTime.UtcNow;
            order.LyDoHuyDon = string.IsNullOrWhiteSpace(request.LyDoHuyDon) ? order.LyDoHuyDon : request.LyDoHuyDon.Trim();
        }

        order.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(await _orderService.GetOrderByIdAsync(id, this.GetCurrentUserId(), true));
    }
}

public class UpdateOrderStatusRequest
{
    public string? TrangThaiDonHang { get; set; }
    public string? Status { get; set; }
    public string? TrangThaiVanChuyen { get; set; }
    public string? GhiChuGiaoNhan { get; set; }
    public string? LyDoHuyDon { get; set; }
    public string? TrangThaiThanhToan { get; set; }
    public string? PaymentStatus { get; set; }
}

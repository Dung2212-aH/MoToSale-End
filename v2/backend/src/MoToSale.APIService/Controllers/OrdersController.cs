using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common.Auth;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Audit;
using MoToSale.Repository;
using MoToSale.Services.Ordering;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly AppDbContext _db;

    public OrdersController(IOrderService orders, AppDbContext db)
    {
        _orders = orders;
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? UserIdOrNull => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool IsStaff => User.IsInRole(RoleConstant.Admin) || User.IsInRole(RoleConstant.Staff);

    private async Task AddAuditAsync(int orderId, string action, string? newValue = null)
    {
        var now = DateTime.UtcNow;
        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Order",
            EntityId = orderId.ToString(),
            Action = action,
            NewValueJson = newValue,
            ActorId = UserIdOrNull,
            ActorName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email),
            At = now,
            CreatedDate = now
        });
        await _db.SaveChangesAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        try { return Ok(new { id = await _orders.CheckoutAsync(CurrentUserId, request) }); }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(new { items = await _orders.GetMyOrdersAsync(CurrentUserId) });

    /// <summary>
    /// Role-aware: admin/staff → tìm kiếm phân trang toàn bộ đơn; khách hàng → danh sách đơn của chính họ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] OrderSearchRequest request)
    {
        if (IsStaff) return Ok(await _orders.SearchOrdersAsync(request));
        return Ok(new { items = await _orders.GetMyOrdersAsync(CurrentUserId) });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orders.GetOrderAsync(id);
        if (order is null) return NotFound();
        if (!IsStaff && order.UserId != CurrentUserId) return NotFound();
        return Ok(order);
    }

    [HttpPost("{id:int}/cancel")]
    public Task<IActionResult> Cancel(int id, CancelOrderRequest request) => CancelCoreAsync(id, request);

    // Frontend dùng PUT cho hủy đơn — cùng logic với POST.
    [HttpPut("{id:int}/cancel")]
    public Task<IActionResult> CancelPut(int id, CancelOrderRequest request) => CancelCoreAsync(id, request);

    private async Task<IActionResult> CancelCoreAsync(int id, CancelOrderRequest request)
    {
        if (!IsStaff)
        {
            var order = await _orders.GetOrderAsync(id);
            if (order is null || order.UserId != CurrentUserId) return NotFound();
        }
        try
        {
            await _orders.CancelOrderAsync(id, request.Reason, UserIdOrNull);
            await AddAuditAsync(id, "Cancel", request.Reason);
            return Ok(new { message = "Đã hủy đơn." });
        }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Thông tin chuyển khoản / QR cho đơn — chỉ chủ đơn hoặc staff được xem.</summary>
    [HttpGet("{id:int}/payment-info")]
    public async Task<IActionResult> GetPaymentInfo(int id)
    {
        var order = await _orders.GetOrderAsync(id);
        if (order is null) return NotFound();
        if (!IsStaff && order.UserId != CurrentUserId) return NotFound();
        try { return Ok(await _orders.GetPaymentInfoAsync(id)); }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Báo giá phí vận chuyển theo phương thức nhận hàng.</summary>
    [HttpPost("shipping-quote")]
    public IActionResult ShippingQuote(ShippingQuoteRequest request) => Ok(_orders.GetShippingQuote(request));

    /// <summary>Khách hàng yêu cầu hoàn tiền cho đơn của mình.</summary>
    [HttpPost("{id:int}/request-refund")]
    public async Task<IActionResult> RequestRefund(int id, RequestRefundRequest request)
    {
        try
        {
            var refundId = await _orders.RequestRefundAsync(id, CurrentUserId, request);
            await AddAuditAsync(id, "RequestRefund", request.Reason);
            return Ok(new { id = refundId });
        }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpGet("{id:int}/allocation-suggestion")]
    public async Task<IActionResult> AllocationSuggestion(int id)
    {
        try { return Ok(new { items = await _orders.GetAllocationSuggestionAsync(id) }); }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPost("{id:int}/allocate")]
    public async Task<IActionResult> Allocate(int id, AllocateRequest request)
    {
        try
        {
            await _orders.AllocateAsync(id, request, UserIdOrNull);
            await AddAuditAsync(id, "Allocate", $"Lines={request.Allocations.Count}");
            return Ok(new { message = "Phân bổ thành công." });
        }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        try
        {
            await _orders.UpdateStatusAsync(id, request, UserIdOrNull);
            await AddAuditAsync(id, "UpdateStatus", $"{request.ToStatus};{request.Note}");
            return Ok(new { id });
        }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPut("{id:int}/fulfillment-status")]
    public async Task<IActionResult> UpdateFulfillmentStatus(int id, UpdateFulfillmentStatusRequest request)
    {
        try
        {
            await _orders.UpdateFulfillmentStatusAsync(id, request, UserIdOrNull);
            await AddAuditAsync(id, "UpdateFulfillmentStatus", $"{request.ToStatus};{request.Note}");
            return Ok(new { id });
        }
        catch (OrderException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

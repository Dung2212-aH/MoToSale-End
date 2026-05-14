using OrderService.DTOs.Orders;
using OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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
    [HttpPost("from-cart")]
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

    [HttpPost("{id:int}/cancel")]
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
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        try
        {
            return Ok(await _orderService.UpdateOrderStatusAsync(id, request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/shipping")]
    public async Task<IActionResult> UpdateShipping(int id, UpdateShippingStatusRequest request)
    {
        try
        {
            return Ok(await _orderService.UpdateShippingStatusAsync(id, request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }
}

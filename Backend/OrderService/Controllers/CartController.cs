using OrderService.DTOs.Cart;
using OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CartController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCart()
    {
        try
        {
            return Ok(await _orderService.GetMyCartAsync(this.GetCurrentUserId()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        try
        {
            var cart = await _orderService.GetMyCartAsync(this.GetCurrentUserId());
            return Ok(new { count = cart.TongSoLuong, totalItems = cart.TongSoLuong });
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request)
    {
        try
        {
            return Ok(await _orderService.AddCartItemAsync(this.GetCurrentUserId(), request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, UpdateCartItemRequest request)
    {
        try
        {
            return Ok(await _orderService.UpdateCartItemAsync(this.GetCurrentUserId(), id, request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> RemoveItem(int id)
    {
        try
        {
            return Ok(await _orderService.RemoveCartItemAsync(this.GetCurrentUserId(), id));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        try
        {
            return Ok(await _orderService.ClearCartAsync(this.GetCurrentUserId()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }
}

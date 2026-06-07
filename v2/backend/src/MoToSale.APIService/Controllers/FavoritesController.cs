using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Services.Catalog;

namespace MoToSale.APIService.Controllers;

/// <summary>Sản phẩm yêu thích / Wishlist của khách hàng đang đăng nhập.</summary>
[ApiController]
[Authorize]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public FavoritesController(ICatalogService catalog) => _catalog = catalog;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Danh sách sản phẩm yêu thích của người dùng hiện tại.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine() => Ok(await _catalog.GetFavoritesAsync(CurrentUserId));

    /// <summary>Thêm sản phẩm vào yêu thích (idempotent).</summary>
    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        try
        {
            await _catalog.AddFavoriteAsync(CurrentUserId, productId);
            return Ok(new { productId });
        }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Bỏ sản phẩm khỏi yêu thích (idempotent).</summary>
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        await _catalog.RemoveFavoriteAsync(CurrentUserId, productId);
        return Ok(new { productId });
    }
}

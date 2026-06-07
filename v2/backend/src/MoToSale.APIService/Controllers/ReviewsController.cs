using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common.Auth;
using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;
using MoToSale.Services.Catalog;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private const string StaffRoles = $"{RoleConstant.Admin},{RoleConstant.Staff}";

    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews) => _reviews = reviews;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = StaffRoles)]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PagingRequest request, [FromQuery] string? status) => Ok(await _reviews.SearchAsync(request, status));

    [Authorize(Roles = StaffRoles)]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateReviewStatusRequest request)
    {
        try { await _reviews.UpdateStatusAsync(id, request.Status); return Ok(new { id }); }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _reviews.DeleteAsync(id); return Ok(new { message = "Đã xóa." }); }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Khách hàng: trạng thái đánh giá của bản thân cho một sản phẩm (có thể đánh giá hay chưa, đánh giá hiện có).</summary>
    [Authorize]
    [HttpGet("product/{productId:int}/me")]
    public async Task<IActionResult> GetMyProductReview(int productId) =>
        Ok(await _reviews.GetMyReviewAsync(productId, CurrentUserId));
}

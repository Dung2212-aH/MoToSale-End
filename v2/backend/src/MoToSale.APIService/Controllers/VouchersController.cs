using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common.Auth;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Services.Ordering;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Route("api/vouchers")]
public class VouchersController : ControllerBase
{
    private readonly IVoucherService _vouchers;

    public VouchersController(IVoucherService vouchers) => _vouchers = vouchers;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Admin/Staff: tìm kiếm toàn bộ voucher (phân trang). Khách/ẩn danh: chỉ trả voucher công khai đang hiệu lực.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PagingRequest request)
    {
        if (User.IsInRole(RoleConstant.Admin) || User.IsInRole(RoleConstant.Staff))
            return Ok(await _vouchers.SearchAsync(request));
        return Ok(await _vouchers.GetPublicVouchersAsync(request));
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var v = await _vouchers.GetAsync(id);
        return v is null ? NotFound() : Ok(v);
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPost]
    public async Task<IActionResult> Create(SaveVoucherRequest request)
    {
        try { return Ok(new { id = await _vouchers.CreateAsync(request) }); }
        catch (VoucherException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveVoucherRequest request)
    {
        try { await _vouchers.UpdateAsync(id, request); return Ok(new { id }); }
        catch (VoucherException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _vouchers.DeleteAsync(id); return Ok(new { message = "Đã xóa voucher." }); }
        catch (VoucherException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Khách kiểm tra voucher khi đặt hàng.</summary>
    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateVoucherBody body) =>
        Ok(await _vouchers.ValidateAsync(body.Code, body.Subtotal));

    /// <summary>Khách lấy danh sách voucher có thể áp dụng cho đơn (kèm số tiền giảm tính sẵn).</summary>
    [Authorize]
    [HttpPost("applicable")]
    public async Task<IActionResult> Applicable([FromBody] ApplicableVouchersRequest body) =>
        Ok(await _vouchers.GetApplicableAsync(CurrentUserId, body.Subtotal, body.OrderType));

    /// <summary>Khách lưu (claim) một voucher công khai vào tài khoản.</summary>
    [Authorize]
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] SaveVoucherCodeRequest body)
    {
        try
        {
            await _vouchers.SaveForUserAsync(CurrentUserId, body.Code);
            return Ok(new { success = true, message = "Đã nhận voucher thành công." });
        }
        catch (VoucherException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    /// <summary>Danh sách voucher khách đã lưu.</summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> My() => Ok(await _vouchers.GetMyVouchersAsync(CurrentUserId));

    /// <summary>Số lượng voucher khách đã lưu (chưa dùng).</summary>
    [Authorize]
    [HttpGet("my/count")]
    public async Task<IActionResult> MyCount() => Ok(new { count = await _vouchers.CountMyVouchersAsync(CurrentUserId) });
}

public record ValidateVoucherBody(string Code, decimal Subtotal);

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common.Auth;
using MoToSale.DTO.Payments;
using MoToSale.Services.Payments;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Authorize] // Mặc định yêu cầu đăng nhập; các thao tác quản trị được giới hạn ở cấp method.
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private const string StaffRoles = $"{RoleConstant.Admin},{RoleConstant.Staff}";

    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments) => _payments = payments;

    private int? CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool IsStaff => User.IsInRole(RoleConstant.Admin) || User.IsInRole(RoleConstant.Staff);

    /// <summary>
    /// Role-aware: admin/staff → ghi nhận thanh toán thủ công (đã thu, xác nhận đơn);
    /// khách hàng → khởi tạo phiếu thanh toán cho đơn của mình ở trạng thái Pending (chờ admin xác nhận).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerPaymentRequest request)
    {
        if (CurrentUserId is null) return Unauthorized();
        try
        {
            if (IsStaff)
            {
                // Admin/staff ghi nhận thủ công: dùng request đầy đủ với mặc định hợp lý.
                var record = new CreatePaymentRequest(
                    request.OrderId,
                    string.IsNullOrWhiteSpace(request.PaymentType) ? MoToSale.Common.PaymentRecordType.Full : request.PaymentType,
                    request.Amount ?? 0,
                    string.IsNullOrWhiteSpace(request.Method) ? MoToSale.Common.PaymentMethod.Cash : request.Method,
                    request.TransactionRef, request.Note);
                return Ok(new { id = await _payments.RecordPaymentAsync(record, CurrentUserId) });
            }
            return Ok(new { id = await _payments.CreateCustomerPaymentAsync(request, CurrentUserId.Value) });
        }
        catch (PaymentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>Khách hàng báo "đã chuyển khoản" — phiếu chờ admin xác nhận thực thu.</summary>
    [HttpPost("{id:int}/confirm-success")]
    public async Task<IActionResult> ConfirmSuccess(int id, ConfirmPaymentSuccessRequest? request)
    {
        if (CurrentUserId is null) return Unauthorized();
        if (!IsStaff)
        {
            var ownerId = await _payments.GetPaymentOwnerAsync(id);
            if (ownerId is null || ownerId != CurrentUserId) return NotFound();
        }
        try { await _payments.ConfirmSuccessAsync(id, CurrentUserId); return Ok(new { message = "Đã ghi nhận báo chuyển khoản, chờ xác nhận." }); }
        catch (PaymentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        if (!IsStaff)
        {
            var ownerId = await _payments.GetOrderOwnerAsync(orderId);
            if (ownerId is null || ownerId != CurrentUserId) return NotFound();
        }
        return Ok(new { items = await _payments.GetByOrderAsync(orderId) });
    }

    [Authorize(Roles = StaffRoles)]
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] MoToSale.DTO.Common.PagingRequest request, [FromQuery] string? status) =>
        Ok(await _payments.SearchAsync(request, status));

    [Authorize(Roles = StaffRoles)]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelPaymentRequest request)
    {
        try { await _payments.CancelAsync(id, request.Reason); return Ok(new { message = "Đã hủy phiếu thanh toán." }); }
        catch (PaymentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

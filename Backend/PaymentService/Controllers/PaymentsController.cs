using PaymentService.DTOs.Payments;
using PaymentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] PaymentSearchDto search)
    {
        try
        {
            return Ok(await _paymentService.GetPaymentsAsync(search, this.GetCurrentUserId(), this.CanManagePayments()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetPaymentsByOrder(int orderId)
    {
        try
        {
            var search = new PaymentSearchDto { MaDonHang = orderId, Page = 1, PageSize = 100 };
            return Ok(await _paymentService.GetPaymentsAsync(search, this.GetCurrentUserId(), this.CanManagePayments()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        try
        {
            return Ok(await _paymentService.GetPaymentByIdAsync(id, this.GetCurrentUserId(), this.CanManagePayments()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpGet("orders/{orderId:int}/summary")]
    public async Task<IActionResult> GetOrderPaymentSummary(int orderId)
    {
        try
        {
            return Ok(await _paymentService.GetOrderPaymentSummaryAsync(orderId, this.GetCurrentUserId(), this.CanManagePayments()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(CreatePaymentRequest request)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(this.GetCurrentUserId(), this.CanManagePayments(), request);
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.MaThanhToan }, payment);
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPost("{id:int}/confirm-success")]
    public async Task<IActionResult> ConfirmSuccess(int id, ConfirmPaymentRequest request)
    {
        try
        {
            return Ok(await _paymentService.ConfirmPaymentSuccessAsync(id, this.GetCurrentUserId(), this.CanManagePayments(), request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, ConfirmPaymentRequest? request)
    {
        try
        {
            return Ok(await _paymentService.ConfirmPaymentSuccessAsync(id, this.GetCurrentUserId(), this.CanManagePayments(), request ?? new ConfirmPaymentRequest()));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/failed")]
    public async Task<IActionResult> MarkFailed(int id, FailPaymentRequest request)
    {
        try
        {
            return Ok(await _paymentService.MarkPaymentFailedAsync(id, this.GetCurrentUserId(), this.CanManagePayments(), request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancelPaymentRequest request)
    {
        try
        {
            return Ok(await _paymentService.CancelPaymentAsync(id, this.GetCurrentUserId(), this.CanManagePayments(), request));
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }
}

using PaymentService.DTOs.Common;
using PaymentService.DTOs.Payments;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PagedResultDto<PaymentDto>> GetPaymentsAsync(PaymentSearchDto search, int currentUserId, bool canManagePayments);
    Task<PaymentDto> GetPaymentByIdAsync(int maThanhToan, int currentUserId, bool canManagePayments);
    Task<PaymentOrderSummaryDto> GetOrderPaymentSummaryAsync(int maDonHang, int currentUserId, bool canManagePayments);
    Task<PaymentDto> CreatePaymentAsync(int currentUserId, bool canManagePayments, CreatePaymentRequest request);
    Task<PaymentOrderSummaryDto> ConfirmPaymentSuccessAsync(int maThanhToan, int currentUserId, bool canManagePayments, ConfirmPaymentRequest request);
    Task<PaymentDto> MarkPaymentFailedAsync(int maThanhToan, int currentUserId, bool canManagePayments, FailPaymentRequest request);
    Task<PaymentDto> CancelPaymentAsync(int maThanhToan, int currentUserId, bool canManagePayments, CancelPaymentRequest request);
    Task<PaymentOrderSummaryDto> RefundPaymentAsync(int maThanhToan, int currentUserId, bool canManagePayments, RefundPaymentRequest request);
}

namespace MoToSale.DTO.Payments;

public record CreatePaymentRequest(int OrderId, string PaymentType, decimal Amount, string Method, string? TransactionRef, string? Note);

public record PaymentDto(
    int Id, string Code, int OrderId, string PaymentType, decimal Amount, string Method,
    string Status, string? TransactionRef, DateTime? PaidAt);

public record PaymentListItem(
    int Id, string Code, int OrderId, string? OrderCode, string PaymentType, decimal Amount,
    string Method, string Status, DateTime? PaidAt, DateTime CreatedDate);

public record CancelPaymentRequest(string? Reason);

/// <summary>
/// Khách hàng khởi tạo phiếu thanh toán (trạng thái Pending — chờ admin xác nhận đã nhận tiền).
/// Số tiền có thể bỏ trống → mặc định lấy số tiền còn phải trả của đơn.
/// </summary>
public record CreateCustomerPaymentRequest(
    int OrderId, string? PaymentType, decimal? Amount, string? Method, string? TransactionRef, string? Note);

/// <summary>Khách hàng báo "đã chuyển khoản" cho một phiếu thanh toán đang chờ.</summary>
public record ConfirmPaymentSuccessRequest(string? TransactionRef);

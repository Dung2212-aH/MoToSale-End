using MoToSale.DTO.Payments;

namespace MoToSale.Services.Payments;

public interface IPaymentService
{
    Task<int> RecordPaymentAsync(CreatePaymentRequest request, int? userId);
    Task<List<PaymentDto>> GetByOrderAsync(int orderId);
    Task<MoToSale.DTO.Common.PagingResponse<PaymentListItem>> SearchAsync(MoToSale.DTO.Common.PagingRequest request, string? status);
    Task CancelAsync(int id, string? reason);

    // Customer-facing
    Task<int> CreateCustomerPaymentAsync(CreateCustomerPaymentRequest request, int userId);
    Task ConfirmSuccessAsync(int paymentId, int? userId);
    Task<int?> GetOrderOwnerAsync(int orderId);
    Task<int?> GetPaymentOwnerAsync(int paymentId);
}

public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}

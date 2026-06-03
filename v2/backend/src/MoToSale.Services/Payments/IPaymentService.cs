using MoToSale.DTO.Payments;

namespace MoToSale.Services.Payments;

public interface IPaymentService
{
    Task<int> RecordPaymentAsync(CreatePaymentRequest request, int? userId);
    Task<List<PaymentDto>> GetByOrderAsync(int orderId);
    Task<MoToSale.DTO.Common.PagingResponse<PaymentListItem>> SearchAsync(MoToSale.DTO.Common.PagingRequest request, string? status);
    Task CancelAsync(int id, string? reason);
}

public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}

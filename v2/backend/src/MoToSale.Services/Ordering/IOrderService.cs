using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;

namespace MoToSale.Services.Ordering;

public interface IOrderService
{
    // Cart
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemAsync(int userId, int itemId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(int userId, int itemId);
    Task<CartDto> ClearCartAsync(int userId);

    // Order
    Task<int> CheckoutAsync(int userId, CheckoutRequest request);
    Task<List<OrderListItem>> GetMyOrdersAsync(int userId);
    Task<OrderDetail?> GetOrderAsync(int id);
    Task<PagingResponse<OrderListItem>> SearchOrdersAsync(OrderSearchRequest request);

    // Allocation (admin phân phối)
    Task<List<AllocationSuggestionItem>> GetAllocationSuggestionAsync(int orderId);
    Task AllocateAsync(int orderId, AllocateRequest request, int? userId);

    // Thanh toán / vận chuyển / hoàn tiền (customer-facing)
    Task<PaymentInfoDto> GetPaymentInfoAsync(int orderId);
    ShippingQuoteResponse GetShippingQuote(ShippingQuoteRequest request);
    Task<int> RequestRefundAsync(int orderId, int userId, RequestRefundRequest request);

    // Status
    Task CancelOrderAsync(int orderId, string? reason, int? userId);
    Task UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request, int? userId);
    Task UpdateFulfillmentStatusAsync(int orderId, UpdateFulfillmentStatusRequest request, int? userId);
}

public class OrderException : Exception
{
    public OrderException(string message) : base(message) { }
}

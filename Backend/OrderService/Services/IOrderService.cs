using OrderService.DTOs.Cart;
using OrderService.DTOs.Common;
using OrderService.DTOs.Orders;

namespace OrderService.Services;

public interface IOrderService
{
    Task<CartDto> GetMyCartAsync(int maNguoiDung);
    Task<CartDto> AddCartItemAsync(int maNguoiDung, AddCartItemRequest request);
    Task<CartDto> UpdateCartItemAsync(int maNguoiDung, int maChiTietGioHang, UpdateCartItemRequest request);
    Task<CartDto> RemoveCartItemAsync(int maNguoiDung, int maChiTietGioHang);
    Task<CartDto> ClearCartAsync(int maNguoiDung);
    Task<ShippingQuoteResponse> GetShippingQuoteAsync(int maNguoiDung, ShippingQuoteRequest request);
    Task<OrderDto> CreateOrderFromCartAsync(int maNguoiDung, CreateOrderFromCartRequest request);
    Task<PagedResultDto<OrderSummaryDto>> GetOrdersAsync(OrderSearchDto search, int currentUserId, bool canViewAll);
    Task<OrderDto> GetOrderByIdAsync(int maDonHang, int currentUserId, bool canViewAll);
    Task<OrderDto> CancelOrderAsync(int maDonHang, int currentUserId, bool canManageAll, CancelOrderRequest request);
    Task<PaymentInfoDto> GetPaymentInfoAsync(int maDonHang, int currentUserId, bool canViewAll);
    Task<OrderDto> ConfirmOrderPaymentAsync(int maDonHang, ConfirmOrderPaymentRequest request);
    Task<OrderDto> RequestRefundAsync(int maDonHang, int currentUserId, CreateRefundRequestDto request);
    Task<OrderDto> ConfirmRefundAsync(int maDonHang, int maYeuCauHoanTien, ConfirmRefundRequest request);
}

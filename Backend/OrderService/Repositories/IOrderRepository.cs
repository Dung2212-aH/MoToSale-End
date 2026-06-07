using System.Data;
using OrderService.DTOs.Orders;
using OrderService.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task<User?> GetUserAsync(int maNguoiDung);
    Task<UserAddress?> GetUserAddressAsync(int maNguoiDung, int maDiaChi);
    Task<Product?> GetProductAsync(int maSanPham);
    Task<ProductVariant?> GetVariantAsync(int maBienSanPham);
    Task<bool> ProductHasVariantsAsync(int maSanPham);
    Task<Cart?> GetActiveCartByUserIdAsync(int maNguoiDung);
    Task<CartItem?> GetCartItemForUserAsync(int maNguoiDung, int maChiTietGioHang);
    Task AddCartAsync(Cart cart);
    Task AddCartItemAsync(CartItem cartItem);
    void RemoveCartItem(CartItem cartItem);
    void RemoveCartItems(IEnumerable<CartItem> cartItems);
    Task<int> GetAvailableStockAsync(int maSanPham, int? maBienSanPham);
    Task CleanupExpiredInventoryHoldsAsync();
    Task AddOrderAsync(Order order);
    Task<Order?> GetOrderByIdAsync(int maDonHang);
    Task<List<Order>> GetOrdersAsync(OrderSearchDto search, int? maNguoiDung, bool hideAwaitingPayment = false);
    Task<int> CountOrdersAsync(OrderSearchDto search, int? maNguoiDung, bool hideAwaitingPayment = false);
    Task<VoucherValidationResult?> ValidateVoucherAsync(int maNguoiDung, int maGioHang, string maVoucherCode, decimal phiVanChuyen);
    Task<bool> UserHasSavedVoucherAsync(int maNguoiDung, string maVoucherCode);
    Task RecordVoucherUseAsync(int maNguoiDung, int maDonHang, string maVoucherCode, decimal soTienGiam);
    Task CancelVoucherUseAsync(int maDonHang);
    Task SaveChangesAsync();
}

using System.Data;
using PaymentService.DTOs.Payments;
using PaymentService.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task<User?> GetUserAsync(int maNguoiDung);
    Task<Order?> GetOrderByIdAsync(int maDonHang);
    Task<Payment?> GetPaymentByIdAsync(int maThanhToan);
    Task<List<Payment>> GetPaymentsAsync(PaymentSearchDto search, int? maNguoiDung);
    Task<int> CountPaymentsAsync(PaymentSearchDto search, int? maNguoiDung);
    Task<Product?> GetProductAsync(int maSanPham);
    Task<ProductVariant?> GetVariantAsync(int maBienSanPham);
    Task AddPaymentAsync(Payment payment);
    Task AddRefundAsync(PaymentRefund refund);
    Task CleanupExpiredInventoryHoldsAsync();
    Task SaveChangesAsync();
}

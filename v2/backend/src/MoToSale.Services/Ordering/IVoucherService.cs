using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;

namespace MoToSale.Services.Ordering;

public interface IVoucherService
{
    Task<PagingResponse<VoucherDto>> SearchAsync(PagingRequest request);
    Task<VoucherDto?> GetAsync(int id);
    Task<int> CreateAsync(SaveVoucherRequest request);
    Task UpdateAsync(int id, SaveVoucherRequest request);
    Task DeleteAsync(int id);
    Task<VoucherValidationResult> ValidateAsync(string code, decimal subtotal);

    // ===== Khách hàng =====
    /// <summary>Danh sách voucher công khai đang hiệu lực (cho khách xem).</summary>
    Task<PagingResponse<VoucherDto>> GetPublicVouchersAsync(PagingRequest request);
    /// <summary>Voucher khách có thể áp dụng cho đơn (đã lưu, hợp lệ, đủ điều kiện) kèm số tiền giảm.</summary>
    Task<IReadOnlyList<ApplicableVoucherDto>> GetApplicableAsync(int userId, decimal subtotal, string? orderType);
    /// <summary>Khách lưu (claim) một voucher công khai theo mã. Idempotent nếu đã lưu.</summary>
    Task SaveForUserAsync(int userId, string code);
    /// <summary>Danh sách voucher khách đã lưu.</summary>
    Task<IReadOnlyList<UserVoucherDto>> GetMyVouchersAsync(int userId);
    /// <summary>Số lượng voucher khách đã lưu nhưng chưa dùng.</summary>
    Task<int> CountMyVouchersAsync(int userId);
    /// <summary>Tra cứu voucher theo mã (dùng cho tra cứu công khai).</summary>
    Task<VoucherDto?> GetByCodeAsync(string code);
}

public class VoucherException : Exception
{
    public VoucherException(string message) : base(message) { }
}

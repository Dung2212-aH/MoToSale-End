namespace MoToSale.DTO.Ordering;

// ===== Voucher khách hàng (save / my-list / applicable) =====

/// <summary>Body khi khách lấy danh sách voucher có thể áp dụng cho đơn hàng.</summary>
public record ApplicableVouchersRequest(decimal Subtotal, string? OrderType);

/// <summary>Body khi khách lưu (claim) một voucher công khai theo mã.</summary>
public record SaveVoucherCodeRequest(string Code);

/// <summary>Voucher có thể áp dụng kèm số tiền giảm đã tính sẵn cho đơn hiện tại.</summary>
public record ApplicableVoucherDto(VoucherDto Voucher, decimal DiscountAmount);

/// <summary>Voucher người dùng đã lưu, kèm chi tiết voucher và trạng thái lưu.</summary>
public record UserVoucherDto(
    int Id,
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountValue,
    decimal MinOrderValue,
    DateTime? StartAt,
    DateTime? EndAt,
    string? Scope,
    string VoucherStatus,
    DateTime SavedAt,
    DateTime? UsedAt);

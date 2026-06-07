using MoToSale.Common;

namespace MoToSale.Entities.Ordering;

/// <summary>Voucher người dùng đã claim (save) hoặc đã dùng. (Phân biệt VoucherRedemption ở chỗ Saved chưa dùng.)</summary>
public class UserVoucher : BaseEntity
{
    public int UserId { get; set; }
    public int VoucherId { get; set; }
    public string VoucherStatus { get; set; } = UserVoucherStatus.Saved; // Saved | Used | Expired
    public DateTime SavedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? OrderId { get; set; }                  // Đơn đã dùng (nếu Used)
    public decimal? DiscountAmount { get; set; }       // Số tiền giảm thực tế khi áp vào đơn

    public Voucher Voucher { get; set; } = null!;
}

public static class UserVoucherStatus
{
    public const string Saved = "Saved";
    public const string Used = "Used";
    public const string Expired = "Expired";
}

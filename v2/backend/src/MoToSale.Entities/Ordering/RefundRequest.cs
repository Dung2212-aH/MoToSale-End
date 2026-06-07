using MoToSale.Common;

namespace MoToSale.Entities.Ordering;

/// <summary>Yêu cầu hủy &amp; hoàn tiền do khách hàng khởi tạo.</summary>
public class RefundRequest : BaseEntity
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }                       // SoTien — usually = order's paid amount
    public string BankName { get; set; } = string.Empty;      // TenNganHang
    public string AccountNumber { get; set; } = string.Empty; // SoTaiKhoan
    public string AccountHolder { get; set; } = string.Empty; // ChuTaiKhoan
    public string? Reason { get; set; }                       // LyDo
    public string RefundStatus { get; set; } = RefundRequestStatus.Pending; // Pending | Completed | Rejected
    public DateTime? CompletedAt { get; set; }                // NgayHoanTat
    public string? AdminNote { get; set; }                    // GhiChuAdmin
    public string? RefundTransactionRef { get; set; }         // MaGiaoDichHoan
    public int? HandledByUserId { get; set; }                 // Admin/Staff người duyệt

    public Order? Order { get; set; }
}

public static class RefundRequestStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
}

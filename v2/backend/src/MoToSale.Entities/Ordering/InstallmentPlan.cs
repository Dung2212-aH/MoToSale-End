using MoToSale.Common;

namespace MoToSale.Entities.Ordering;

/// <summary>Hồ sơ trả góp — gắn 1-1 với Order khi OrderType = Installment.</summary>
public class InstallmentPlan : BaseEntity
{
    public int OrderId { get; set; }

    // --- Financial summary ---
    public decimal DownPayment { get; set; }       // TienTraTruoc
    public decimal Principal { get; set; }         // SoTienGoc (financed amount = GrandTotal - DownPayment)
    public int TermCount { get; set; }             // SoKy (6 / 9 / 12 months)
    public decimal AnnualRate { get; set; }        // LaiSuatNam (e.g. 12 = 12%)
    public decimal TotalInterest { get; set; }     // TongTienLai
    public decimal TotalDue { get; set; }          // TongPhaiTra (Principal + TotalInterest)
    public string PlanStatus { get; set; } = InstallmentStatus.Active; // Active | Completed | Defaulted | Cancelled

    // --- Borrower personal info ---
    public string BorrowerFullName { get; set; } = string.Empty;   // HoTenNguoiVay
    public string NationalId { get; set; } = string.Empty;          // SoCCCD
    public DateTime? NationalIdIssuedDate { get; set; }             // NgayCapCCCD
    public string? NationalIdIssuedPlace { get; set; }              // NoiCapCCCD
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }                        // SoDienThoai
    public string? PermanentAddress { get; set; }                   // DiaChiThuongTru

    // --- Employment & income ---
    public string? Occupation { get; set; }                         // NgheNghiep
    public string? CompanyName { get; set; }                        // TenCongTy
    public int? EmploymentMonths { get; set; }                      // ThoiGianLamViecThang
    public decimal? MonthlyIncome { get; set; }                     // ThuNhapHangThang

    public Order? Order { get; set; }
    public ICollection<InstallmentTerm> Terms { get; set; } = new List<InstallmentTerm>();
}

/// <summary>Kỳ trả góp — N records per InstallmentPlan, one per month.</summary>
public class InstallmentTerm : BaseEntity
{
    public int PlanId { get; set; }
    public int TermNo { get; set; }                   // KyThu (1..N)
    public DateTime DueDate { get; set; }              // NgayDenHan
    public decimal PrincipalAmount { get; set; }       // SoTienGoc
    public decimal InterestAmount { get; set; }        // SoTienLai
    public decimal TotalAmount { get; set; }           // TongTien (Principal + Interest)
    public string TermStatus { get; set; } = InstallmentTermStatus.Pending; // Pending | Paid | Cancelled
    public DateTime? PaidAt { get; set; }              // NgayThanhToan
    public int? PaymentId { get; set; }                // FK to Payment khi đã trả

    public InstallmentPlan Plan { get; set; } = null!;
}

public static class InstallmentStatus
{
    public const string Active = "Active";
    public const string Completed = "Completed";
    public const string Defaulted = "Defaulted";
    public const string Cancelled = "Cancelled";
}

public static class InstallmentTermStatus
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
}

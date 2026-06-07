using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class RefundRequestDto
{
    public int MaYeuCauHoanTien { get; set; }
    public int MaDonHang { get; set; }
    public decimal SoTien { get; set; }
    public string TenNganHang { get; set; } = string.Empty;
    public string SoTaiKhoan { get; set; } = string.Empty;
    public string ChuTaiKhoan { get; set; } = string.Empty;
    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public string? GhiChuAdmin { get; set; }
    public string? MaGiaoDichHoan { get; set; }
}

public class CreateRefundRequestDto
{
    [Required]
    [MaxLength(100)]
    public string TenNganHang { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{6,20}$", ErrorMessage = "So tai khoan khong hop le.")]
    public string SoTaiKhoan { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ChuTaiKhoan { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LyDo { get; set; }
}

public class ConfirmRefundRequest
{
    [MaxLength(120)]
    public string? MaGiaoDichHoan { get; set; }

    [MaxLength(500)]
    public string? GhiChuAdmin { get; set; }
}

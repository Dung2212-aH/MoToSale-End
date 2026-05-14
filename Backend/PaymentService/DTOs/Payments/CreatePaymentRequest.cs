using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs.Payments;

public class CreatePaymentRequest
{
    [Range(1, int.MaxValue)]
    public int MaDonHang { get; set; }

    [Required]
    [MaxLength(30)]
    public string LoaiThanhToan { get; set; } = "Full";

    [Range(0.01, 999999999999.99)]
    public decimal SoTien { get; set; }

    [Required]
    [MaxLength(30)]
    public string PhuongThuc { get; set; } = "BankTransfer";

    [MaxLength(120)]
    public string? MaGiaoDich { get; set; }

    [MaxLength(500)]
    public string? NoiDungChuyenKhoan { get; set; }

    [MaxLength(50)]
    public string? MaNganHang { get; set; }

    public string? ResponseRaw { get; set; }
}

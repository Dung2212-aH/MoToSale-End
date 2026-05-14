using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs.Payments;

public class RefundPaymentRequest
{
    [Range(0.01, 999999999999.99)]
    public decimal SoTienHoan { get; set; }

    [MaxLength(120)]
    public string? MaGiaoDichHoanTien { get; set; }

    [MaxLength(500)]
    public string? LyDo { get; set; }

    public string? ResponseRaw { get; set; }
}

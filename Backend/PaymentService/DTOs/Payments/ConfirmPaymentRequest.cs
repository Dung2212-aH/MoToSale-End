using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs.Payments;

public class ConfirmPaymentRequest
{
    [MaxLength(120)]
    public string? MaGiaoDich { get; set; }

    public string? ResponseRaw { get; set; }
}

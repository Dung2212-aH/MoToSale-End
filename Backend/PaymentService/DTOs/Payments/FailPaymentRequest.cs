using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs.Payments;

public class FailPaymentRequest
{
    [MaxLength(500)]
    public string? LyDo { get; set; }

    public string? ResponseRaw { get; set; }
}

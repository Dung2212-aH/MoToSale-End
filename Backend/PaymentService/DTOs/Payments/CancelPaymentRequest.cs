using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs.Payments;

public class CancelPaymentRequest
{
    [MaxLength(500)]
    public string? LyDoHuy { get; set; }
}

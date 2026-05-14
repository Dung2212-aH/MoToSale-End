using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class CancelOrderRequest
{
    [MaxLength(500)]
    public string? LyDoHuyDon { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class ShippingQuoteRequest
{
    [Required]
    [MaxLength(30)]
    public string PhuongThucNhanHang { get; set; } = "Delivery";

    [MaxLength(100)]
    public string? ShippingProvince { get; set; }

    [MaxLength(50)]
    public string? MaVoucherCode { get; set; }

    [MaxLength(20)]
    public string? OrderType { get; set; }
}

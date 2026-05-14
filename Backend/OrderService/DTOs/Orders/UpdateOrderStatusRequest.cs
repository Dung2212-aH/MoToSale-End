using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    [MaxLength(20)]
    public string TrangThaiDonHang { get; set; } = string.Empty;
}

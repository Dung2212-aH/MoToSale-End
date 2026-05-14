using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class UpdateShippingStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string TrangThaiVanChuyen { get; set; } = string.Empty;

    public DateTime? NgayHenNhanXe { get; set; }

    [MaxLength(500)]
    public string? GhiChuGiaoNhan { get; set; }
}

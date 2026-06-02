using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class CreateOrderFromCartRequest
{
    [Range(1, int.MaxValue)]
    public int? MaShowroom { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaDiaChiNhanHang { get; set; }

    [Required]
    [MaxLength(150)]
    public string HoTenNhanHang { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9+]{9,15}$")]
    public string SoDienThoaiNhanHang { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? EmailNhanHang { get; set; }

    [Required]
    [MaxLength(255)]
    public string DiaChiNhanHang { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ShippingProvince { get; set; }

    [Range(0, 999999999)]
    public decimal PhiVanChuyen { get; set; }

    [MaxLength(50)]
    public string? MaVoucherCode { get; set; }

    [MaxLength(1000)]
    public string? GhiChu { get; set; }

    [Required]
    [MaxLength(30)]
    public string PhuongThucNhanHang { get; set; } = "Delivery";

    [Required]
    [MaxLength(20)]
    public string LoaiDonHang { get; set; } = "FullPayment";

    [Range(0, 999999999)]
    public decimal TienDatCoc { get; set; }

    public DateTime? NgayHenNhanXe { get; set; }

    [MaxLength(500)]
    public string? GhiChuGiaoNhan { get; set; }

    [Range(1, 240)]
    public int SoPhutGiuCho { get; set; } = 15;
}

using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductVariants;

public class ProductVariantUpdateDto
{
    [Required]
    [MaxLength(180)]
    public string TenBienThe { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? GiaGoc { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? GiaKhuyenMai { get; set; }

    [Range(0, int.MaxValue)]
    public int? SoLuongTon { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PhienBan { get; set; }

    [MaxLength(80)]
    public string? MauSac { get; set; }
}

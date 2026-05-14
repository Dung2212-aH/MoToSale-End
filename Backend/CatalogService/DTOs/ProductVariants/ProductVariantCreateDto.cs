using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductVariants;

public class ProductVariantCreateDto
{
    public int MaSanPham { get; set; }

    [Required]
    [MaxLength(180)]
    public string TenBienThe { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string SKU { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? GiaGhiDe { get; set; }

    [Range(0, int.MaxValue)]
    public int? SoLuongTon { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "Active";

    [MaxLength(100)]
    public string? PhienBan { get; set; }

    [MaxLength(80)]
    public string? MauSac { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Brands;

public class BrandUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string TenHang { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public bool DangHoatDong { get; set; }
}

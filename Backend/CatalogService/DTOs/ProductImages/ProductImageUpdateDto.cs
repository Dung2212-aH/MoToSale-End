using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductImages;

public class ProductImageUpdateDto
{
    public int? MaBienSanPham { get; set; }

    [Required]
    [MaxLength(500)]
    public string UrlAnh { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AltText { get; set; }

    public bool LaAnhChinh { get; set; }
    public int ThuTuHienThi { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CatalogService.DTOs.ProductImages;

public class ProductImageCreateDto
{
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }

    [Required]
    public IFormFile Image { get; set; } = default!;

    [MaxLength(255)]
    public string? AltText { get; set; }

    public bool LaAnhChinh { get; set; }
    public int ThuTuHienThi { get; set; }
}

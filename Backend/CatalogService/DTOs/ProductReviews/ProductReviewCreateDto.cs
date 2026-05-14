using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductReviews;

public class ProductReviewCreateDto
{
    public int MaSanPham { get; set; }
    public int? MaDonHang { get; set; }

    [Range(1, 5)]
    public byte Diem { get; set; }

    [MaxLength(255)]
    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public Microsoft.AspNetCore.Http.IFormFile? Image { get; set; }
}

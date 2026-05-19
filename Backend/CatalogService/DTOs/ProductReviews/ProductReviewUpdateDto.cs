using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductReviews;

public class ProductReviewUpdateDto
{
    [Range(1, 5)]
    public byte Diem { get; set; }

    [MaxLength(255)]
    public string? TieuDe { get; set; }

    [Required]
    public string? NoiDung { get; set; }

    public Microsoft.AspNetCore.Http.IFormFile? Image { get; set; }
}

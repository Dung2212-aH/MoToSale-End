using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.ProductReviews;

public class ProductReviewStatusUpdateDto
{
    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = string.Empty;
}

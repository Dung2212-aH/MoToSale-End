using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Showrooms;

public class ShowroomCreateDto
{
    [Required]
    [MaxLength(180)]
    public string TenShowroom { get; set; } = string.Empty;

    [Required]
    [MaxLength(220)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DiaChi { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? GioMoCua { get; set; }

    public bool DangHoatDong { get; set; } = true;
}

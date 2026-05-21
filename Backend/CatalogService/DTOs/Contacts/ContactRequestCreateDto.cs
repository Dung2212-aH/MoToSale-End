using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Contacts;

public class ContactRequestCreateDto
{
    [Required]
    [MaxLength(150)]
    public string HoTen { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? TieuDe { get; set; }

    [Required]
    public string NoiDung { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string LoaiYeuCau { get; set; } = string.Empty;

    public int? MaSanPham { get; set; }
}

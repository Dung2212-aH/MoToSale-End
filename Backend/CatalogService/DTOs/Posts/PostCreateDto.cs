using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Posts;

public class PostCreateDto
{
    [Required]
    [MaxLength(255)]
    public string TieuDe { get; set; } = string.Empty;

    [Required]
    [MaxLength(280)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TomTat { get; set; }

    [Required]
    public string NoiDung { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AnhDaiDienUrl { get; set; }

    [MaxLength(100)]
    public string? DanhMuc { get; set; }

    public int? MaTacGia { get; set; }
    public DateTime? XuatBanLuc { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "Draft";
}

using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Categories;

public class CategoryUpdateDto
{
    public int? MaDanhMucCha { get; set; }

    [Required]
    [MaxLength(150)]
    public string TenDanhMuc { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTa { get; set; }

    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
}

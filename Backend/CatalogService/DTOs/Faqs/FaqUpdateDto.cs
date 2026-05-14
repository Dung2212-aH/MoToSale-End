using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Faqs;

public class FaqUpdateDto
{
    [Required]
    [MaxLength(500)]
    public string CauHoi { get; set; } = string.Empty;

    [Required]
    public string CauTraLoi { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DanhMuc { get; set; }

    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
}

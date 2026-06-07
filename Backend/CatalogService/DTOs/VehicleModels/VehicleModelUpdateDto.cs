using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.VehicleModels;

public class VehicleModelUpdateDto
{
    public int MaHangXe { get; set; }

    [Required]
    [MaxLength(120)]
    public string TenDongXe { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Slug { get; set; } = string.Empty;

    // XeSo / TayGa / ConTay / XeDien / Khac (rỗng -> Khac)
    [MaxLength(20)]
    public string? LoaiXe { get; set; }

    public bool DangHoatDong { get; set; }
}

namespace CatalogService.DTOs.VehicleModels;

public class VehicleModelDto
{
    public int MaDongXe { get; set; }
    public int MaHangXe { get; set; }
    public string TenDongXe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LoaiXe { get; set; } = "Khac";
    public bool DangHoatDong { get; set; }
}

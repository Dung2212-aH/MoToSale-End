namespace CatalogService.DTOs.PartCompatibilities;

public class PartCompatibilityDto
{
    public int MaTuongThich { get; set; }
    public int MaPhuTung { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public short? NamTu { get; set; }
    public short? NamDen { get; set; }
    public bool ApDungTatCaXe { get; set; }
    public string? GhiChu { get; set; }
    public bool DangHoatDong { get; set; }
}

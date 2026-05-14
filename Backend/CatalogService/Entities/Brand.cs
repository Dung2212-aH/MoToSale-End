namespace CatalogService.Entities;
//HANGXE
public class Brand
{
    public int MaHangXe { get; set; }
    public string TenHang { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

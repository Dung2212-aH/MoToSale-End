namespace CatalogService.Entities;
//BANNER_TRANGCHU
public class BannerTrangChu
{
    public int MaBanner { get; set; }
    public string ViTri { get; set; } = "Slider";
    public string? TieuDe { get; set; }
    public string UrlAnh { get; set; } = string.Empty;
    public string? LienKet { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

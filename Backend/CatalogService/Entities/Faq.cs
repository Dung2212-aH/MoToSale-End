namespace CatalogService.Entities;

public class Faq
{
    public int MaFAQ { get; set; }
    public string CauHoi { get; set; } = string.Empty;
    public string CauTraLoi { get; set; } = string.Empty;
    public string? DanhMuc { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

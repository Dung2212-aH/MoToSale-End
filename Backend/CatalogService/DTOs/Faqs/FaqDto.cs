namespace CatalogService.DTOs.Faqs;

public class FaqDto
{
    public int MaFAQ { get; set; }
    public string CauHoi { get; set; } = string.Empty;
    public string CauTraLoi { get; set; } = string.Empty;
    public string? DanhMuc { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
}

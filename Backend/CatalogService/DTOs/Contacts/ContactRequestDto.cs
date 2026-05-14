namespace CatalogService.DTOs.Contacts;

public class ContactRequestDto
{
    public int MaLienHe { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TieuDe { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string LoaiYeuCau { get; set; } = string.Empty;
    public int? MaSanPham { get; set; }
    public int? MaShowroom { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? DaXuLyLuc { get; set; }
}

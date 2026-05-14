namespace CatalogService.Entities;
//DANHGIASANPHAM
public class ProductReview
{
    public int MaDanhGia { get; set; }
    public int MaSanPham { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaDonHang { get; set; }
    public byte Diem { get; set; }
    public string? TieuDe { get; set; }
    public string? NoiDung { get; set; }
    public string? HinhAnhUrl { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}

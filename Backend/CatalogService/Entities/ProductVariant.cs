namespace CatalogService.Entities;
//BIENSANPHAM
public class ProductVariant
{
    public int MaBienSanPham { get; set; }
    public int MaSanPham { get; set; }
    public string TenBienThe { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public int? SoLuongTon { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? PhienBan { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public string? MauSac { get; set; }
}

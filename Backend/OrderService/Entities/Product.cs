namespace OrderService.Entities;

public class Product
{
    public int MaSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string? MoTaNgan { get; set; }
    public string? MoTa { get; set; }
    public string? AnhChinhUrl { get; set; }
    public bool DangHoatDong { get; set; }
    public string TrangThaiSanPham { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

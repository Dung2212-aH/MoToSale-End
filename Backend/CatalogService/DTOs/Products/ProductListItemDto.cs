namespace CatalogService.DTOs.Products;

public class ProductListItemDto
{
    public int MaSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string LoaiSanPham { get; set; } = string.Empty;
    public decimal GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public decimal GiaBan { get; set; }
    public int? TyLeGiam { get; set; }
    public int SoLuongTon { get; set; }
    public string? AnhChinhUrl { get; set; }
    public string TrangThaiSanPham { get; set; } = string.Empty;
    public bool NoiBat { get; set; }
    public bool HotDeal { get; set; }
}

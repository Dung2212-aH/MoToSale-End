namespace CatalogService.DTOs.ProductVariants;

public class ProductVariantDto
{
    public int MaBienSanPham { get; set; }
    public int MaSanPham { get; set; }
    public string TenBienThe { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public decimal GiaBan { get; set; }
    public decimal? TyLeGiam { get; set; }
    public int? SoLuongTon { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? PhienBan { get; set; }
    public string? MauSac { get; set; }
}

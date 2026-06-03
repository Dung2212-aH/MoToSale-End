using CatalogService.DTOs.ProductImages;
using CatalogService.DTOs.ProductVariants;

namespace CatalogService.DTOs.Products;

public class ProductDetailDto
{
    public int MaSanPham { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string? MoTaNgan { get; set; }
    public string? MoTa { get; set; }
    public decimal GiaGoc { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public decimal GiaBan { get; set; }
    public int? TyLeGiam { get; set; }
    public int SoLuongTon { get; set; }
    public string? AnhChinhUrl { get; set; }
    public bool DangHoatDong { get; set; }
    public bool NoiBat { get; set; }
    public bool HotDeal { get; set; }
    public double DiemTrungBinh { get; set; }
    public int TongDanhGia { get; set; }
    public List<ProductVariantDto> BienThe { get; set; } = new();
    public List<ProductImageDto> Anh { get; set; } = new();
}

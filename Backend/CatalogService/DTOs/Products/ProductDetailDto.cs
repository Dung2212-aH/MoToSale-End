using CatalogService.DTOs.ProductImages;
using CatalogService.DTOs.ProductVariants;

namespace CatalogService.DTOs.Products;

public class ProductDetailDto
{
    public int MaSanPham { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MaDanhMuc { get; set; }
    public string? TenDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public string? TenHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string? MoTaNgan { get; set; }
    public string? MoTa { get; set; }
    // Giá tổng hợp từ các biến thể (giá thật hiển thị theo biến thể đang chọn ở BienThe[]).
    public decimal GiaThapNhat { get; set; }
    public decimal GiaCaoNhat { get; set; }
    public decimal GiaBan { get; set; }           // = GiaThapNhat (giá khởi tạo trước khi chọn biến thể)
    public decimal? TyLeGiam { get; set; }
    public int TongTon { get; set; }
    public bool DangHoatDong { get; set; }
    public double DiemTrungBinh { get; set; }
    public int TongDanhGia { get; set; }
    public List<ProductVariantDto> BienThe { get; set; } = new();
    public List<ProductImageDto> Anh { get; set; } = new();
}

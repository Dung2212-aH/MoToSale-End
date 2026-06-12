namespace CatalogService.DTOs.Products;

public class ProductListItemDto
{
    public int MaSanPham { get; set; }
    public string MaSanPhamKinhDoanh { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int MaDanhMuc { get; set; }
    public string? TenDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public string? TenHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public string LoaiSanPham { get; set; } = string.Empty;
    // Giá tổng hợp từ các biến thể đang bán (giá thật nằm ở BIENSANPHAM).
    public decimal GiaThapNhat { get; set; }      // min(GiaBan) -> dùng cho thẻ "Từ {giá}"
    public decimal GiaGocThapNhat { get; set; }   // GiaGoc của biến thể rẻ nhất -> gạch ngang
    public decimal GiaBan { get; set; }           // = GiaThapNhat (giữ tên cũ cho FE)
    public decimal? TyLeGiam { get; set; }        // tỷ lệ giảm của biến thể rẻ nhất
    public int TongTon { get; set; }              // tổng tồn các biến thể
    public int SoBienThe { get; set; }
    public string TrangThaiSanPham { get; set; } = string.Empty;
    public string? AnhChinhUrl { get; set; }
    public double DiemTrungBinh { get; set; }
    public int TongDanhGia { get; set; }
}

namespace CatalogService.DTOs.ProductReviews;

public class ProductReviewMeDto
{
    public int MaSanPham { get; set; }
    public bool DaDangNhap { get; set; }
    public bool DaMua { get; set; }
    public bool CoTheDanhGia { get; set; }
    public int? MaDonHangDuDieuKien { get; set; }
    public string? LyDo { get; set; }
    public ProductReviewDto? DanhGiaCuaToi { get; set; }
}

namespace CatalogService.DTOs.ProductImages;

public class ProductImageDto
{
    public int MaAnhSanPham { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string UrlAnh { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool LaAnhChinh { get; set; }
    public int ThuTuHienThi { get; set; }
}

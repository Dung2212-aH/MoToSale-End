namespace CatalogService.Entities;
//ANHSANPHAM
public class ProductImage
{
    public int MaAnhSanPham { get; set; }
    public int MaSanPham { get; set; }
    public string UrlAnh { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool LaAnhChinh { get; set; }
    public int ThuTuHienThi { get; set; }
    public DateTime NgayTao { get; set; }
    public int? MaBienSanPham { get; set; }
}

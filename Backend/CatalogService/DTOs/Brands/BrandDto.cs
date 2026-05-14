namespace CatalogService.DTOs.Brands;

public class BrandDto
{
    public int MaHangXe { get; set; }
    public string TenHang { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool DangHoatDong { get; set; }
}

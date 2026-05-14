namespace CatalogService.DTOs.Showrooms;

public class ShowroomDto
{
    public int MaShowroom { get; set; }
    public string TenShowroom { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? GioMoCua { get; set; }
    public bool DangHoatDong { get; set; }
}

namespace CatalogService.DTOs.Posts;

public class PostListItemDto
{
    public int MaBaiViet { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string? AnhDaiDienUrl { get; set; }
    public string? DanhMuc { get; set; }
    public DateTime? XuatBanLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

namespace CatalogService.Entities;
//BAIVIET
public class Post
{
    public int MaBaiViet { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? AnhDaiDienUrl { get; set; }
    public string? DanhMuc { get; set; }
    public int? MaTacGia { get; set; }
    public DateTime? XuatBanLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

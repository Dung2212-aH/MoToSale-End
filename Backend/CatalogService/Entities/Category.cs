namespace CatalogService.Entities;
//DANHMUC
public class Category
{
    public int MaDanhMuc { get; set; }
    public int? MaDanhMucCha { get; set; }
    public string TenDanhMuc { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string? AnhDaiDienUrl { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

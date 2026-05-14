namespace CatalogService.DTOs.Products;

public class ProductSearchDto
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public string? Keyword { get; set; }
    public int? MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public int? MaDongXeTuongThich { get; set; }
    public int? MaShowroom { get; set; }
    public string? LoaiSanPham { get; set; }
    public string? TrangThaiSanPham { get; set; }
    public decimal? GiaTu { get; set; }
    public decimal? GiaDen { get; set; }
    public bool? DangHoatDong { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;

    public static int NormalizePageSize(int pageSize)
    {
        return pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
    }
}

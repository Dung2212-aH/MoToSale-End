namespace OrderService.DTOs.Orders;

public class OrderSearchDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MaNguoiDung { get; set; }
    public string? TrangThaiDonHang { get; set; }
    public string? TrangThaiThanhToan { get; set; }
    public string? TrangThaiVanChuyen { get; set; }
    public string? Keyword { get; set; }
    public string? Search
    {
        get => Keyword;
        set => Keyword = value;
    }
    public string? Status
    {
        get => TrangThaiDonHang;
        set => TrangThaiDonHang = value;
    }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public DateTime? StartDate
    {
        get => TuNgay;
        set => TuNgay = value;
    }
    public DateTime? EndDate
    {
        get => DenNgay;
        set => DenNgay = value?.Date.AddDays(1).AddTicks(-1);
    }
}

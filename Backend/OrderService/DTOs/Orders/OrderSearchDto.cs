namespace OrderService.DTOs.Orders;

public class OrderSearchDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MaNguoiDung { get; set; }
    public string? TrangThaiDonHang { get; set; }
    public string? TrangThaiThanhToan { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
}

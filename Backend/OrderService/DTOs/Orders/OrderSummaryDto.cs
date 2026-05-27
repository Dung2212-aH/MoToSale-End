namespace OrderService.DTOs.Orders;

public class OrderSummaryDto
{
    public int MaDonHang { get; set; }
    public string MaDonHangKinhDoanh { get; set; } = string.Empty;
    public int MaNguoiDung { get; set; }
    public string HoTenNhanHang { get; set; } = string.Empty;
    public string? EmailNhanHang { get; set; }
    public string SoDienThoaiNhanHang { get; set; } = string.Empty;
    public decimal TongThanhToan { get; set; }
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public string TrangThaiVanChuyen { get; set; } = string.Empty;
    public string LoaiDonHang { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayThanhToanThanhCong { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

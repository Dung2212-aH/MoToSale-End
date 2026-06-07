namespace OrderService.DTOs.Orders;

public class PaymentInfoDto
{
    public int MaDonHang { get; set; }
    public string MaDonHangKinhDoanh { get; set; } = string.Empty;
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public string LoaiDonHang { get; set; } = string.Empty;
    public decimal TongThanhToan { get; set; }
    public decimal SoTienCanThanhToan { get; set; }
    public string NoiDungChuyenKhoan { get; set; } = string.Empty;
    public bool DaCauHinhNganHang { get; set; }
    public string? TenNganHang { get; set; }
    public string? SoTaiKhoan { get; set; }
    public string? ChuTaiKhoan { get; set; }
    public string? QrImageUrl { get; set; }
}

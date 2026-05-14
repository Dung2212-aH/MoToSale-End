namespace PaymentService.DTOs.Payments;

public class PaymentOrderSummaryDto
{
    public int MaDonHang { get; set; }
    public string MaDonHangKinhDoanh { get; set; } = string.Empty;
    public int MaNguoiDung { get; set; }
    public string LoaiDonHang { get; set; } = string.Empty;
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public decimal TongThanhToan { get; set; }
    public decimal TienDatCoc { get; set; }
    public decimal TongDaThanhToan { get; set; }
    public decimal TongDaHoan { get; set; }
    public decimal TongThucThu { get; set; }
    public decimal SoTienConPhaiThu { get; set; }
    public int SoLanThanhToanThanhCong { get; set; }
    public int SoLanDangCho { get; set; }
    public DateTime? NgayThanhToanThanhCong { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}

namespace PaymentService.DTOs.Payments;

public class PaymentRefundDto
{
    public int MaHoanTien { get; set; }
    public int MaThanhToan { get; set; }
    public int MaDonHang { get; set; }
    public decimal SoTienHoan { get; set; }
    public string? MaGiaoDichHoanTien { get; set; }
    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}

namespace PaymentService.Entities;

public class PaymentRefund
{
    public int MaHoanTien { get; set; }
    public int MaThanhToan { get; set; }
    public int MaDonHang { get; set; }
    public decimal SoTienHoan { get; set; }
    public string? MaGiaoDichHoanTien { get; set; }
    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? ResponseRaw { get; set; }
    public DateTime NgayTao { get; set; }

    public Payment? Payment { get; set; }
    public Order? Order { get; set; }
}

namespace OrderService.DTOs.Orders;

public class PaymentDto
{
    public int MaThanhToan { get; set; }
    public string MaThanhToanKinhDoanh { get; set; } = string.Empty;
    public int MaDonHang { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string LoaiThanhToan { get; set; } = string.Empty;
    public string? MaGiaoDich { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public DateTime? DaThanhToanLuc { get; set; }
    public DateTime NgayTao { get; set; }
}

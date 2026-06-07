namespace OrderService.Entities;

public class RefundRequest
{
    public int MaYeuCauHoanTien { get; set; }
    public int MaDonHang { get; set; }
    public decimal SoTien { get; set; }
    public string TenNganHang { get; set; } = string.Empty;
    public string SoTaiKhoan { get; set; } = string.Empty;
    public string ChuTaiKhoan { get; set; } = string.Empty;
    public string? LyDo { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public string? GhiChuAdmin { get; set; }
    public string? MaGiaoDichHoan { get; set; }

    public Order? Order { get; set; }
}

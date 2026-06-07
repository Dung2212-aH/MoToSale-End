namespace OrderService.Entities;

public class Payment
{
    public int MaThanhToan { get; set; }
    public string MaThanhToanKinhDoanh { get; set; } = string.Empty;
    public int MaDonHang { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? MaGiaoDich { get; set; }
    public DateTime? DaThanhToanLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public string LoaiThanhToan { get; set; } = string.Empty;
    public string? NoiDungChuyenKhoan { get; set; }
    public string? MaNganHang { get; set; }
    public string? LyDoHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public string? ResponseRaw { get; set; }

    public Order? Order { get; set; }
}

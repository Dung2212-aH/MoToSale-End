namespace OrderService.Entities;

public class OrderHistory
{
    public int MaLichSuDonHang { get; set; }
    public int MaDonHang { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string? GiaTriCu { get; set; }
    public string? GiaTriMoi { get; set; }
    public string? GhiChu { get; set; }
    public int? MaNguoiThucHien { get; set; }
    public DateTime ThoiGian { get; set; }

    public Order? Order { get; set; }
}

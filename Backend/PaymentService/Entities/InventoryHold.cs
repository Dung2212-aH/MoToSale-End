namespace PaymentService.Entities;

public class InventoryHold
{
    public int MaGiuCho { get; set; }
    public int MaDonHang { get; set; }
    public int? MaChiTietDonHang { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public int SoLuong { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime HetHanLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public string? GhiChu { get; set; }

    public Order? Order { get; set; }
}

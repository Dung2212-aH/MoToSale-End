namespace PaymentService.Entities;

public class Order
{
    public int MaDonHang { get; set; }
    public string MaDonHangKinhDoanh { get; set; } = string.Empty;
    public int MaNguoiDung { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public DateTime? NgayThanhToanThanhCong { get; set; }
    public DateTime? NgayHuyDon { get; set; }
    public string? LyDoHuyDon { get; set; }
    public string LoaiDonHang { get; set; } = string.Empty;
    public decimal TienDatCoc { get; set; }
    public decimal SoTienConLai { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InventoryHold> InventoryHolds { get; set; } = new List<InventoryHold>();
}

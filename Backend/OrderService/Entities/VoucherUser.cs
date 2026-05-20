namespace OrderService.Entities;

public class VoucherUser
{
    public int MaVoucherNguoiDung { get; set; }
    public int MaVoucher { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaDonHang { get; set; }
    public string MaVoucherCodeSnapshot { get; set; } = string.Empty;
    public string? LoaiGiamGiaSnapshot { get; set; }
    public decimal? GiaTriGiamSnapshot { get; set; }
    public decimal SoTienGiam { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgaySuDung { get; set; }
    public DateTime NgayTao { get; set; }

    public Voucher? Voucher { get; set; }
}

namespace OrderService.Entities;

public class OrderVoucher
{
    public int MaDonHang { get; set; }
    public int MaVoucher { get; set; }
    public string MaVoucherCodeSnapshot { get; set; } = string.Empty;
    public decimal SoTienGiam { get; set; }
    public DateTime NgayTao { get; set; }
    public string? LoaiGiamGiaSnapshot { get; set; }
    public decimal? GiaTriGiamSnapshot { get; set; }

    public Order? Order { get; set; }
}

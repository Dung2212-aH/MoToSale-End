namespace OrderService.DTOs.Orders;

public class OrderVoucherDto
{
    public int MaVoucher { get; set; }
    public string MaVoucherCodeSnapshot { get; set; } = string.Empty;
    public decimal SoTienGiam { get; set; }
    public string? LoaiGiamGiaSnapshot { get; set; }
    public decimal? GiaTriGiamSnapshot { get; set; }
}

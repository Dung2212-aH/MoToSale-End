namespace OrderService.Entities;

public class VoucherValidationResult
{
    public bool HopLe { get; set; }
    public string? LyDoKhongHopLe { get; set; }
    public int? MaVoucher { get; set; }
    public string? MaVoucherCode { get; set; }
    public string? LoaiGiamGia { get; set; }
    public string? PhamViApDung { get; set; }
    public decimal TongTienHang { get; set; }
    public decimal TongTienHopLe { get; set; }
    public decimal SoTienGiam { get; set; }
}

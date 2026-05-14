namespace OrderService.Entities;

public class Voucher
{
    public int MaVoucher { get; set; }
    public string MaVoucherCode { get; set; } = string.Empty;
    public string LoaiGiamGia { get; set; } = string.Empty;
    public decimal GiaTriGiam { get; set; }
    public decimal GiaTriDonToiThieu { get; set; }
    public decimal? GiaTriGiamToiDa { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public int? GioiHanSuDung { get; set; }
    public int SoLanDaDung { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public string? MoTa { get; set; }
    public int SoLanToiDaMoiNguoiDung { get; set; }
    public string PhamViApDung { get; set; } = string.Empty;
    public DateTime NgayCapNhat { get; set; }
    public string? ApDungLoaiDonHang { get; set; }
}

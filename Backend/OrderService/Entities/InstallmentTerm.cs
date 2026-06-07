namespace OrderService.Entities;

public class InstallmentTerm
{
    public int MaKyTraGop { get; set; }
    public int MaHoSoTraGop { get; set; }
    public int KyThu { get; set; }
    public DateTime NgayDenHan { get; set; }
    public decimal SoTienGoc { get; set; }
    public decimal SoTienLai { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? NgayThanhToan { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }

    public InstallmentPlan? Plan { get; set; }
}

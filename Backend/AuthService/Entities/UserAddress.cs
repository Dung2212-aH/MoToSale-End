namespace AuthService.Entities;

public class UserAddress
{
    public int MaDiaChi { get; set; }
    public int MaNguoiDung { get; set; }
    public string HoTenNhanHang { get; set; } = string.Empty;
    public string SoDienThoaiNhanHang { get; set; } = string.Empty;
    public string DiaChiNhanHang { get; set; } = string.Empty;
    public string? PhuongXa { get; set; }
    public string? QuanHuyen { get; set; }
    public string TinhThanh { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public bool LaMacDinh { get; set; } = true;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

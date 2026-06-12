namespace OrderService.Entities;

public class InstallmentPlan
{
    public int MaHoSoTraGop { get; set; }
    public int MaDonHang { get; set; }
    public decimal TienTraTruoc { get; set; }
    public decimal SoTienGoc { get; set; }
    public int SoKy { get; set; }
    public decimal LaiSuatNam { get; set; }
    public decimal TongTienLai { get; set; }
    public decimal TongPhaiTra { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }

    // --- Borrower personal info ---
    public string HoTenNguoiVay { get; set; } = string.Empty;
    public string SoCCCD { get; set; } = string.Empty;
    public DateTime? NgayCapCCCD { get; set; }
    public string? NoiCapCCCD { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChiThuongTru { get; set; }

    // --- Employment & income (for underwriting) ---
    public string? NgheNghiep { get; set; }
    public string? TenCongTy { get; set; }
    public int? ThoiGianLamViecThang { get; set; }
    public decimal? ThuNhapHangThang { get; set; }

    public Order? Order { get; set; }
}

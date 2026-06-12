using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Orders;

public class InstallmentPlanDto
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
    public string HoTenNguoiVay { get; set; } = string.Empty;
    public string SoCCCD { get; set; } = string.Empty;
    public DateTime? NgayCapCCCD { get; set; }
    public string? NoiCapCCCD { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChiThuongTru { get; set; }
    public string? NgheNghiep { get; set; }
    public string? TenCongTy { get; set; }
    public int? ThoiGianLamViecThang { get; set; }
    public decimal? ThuNhapHangThang { get; set; }
}

public class InstallmentApplicationDto
{
    [Required(ErrorMessage = "Vui long nhap ho ten nguoi vay.")]
    [MaxLength(150)]
    public string HoTenNguoiVay { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap so CCCD/CMND.")]
    [RegularExpression(@"^[0-9]{9,15}$", ErrorMessage = "So CCCD/CMND khong hop le.")]
    public string SoCCCD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap ngay cap CCCD.")]
    public DateTime? NgayCapCCCD { get; set; }

    [Required(ErrorMessage = "Vui long nhap noi cap CCCD.")]
    [MaxLength(150)]
    public string NoiCapCCCD { get; set; } = string.Empty;

    public DateTime? NgaySinh { get; set; }

    [Required(ErrorMessage = "Vui long nhap so dien thoai nguoi vay.")]
    [RegularExpression(@"^[0-9+]{9,15}$", ErrorMessage = "So dien thoai khong hop le.")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap dia chi thuong tru.")]
    [MaxLength(255)]
    public string DiaChiThuongTru { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NgheNghiep { get; set; }

    [MaxLength(150)]
    public string? TenCongTy { get; set; }

    [Range(0, 600)]
    public int? ThoiGianLamViecThang { get; set; }

    [Range(0, 9_999_999_999)]
    public decimal? ThuNhapHangThang { get; set; }
}

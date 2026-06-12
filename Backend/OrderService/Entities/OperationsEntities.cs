namespace OrderService.Entities;

// ===== Nha cung cap (NHACUNGCAP) =====
public class NhaCungCap
{
    public int MaNhaCungCap { get; set; }
    public string MaNhaCungCapKinhDoanh { get; set; } = string.Empty;
    public string TenNhaCungCap { get; set; } = string.Empty;
    public string? MaSoThue { get; set; }
    public string? NguoiLienHe { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string? GhiChu { get; set; }
    public int TrangThai { get; set; } = 1;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

// ===== Don nhap hang (DONNHAPHANG) =====
public class DonNhapHang
{
    public int MaDonNhap { get; set; }
    public string MaDonNhapKinhDoanh { get; set; } = string.Empty;
    public int MaNhaCungCap { get; set; }
    public string TrangThai { get; set; } = "Draft";
    public decimal TongTien { get; set; }
    public decimal DaThanhToan { get; set; }
    public string? GhiChu { get; set; }
    public int? MaNguoiTao { get; set; }
    public int? MaNguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public List<ChiTietDonNhap> ChiTiet { get; set; } = new();
}

public class ChiTietDonNhap
{
    public int MaChiTietNhap { get; set; }
    public int MaDonNhap { get; set; }
    public int MaBienSanPham { get; set; }
    public int SoLuongDat { get; set; }
    public int SoLuongNhan { get; set; }
    public decimal DonGiaNhap { get; set; }
    public DateTime NgayTao { get; set; }
    public DonNhapHang? DonNhap { get; set; }
}

// ===== Phieu nhap kho (PHIEUNHAPKHO) =====
public class PhieuNhapKho
{
    public int MaPhieuNhap { get; set; }
    public string MaPhieuNhapKinhDoanh { get; set; } = string.Empty;
    public int MaDonNhap { get; set; }
    public string? GhiChu { get; set; }
    public int? MaNguoiNhan { get; set; }
    public DateTime NgayNhan { get; set; }
    public DateTime NgayTao { get; set; }
    public List<ChiTietPhieuNhap> ChiTiet { get; set; } = new();
}

public class ChiTietPhieuNhap
{
    public int MaChiTietPhieuNhap { get; set; }
    public int MaPhieuNhap { get; set; }
    public int MaChiTietNhap { get; set; }
    public int MaBienSanPham { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }
}

// ===== So quy / thu chi (SOQUY) =====
public class GiaoDichTienMat
{
    public int MaGiaoDich { get; set; }
    public string MaGiaoDichKinhDoanh { get; set; } = string.Empty;
    public string LoaiGiaoDich { get; set; } = "Receipt"; // Receipt | Payment
    public string DanhMuc { get; set; } = "Other";
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = "Cash"; // Cash | BankTransfer
    public string? LoaiThamChieu { get; set; }
    public int? MaThamChieu { get; set; }
    public string? GhiChu { get; set; }
    public int? MaNguoiGhi { get; set; }
    public DateTime NgayGiaoDich { get; set; }
    public DateTime NgayTao { get; set; }
}

// ===== Phieu sua chua (PHIEUSUACHUA) =====
public class PhieuSuaChua
{
    public int MaPhieuSua { get; set; }
    public string MaPhieuSuaKinhDoanh { get; set; } = string.Empty;
    public int MaKhachHang { get; set; }
    public int? MaNhanVienPhuTrach { get; set; }
    public int? MaBaoHanh { get; set; }
    public string MoTaXe { get; set; } = string.Empty;
    public string MoTaLoi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "Received";
    public decimal ChiPhiCong { get; set; }
    public decimal ChiPhiLinhKien { get; set; }
    public bool DaXuatLinhKien { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTiepNhan { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public List<ChiTietSuaChua> ChiTiet { get; set; } = new();
    public List<LichSuSuaChua> LichSu { get; set; } = new();
}

public class ChiTietSuaChua
{
    public int MaChiTietSua { get; set; }
    public int MaPhieuSua { get; set; }
    public int? MaBienSanPham { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public DateTime NgayTao { get; set; }
}

public class LichSuSuaChua
{
    public int MaLichSuSua { get; set; }
    public int MaPhieuSua { get; set; }
    public string? TrangThaiCu { get; set; }
    public string TrangThaiMoi { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime ThoiGian { get; set; }
}

// ===== Cham soc khach hang (CHAMSOC_KHACHHANG) =====
public class TuongTacKhachHang
{
    public int MaTuongTac { get; set; }
    public int MaKhachHang { get; set; }
    public int? MaNhanVienPhuTrach { get; set; }
    public string LoaiTuongTac { get; set; } = "Call";
    public string TrangThai { get; set; } = "Open";
    public string TieuDe { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime? NgayHenFollowUp { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

// ===== Cham cong (CHAMCONG) =====
public class ChamCong
{
    public int MaChamCong { get; set; }
    public int MaNhanVien { get; set; }
    public DateTime ThoiGianVao { get; set; }
    public DateTime? ThoiGianRa { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
}

// ===== Phieu tra hang (PHIEUTRAHANG) =====
public class PhieuTraHang
{
    public int MaPhieuTra { get; set; }
    public string MaPhieuTraKinhDoanh { get; set; } = string.Empty;
    public int MaDonHang { get; set; }
    public string TrangThai { get; set; } = "Draft";
    public string LyDo { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public decimal SoTienHoan { get; set; }
    public int? MaNguoiTao { get; set; }
    public int? MaNguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public List<ChiTietTraHang> ChiTiet { get; set; } = new();
}

public class ChiTietTraHang
{
    public int MaChiTietTra { get; set; }
    public int MaPhieuTra { get; set; }
    public int MaChiTietDonHang { get; set; }
    public int MaBienSanPham { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public string TinhTrangHang { get; set; } = "Resellable";
    public DateTime NgayTao { get; set; }
}

// ===== Phieu hoan tien (PHIEUHOANTIEN) =====
public class PhieuHoanTien
{
    public int MaHoanTien { get; set; }
    public string MaHoanTienKinhDoanh { get; set; } = string.Empty;
    public int MaDonHang { get; set; }
    public int? MaPhieuTra { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = "Cash";
    public string TrangThai { get; set; } = "Paid";
    public string? LyDo { get; set; }
    public string? MaGiaoDich { get; set; }
    public int? MaNguoiGhi { get; set; }
    public DateTime NgayHoan { get; set; }
    public DateTime NgayTao { get; set; }
}

// ===== Ca lam viec (CALAMVIEC) =====
public class CaLamViec
{
    public int MaCa { get; set; }
    public int MaNhanVien { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }
    public string TrangThai { get; set; } = "Scheduled";
    public string? GhiChu { get; set; }
    public int? MaNguoiPhanCong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

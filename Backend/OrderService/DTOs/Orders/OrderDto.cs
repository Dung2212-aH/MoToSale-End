namespace OrderService.DTOs.Orders;

public class OrderDto
{
    public int MaDonHang { get; set; }
    public string MaDonHangKinhDoanh { get; set; } = string.Empty;
    public int MaNguoiDung { get; set; }
    public int? MaGioHang { get; set; }
    public string HoTenNhanHang { get; set; } = string.Empty;
    public string SoDienThoaiNhanHang { get; set; } = string.Empty;
    public string? EmailNhanHang { get; set; }
    public string DiaChiNhanHang { get; set; } = string.Empty;
    public decimal TongTienHang { get; set; }
    public decimal TienGiam { get; set; }
    public decimal PhiVanChuyen { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiThanhToan { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public DateTime? NgayThanhToanThanhCong { get; set; }
    public DateTime? NgayHuyDon { get; set; }
    public string? LyDoHuyDon { get; set; }
    public string PhuongThucNhanHang { get; set; } = string.Empty;
    public string TrangThaiVanChuyen { get; set; } = string.Empty;
    public string LoaiDonHang { get; set; } = string.Empty;
    public decimal TienDatCoc { get; set; }
    public decimal SoTienConLai { get; set; }
    public DateTime? NgayHenNhanXe { get; set; }
    public string? GhiChuGiaoNhan { get; set; }
    public DateTime? CheckoutHetHanLuc { get; set; }
    public string? PhuongThucThanhToan { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderVoucherDto> Vouchers { get; set; } = new();
    public List<OrderHistoryDto> LichSu { get; set; } = new();
    public List<PaymentDto> DanhSachThanhToan { get; set; } = new();
    public InstallmentPlanDto? TraGop { get; set; }
    public List<RefundRequestDto> YeuCauHoanTien { get; set; } = new();
}

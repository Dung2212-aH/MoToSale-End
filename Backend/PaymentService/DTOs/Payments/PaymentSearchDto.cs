namespace PaymentService.DTOs.Payments;

public class PaymentSearchDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? MaDonHang { get; set; }
    public int? MaNguoiDung { get; set; }
    public string? TrangThai { get; set; }
    public string? PhuongThuc { get; set; }
    public string? LoaiThanhToan { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public DateTime? StartDate
    {
        get => TuNgay;
        set => TuNgay = value;
    }
    public DateTime? EndDate
    {
        get => DenNgay;
        set => DenNgay = value?.Date.AddDays(1).AddTicks(-1);
    }
}

namespace CatalogService.Entities;

public class ReviewOrder
{
    public int MaDonHang { get; set; }
    public int MaNguoiDung { get; set; }
    public string TrangThaiDonHang { get; set; } = string.Empty;
    public string TrangThaiVanChuyen { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }

    public ICollection<ReviewOrderItem> Items { get; set; } = new List<ReviewOrderItem>();
}

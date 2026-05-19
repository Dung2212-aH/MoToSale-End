namespace CatalogService.Entities;

public class ReviewOrderItem
{
    public int MaChiTietDonHang { get; set; }
    public int MaDonHang { get; set; }
    public int MaSanPham { get; set; }

    public ReviewOrder? Order { get; set; }
}

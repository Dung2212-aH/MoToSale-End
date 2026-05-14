namespace OrderService.Entities;

public class OrderItem
{
    public int MaChiTietDonHang { get; set; }
    public int MaDonHang { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string TenSanPhamSnapshot { get; set; } = string.Empty;
    public string? SKUSnapshot { get; set; }
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public decimal ThanhTien { get; private set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
    public ProductVariant? Variant { get; set; }
}

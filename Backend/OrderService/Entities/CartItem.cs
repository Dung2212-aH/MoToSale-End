namespace OrderService.Entities;

public class CartItem
{
    public int MaChiTietGioHang { get; set; }
    public int MaGioHang { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; private set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
    public ProductVariant? Variant { get; set; }
}

namespace OrderService.DTOs.Cart;

public class CartItemDto
{
    public int MaChiTietGioHang { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string? TenBienThe { get; set; }
    public string? SKU { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public string? AnhChinhUrl { get; set; }
}

namespace OrderService.DTOs.Cart;

public class CartDto
{
    public int? MaGioHang { get; set; }
    public int MaNguoiDung { get; set; }
    public string TrangThai { get; set; } = "Active";
    public List<CartItemDto> Items { get; set; } = new();
    public int TongSoLuong { get; set; }
    public decimal TongTienHang { get; set; }
}

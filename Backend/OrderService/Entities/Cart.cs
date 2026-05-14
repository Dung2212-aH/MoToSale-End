namespace OrderService.Entities;

public class Cart
{
    public int MaGioHang { get; set; }
    public int MaNguoiDung { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

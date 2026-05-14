namespace PaymentService.Entities;

public class ProductVariant
{
    public int MaBienSanPham { get; set; }
    public int MaSanPham { get; set; }
    public int? SoLuongTon { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

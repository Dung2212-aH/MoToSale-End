namespace OrderService.Entities;

// Read-model chỉ đọc, ánh xạ bảng ANHSANPHAM (do CatalogService sở hữu).
// OrderService dùng để suy ra ảnh chính cho item giỏ hàng/đơn hàng,
// vì cột denormalized SANPHAM.AnhChinhUrl thường rỗng.
public class ProductImage
{
    public int MaAnhSanPham { get; set; }
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string UrlAnh { get; set; } = string.Empty;
    public bool LaAnhChinh { get; set; }
    public int ThuTuHienThi { get; set; }
}

namespace CatalogService.Entities;
//SANPHAM_LIENQUAN
public class SanPhamLienQuan
{
    public int MaLienQuan { get; set; }
    public int MaSanPham { get; set; }
    public int MaSanPhamLienQuan { get; set; }
    public string LoaiLienQuan { get; set; } = "Accessory";
    public string? GhiChu { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

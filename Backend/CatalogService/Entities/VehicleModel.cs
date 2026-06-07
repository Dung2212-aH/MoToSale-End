namespace CatalogService.Entities;
//DONGXE
public class VehicleModel
{
    public int MaDongXe { get; set; }
    public int MaHangXe { get; set; }
    public string TenDongXe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    // Loại xe: XeSo (xe số) / TayGa (tay ga) / ConTay (côn tay) / XeDien (xe điện) / Khac
    public string LoaiXe { get; set; } = "Khac";
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

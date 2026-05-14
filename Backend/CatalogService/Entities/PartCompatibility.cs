namespace CatalogService.Entities;
//PHUTUNG_TUONGTHICH
public class PartCompatibility
{
    public int MaTuongThich { get; set; }
    public int MaPhuTung { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public short? NamTu { get; set; }
    public short? NamDen { get; set; }
    public bool ApDungTatCaXe { get; set; }
    public string? GhiChu { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs.Products;

public class ProductCreateDto
{
    [Required]
    [MaxLength(50)]
    public string MaSanPhamKinhDoanh { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string TenSanPham { get; set; } = string.Empty;

    [Required]
    [MaxLength(280)]
    public string Slug { get; set; } = string.Empty;

    public int MaDanhMuc { get; set; }
    public int? MaHangXe { get; set; }
    public int? MaDongXe { get; set; }
    public int? MaShowroom { get; set; }

    [Required]
    [MaxLength(20)]
    public string LoaiSanPham { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTaNgan { get; set; }

    public string? MoTa { get; set; }

    [Range(0, double.MaxValue)]
    public decimal GiaGoc { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? GiaKhuyenMai { get; set; }

    [Range(0, int.MaxValue)]
    public int SoLuongTon { get; set; }

    [MaxLength(500)]
    public string? AnhChinhUrl { get; set; }

    public bool DangHoatDong { get; set; } = true;

    [Required]
    [MaxLength(20)]
    public string TrangThaiSanPham { get; set; } = "Active";
}

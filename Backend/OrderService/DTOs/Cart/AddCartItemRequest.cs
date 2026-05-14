using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Cart;

public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int MaSanPham { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaBienSanPham { get; set; }

    [Range(1, 999)]
    public int SoLuong { get; set; } = 1;
}

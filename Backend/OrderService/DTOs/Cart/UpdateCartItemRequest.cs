using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs.Cart;

public class UpdateCartItemRequest
{
    [Range(1, 999)]
    public int SoLuong { get; set; }
}

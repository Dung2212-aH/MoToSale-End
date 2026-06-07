using MoToSale.Common;

namespace MoToSale.Entities.Catalog;

/// <summary>Sản phẩm yêu thích / Wishlist — 1 record / (User, Product).</summary>
public class Favorite : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public Product? Product { get; set; }
}

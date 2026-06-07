namespace MoToSale.DTO.Catalog;

/// <summary>Thông tin sản phẩm rút gọn đính kèm trong mục yêu thích.</summary>
public record FavoriteProductDto(
    int Id, string Code, string Name, string Slug, int CategoryId, int? BrandId, int? VehicleModelId,
    int Kind, decimal ListPrice, decimal? SalePrice, string? MainImageUrl, int Status);

/// <summary>Một sản phẩm trong danh sách yêu thích của khách hàng (1 record / (User, Product)).</summary>
public record FavoriteDto(
    int Id, int UserId, int ProductId, DateTime CreatedAt, FavoriteProductDto Product);

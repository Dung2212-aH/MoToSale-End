using MoToSale.DTO.Common;

namespace MoToSale.DTO.Content;

// Bài viết
public record PostListItem(int Id, string Title, string Slug, string? Category, string PostStatus, DateTime? PublishedAt, DateTime CreatedDate);
public record PostDto(int Id, string Title, string Slug, string? Summary, string Body, string? CoverUrl, string? Category, string PostStatus, DateTime? PublishedAt);
public record SavePostRequest(string Title, string? Slug, string? Summary, string Body, string? CoverUrl, string? Category, string PostStatus, DateTime? PublishedAt);

// Bài viết công khai (trang khách hàng) - chỉ trả về bài đã xuất bản
public record PublicPostListItem(int Id, string Title, string Slug, string? Summary, string? CoverUrl, string? Category, DateTime? PublishedAt, DateTime CreatedDate);
public record PublicPostDto(int Id, string Title, string Slug, string? Summary, string Body, string? CoverUrl, string? Category, DateTime? PublishedAt, DateTime CreatedDate);

// FAQ
public record FaqDto(int Id, string Question, string Answer, string? Category, int SortOrder, int Status);
public record SaveFaqRequest(string Question, string Answer, string? Category, int SortOrder, int Status);

// Liên hệ
public record ContactDto(int Id, string FullName, string Phone, string? Email, string? Subject, string Body, string Type, int? ProductId, string ContactStatus, DateTime CreatedDate, DateTime? HandledAt);

// Form gửi liên hệ công khai (trang khách hàng). Tên trường khớp payload SPA (hoTen, soDienThoai, ...).
public record ContactRequestForm(string? HoTen, string? SoDienThoai, string? Email, string? TieuDe, string? NoiDung, string? LoaiYeuCau, int? MaSanPham);

// Banner
public record BannerDto(int Id, string Position, string? Title, string ImageUrl, string? Link, int SortOrder, int Status);
public record SaveBannerRequest(string Position, string? Title, string ImageUrl, string? Link, int SortOrder, int Status);

// Voucher công khai (trang voucher khách hàng). MaxDiscountValue khớp tên SPA chuẩn hóa.
public record PublicVoucherDto(int Id, string Code, string? Description, string DiscountType, decimal DiscountValue, decimal? MaxDiscountValue, decimal MinOrderValue, DateTime? StartAt, DateTime? EndAt);

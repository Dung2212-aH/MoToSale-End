using Microsoft.AspNetCore.Http;

namespace MoToSale.DTO.Catalog;

/// <summary>Đánh giá hiển thị cho khách hàng (chỉ trạng thái đã duyệt khi liệt kê công khai).</summary>
public record CustomerReviewDto(
    int Id,
    int ProductId,
    int UserId,
    string UserName,
    int Rating,
    string? Title,
    string? Comment,
    string? ImageUrl,
    string Status,
    DateTime CreatedDate);

/// <summary>Phân bố số lượng theo từng mức sao (1..5).</summary>
public record ReviewBreakdownDto(int Five, int Four, int Three, int Two, int One);

/// <summary>Tổng hợp đánh giá đã duyệt của một sản phẩm.</summary>
public record ReviewSummaryDto(
    int ProductId,
    double Average,
    int Count,
    ReviewBreakdownDto Breakdown)
{
    // Bí danh để frontend chuẩn hoá theo nhiều tên trường khác nhau.
    public double AverageRating => Average;
    public int TotalReviews => Count;
}

/// <summary>Trạng thái đánh giá của người dùng hiện tại đối với một sản phẩm.</summary>
public record MyReviewDto(
    int ProductId,
    bool IsAuthenticated,
    bool HasPurchased,
    bool CanReview,
    int? EligibleOrderId,
    string? Reason,
    CustomerReviewDto? MyReview);

/// <summary>Tùy chọn lọc cho trang danh mục sản phẩm.</summary>
public record ProductFiltersDto(
    IEnumerable<CategoryDto> Categories,
    IEnumerable<BrandDto> Brands,
    IEnumerable<VehicleModelDto> Models);

/// <summary>
/// Form tạo đánh giá mới (multipart/form-data).
/// Hỗ trợ cả tên trường theo chuẩn mới (Rating/Title/Comment/File) lẫn tên tiếng Việt mà SPA
/// hiện gửi (Diem/TieuDe/NoiDung/Image) để tương thích ngược.
/// </summary>
public class CreateReviewForm
{
    public int? Rating { get; set; }
    public int? Diem { get; set; }
    public string? Title { get; set; }
    public string? TieuDe { get; set; }
    public string? Comment { get; set; }
    public string? NoiDung { get; set; }
    public IFormFile? File { get; set; }
    public IFormFile? Image { get; set; }

    /// <summary>Điểm đánh giá hợp nhất từ Rating hoặc Diem.</summary>
    public int RatingValue => Rating ?? Diem ?? 0;
    public string? TitleValue => Title ?? TieuDe;
    public string? CommentValue => Comment ?? NoiDung;
    public IFormFile? FileValue => File ?? Image;
}

/// <summary>Form cập nhật đánh giá hiện có của người dùng (multipart/form-data). Xem ghi chú ở <see cref="CreateReviewForm"/>.</summary>
public class UpdateMyReviewForm
{
    public int? Rating { get; set; }
    public int? Diem { get; set; }
    public string? Title { get; set; }
    public string? TieuDe { get; set; }
    public string? Comment { get; set; }
    public string? NoiDung { get; set; }
    public IFormFile? File { get; set; }
    public IFormFile? Image { get; set; }

    public int RatingValue => Rating ?? Diem ?? 0;
    public string? TitleValue => Title ?? TieuDe;
    public string? CommentValue => Comment ?? NoiDung;
    public IFormFile? FileValue => File ?? Image;
}

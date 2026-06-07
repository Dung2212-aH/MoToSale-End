using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;
using MoToSale.Entities.Catalog;
using MoToSale.Repository.Catalog;

namespace MoToSale.Services.Catalog;

public class ReviewService : IReviewService
{
    private const string Pending = "Pending";
    private static readonly HashSet<string> Allowed = new() { "Pending", "Approved", "Rejected", "Hidden" };
    private readonly IReviewRepository _reviews;

    public ReviewService(IReviewRepository reviews) => _reviews = reviews;

    public Task<PagingResponse<ReviewDto>> SearchAsync(PagingRequest request, string? status) => _reviews.SearchAsync(request, status);

    public async Task UpdateStatusAsync(int id, string status)
    {
        if (!Allowed.Contains(status)) throw new CatalogException("Trạng thái không hợp lệ.");
        var rv = await _reviews.GetByIdAsync(id) ?? throw new CatalogException("Không tìm thấy đánh giá.");
        rv.ReviewStatus = status;
        rv.UpdatedDate = DateTime.UtcNow;
        _reviews.Update(rv);
        await _reviews.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var rv = await _reviews.GetByIdAsync(id) ?? throw new CatalogException("Không tìm thấy đánh giá.");
        _reviews.Delete(rv);
        await _reviews.SaveChangesAsync();
    }

    // ===== Khách hàng =====
    public Task<List<CustomerReviewDto>> GetApprovedAsync(int productId) => _reviews.GetApprovedByProductAsync(productId);

    public Task<ReviewSummaryDto> GetSummaryAsync(int productId) => _reviews.GetSummaryAsync(productId);

    public async Task<MyReviewDto> GetMyReviewAsync(int productId, int userId)
    {
        var existing = await _reviews.GetUserReviewAsync(productId, userId);
        var eligibleOrderId = await _reviews.GetEligibleOrderIdAsync(productId, userId);
        var hasPurchased = eligibleOrderId.HasValue;
        // Best-effort: cho phép đánh giá kể cả khi không xác định chắc chắn được lịch sử mua.
        const bool canReview = true;

        return new MyReviewDto(
            ProductId: productId,
            IsAuthenticated: true,
            HasPurchased: hasPurchased,
            CanReview: canReview,
            EligibleOrderId: eligibleOrderId,
            Reason: null,
            MyReview: existing);
    }

    public async Task<CustomerReviewDto> CreateAsync(int productId, int userId, int rating, string? title, string? comment, string? imageUrl)
    {
        if (rating is < 1 or > 5) throw new CatalogException("Điểm đánh giá phải từ 1 đến 5.");
        if (!await _reviews.ProductExistsAsync(productId)) throw new CatalogException("Sản phẩm không tồn tại.");

        var existing = await _reviews.GetUserReviewEntityAsync(productId, userId);
        if (existing is not null) throw new CatalogException("Bạn đã đánh giá sản phẩm này. Vui lòng chỉnh sửa đánh giá hiện có.");

        var now = DateTime.UtcNow;
        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            OrderId = await _reviews.GetEligibleOrderIdAsync(productId, userId),
            Rating = rating,
            Title = Trim(title),
            Comment = Trim(comment),
            ImageUrl = imageUrl,
            ReviewStatus = Pending,
            CreatedDate = now,
            UpdatedDate = now
        };

        _reviews.Add(review);
        await _reviews.SaveChangesAsync();

        return await _reviews.GetUserReviewAsync(productId, userId)
            ?? new CustomerReviewDto(review.Id, productId, userId, "", rating, review.Title, review.Comment, review.ImageUrl, Pending, now);
    }

    public async Task<CustomerReviewDto> UpdateMineAsync(int productId, int userId, int rating, string? title, string? comment, string? imageUrl)
    {
        // 404 nếu chưa có đánh giá (kiểm tra trước khi validate nội dung để controller phân biệt mã lỗi).
        var review = await _reviews.GetUserReviewEntityAsync(productId, userId)
            ?? throw new ReviewNotFoundException("Bạn chưa đánh giá sản phẩm này.");

        if (rating is < 1 or > 5) throw new CatalogException("Điểm đánh giá phải từ 1 đến 5.");

        review.Rating = rating;
        review.Title = Trim(title);
        review.Comment = Trim(comment);
        if (!string.IsNullOrWhiteSpace(imageUrl)) review.ImageUrl = imageUrl; // chỉ thay ảnh khi có ảnh mới
        review.ReviewStatus = Pending;                                        // gửi duyệt lại
        review.UpdatedDate = DateTime.UtcNow;

        _reviews.Update(review);
        await _reviews.SaveChangesAsync();

        return await _reviews.GetUserReviewAsync(productId, userId)
            ?? new CustomerReviewDto(review.Id, productId, userId, "", rating, review.Title, review.Comment, review.ImageUrl, Pending, review.CreatedDate);
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>Ném khi không tìm thấy đánh giá của người dùng (ánh xạ 404). Vẫn là CatalogException để tương thích.</summary>
public class ReviewNotFoundException : CatalogException
{
    public ReviewNotFoundException(string message) : base(message) { }
}

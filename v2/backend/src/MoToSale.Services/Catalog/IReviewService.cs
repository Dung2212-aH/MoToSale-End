using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;

namespace MoToSale.Services.Catalog;

public interface IReviewService
{
    Task<PagingResponse<ReviewDto>> SearchAsync(PagingRequest request, string? status);
    Task UpdateStatusAsync(int id, string status);
    Task DeleteAsync(int id);

    // ===== Khách hàng =====
    /// <summary>Liệt kê đánh giá đã duyệt của một sản phẩm.</summary>
    Task<List<CustomerReviewDto>> GetApprovedAsync(int productId);

    /// <summary>Tổng hợp đánh giá đã duyệt của một sản phẩm.</summary>
    Task<ReviewSummaryDto> GetSummaryAsync(int productId);

    /// <summary>Trạng thái đánh giá của người dùng hiện tại đối với một sản phẩm.</summary>
    Task<MyReviewDto> GetMyReviewAsync(int productId, int userId);

    /// <summary>Tạo đánh giá mới (trạng thái Pending). <paramref name="imageUrl"/> đã được lưu sẵn nếu có.</summary>
    Task<CustomerReviewDto> CreateAsync(int productId, int userId, int rating, string? title, string? comment, string? imageUrl);

    /// <summary>Cập nhật đánh giá hiện có của người dùng và đặt lại trạng thái Pending.</summary>
    Task<CustomerReviewDto> UpdateMineAsync(int productId, int userId, int rating, string? title, string? comment, string? imageUrl);
}

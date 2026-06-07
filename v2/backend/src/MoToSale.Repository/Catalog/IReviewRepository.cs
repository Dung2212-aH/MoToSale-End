using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;
using MoToSale.Entities.Catalog;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Catalog;

public interface IReviewRepository : IRepository<Review>
{
    Task<PagingResponse<ReviewDto>> SearchAsync(PagingRequest request, string? status);

    // ===== Khách hàng =====
    /// <summary>Liệt kê đánh giá ĐÃ DUYỆT (Approved) của một sản phẩm, kèm tên người đánh giá.</summary>
    Task<List<CustomerReviewDto>> GetApprovedByProductAsync(int productId);

    /// <summary>Tổng hợp điểm trung bình, số lượng và phân bố sao từ đánh giá ĐÃ DUYỆT.</summary>
    Task<ReviewSummaryDto> GetSummaryAsync(int productId);

    /// <summary>Lấy đánh giá (mọi trạng thái) của một người dùng cho một sản phẩm, kèm tên người đánh giá.</summary>
    Task<CustomerReviewDto?> GetUserReviewAsync(int productId, int userId);

    /// <summary>Lấy đánh giá thực thể (có theo dõi) của một người dùng cho một sản phẩm.</summary>
    Task<Review?> GetUserReviewEntityAsync(int productId, int userId);

    /// <summary>True nếu người dùng đã mua sản phẩm trong đơn đã giao/hoàn tất; trả mã đơn đủ điều kiện nếu có.</summary>
    Task<int?> GetEligibleOrderIdAsync(int productId, int userId);

    /// <summary>True nếu sản phẩm tồn tại.</summary>
    Task<bool> ProductExistsAsync(int productId);
}

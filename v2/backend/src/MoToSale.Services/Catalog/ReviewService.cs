using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;
using MoToSale.Repository.Catalog;

namespace MoToSale.Services.Catalog;

public class ReviewService : IReviewService
{
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
}

using MoToSale.DTO.Catalog;
using MoToSale.DTO.Common;

namespace MoToSale.Services.Catalog;

public interface IReviewService
{
    Task<PagingResponse<ReviewDto>> SearchAsync(PagingRequest request, string? status);
    Task UpdateStatusAsync(int id, string status);
    Task DeleteAsync(int id);
}

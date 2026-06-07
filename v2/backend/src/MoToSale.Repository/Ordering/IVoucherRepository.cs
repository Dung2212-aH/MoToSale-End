using MoToSale.DTO.Common;
using MoToSale.Entities.Ordering;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Ordering;

public interface IVoucherRepository : IRepository<Voucher>
{
    Task<PagingResponse<Voucher>> SearchAsync(PagingRequest request);
    Task<Voucher?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code, int? exceptId = null);

    /// <summary>Voucher công khai đang hiệu lực (IsPublic, Active, trong khung thời gian, còn lượt) — cho khách xem.</summary>
    Task<PagingResponse<Voucher>> GetActivePublicAsync(PagingRequest request);
}

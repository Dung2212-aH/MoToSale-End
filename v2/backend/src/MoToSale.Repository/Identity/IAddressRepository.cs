using MoToSale.Entities.Identity;
using MoToSale.Repository.EFCore;

namespace MoToSale.Repository.Identity;

public interface IAddressRepository : IRepository<Address>
{
    Task<List<Address>> GetByUserAsync(int userId);
    Task ClearDefaultAsync(int userId);

    /// <summary>Lấy địa chỉ theo Id nhưng chỉ khi thuộc về người dùng (entity được theo dõi để cập nhật/xóa).</summary>
    Task<Address?> GetByIdForUserAsync(int userId, int addressId);
}

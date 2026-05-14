using AuthService.Entities;

namespace AuthService.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
}

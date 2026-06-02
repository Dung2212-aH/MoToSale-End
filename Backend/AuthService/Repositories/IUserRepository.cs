using AuthService.Entities;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByLoginWithRolesAsync(string login);
    Task AddWithRoleAsync(User user, Role role);
}

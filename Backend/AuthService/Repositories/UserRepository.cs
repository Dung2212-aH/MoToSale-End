using AuthService.Data;
using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phone)
    {
        return await _dbContext.Users.AnyAsync(u => u.SoDienThoai == phone);
    }

    public async Task<User?> GetByLoginWithRolesAsync(string login)
    {
        return await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == login || u.SoDienThoai == login);
    }

    public async Task AddWithRoleAsync(User user, Role role)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _dbContext.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            NgayTao = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        user.UserRoles.Add(new UserRole { User = user, Role = role });
    }
}

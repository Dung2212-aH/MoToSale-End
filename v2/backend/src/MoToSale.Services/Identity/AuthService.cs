using MoToSale.Common;
using MoToSale.Common.Auth;
using MoToSale.Common.Helpers;
using MoToSale.DTO.Auth;
using MoToSale.Entities.Identity;
using MoToSale.Repository.Identity;

namespace MoToSale.Services.Identity;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenHelper _token;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenHelper token)
    {
        _users = users;
        _hasher = hasher;
        _token = token;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AuthException("Email và mật khẩu là bắt buộc.");
        }

        if (await _users.EmailExistsAsync(email))
        {
            throw new AuthException("Email đã được sử dụng.");
        }

        var role = await _users.GetRoleByCodeAsync(RoleConstant.Customer);
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            CreatedDate = DateTime.UtcNow,
            Status = (int)EntityStatus.Active,
            UserRoles = { new UserRole { RoleId = role.Id } },
        };

        _users.Add(user);
        await _users.SaveChangesAsync();

        return BuildAuthResponse(user, new[] { RoleConstant.Customer });
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailWithRolesAsync(email);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthException("Email hoặc mật khẩu không đúng.");
        }

        if (user.Status != (int)EntityStatus.Active)
        {
            throw new AuthException("Tài khoản đã bị khóa.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Code).ToArray();
        return BuildAuthResponse(user, roles);
    }

    public async Task<UserResponse?> GetProfileAsync(int userId)
    {
        var user = await _users.GetByIdWithRolesAsync(userId);
        return user is null
            ? null
            : new UserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.UserRoles.Select(ur => ur.Role.Code));
    }

    private AuthResponse BuildAuthResponse(User user, IEnumerable<string> roles)
    {
        var roleList = roles.ToArray();
        var (token, expiresAt) = _token.CreateToken(user.Id, user.FullName, user.Email, roleList);
        return new AuthResponse(token, expiresAt, new UserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, roleList));
    }
}

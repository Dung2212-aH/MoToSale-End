using System.Security.Cryptography;
using MoToSale.Common;
using MoToSale.Common.Auth;
using MoToSale.Common.Helpers;
using MoToSale.DTO.Auth;
using MoToSale.Entities.Identity;
using MoToSale.Repository.Identity;
using MoToSale.Repository.EFCore;

namespace MoToSale.Services.Identity;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenHelper _token;
    private readonly IRepository<PasswordResetToken> _resetTokens;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenHelper token, IRepository<PasswordResetToken> resetTokens)
    {
        _users = users;
        _hasher = hasher;
        _token = token;
        _resetTokens = resetTokens;
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

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailWithRolesAsync(email);

        // Không tiết lộ email tồn tại hay không
        if (user is null)
            return new ForgotPasswordResponse("Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.");

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));

        _resetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedDate = DateTime.UtcNow,
            Status = 1,
        });
        await _resetTokens.SaveChangesAsync();

        // Dev mode: trả về token trực tiếp (production sẽ gửi email)
        return new ForgotPasswordResponse("Nếu email tồn tại, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.", rawToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailWithRolesAsync(email)
            ?? throw new AuthException("Email hoặc token không hợp lệ.");

        var hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Token)));
        var tokens = await _resetTokens.FindAsync(t =>
            t.UserId == user.Id && t.TokenHash == hash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);

        var token = tokens.FirstOrDefault()
            ?? throw new AuthException("Token không hợp lệ hoặc đã hết hạn.");

        user.PasswordHash = _hasher.Hash(request.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        token.UsedAt = DateTime.UtcNow;
        token.UpdatedDate = DateTime.UtcNow;
        _resetTokens.Update(token);

        // Dùng _users.SaveChangesAsync vì user entity được track bởi cùng DbContext
        await _users.SaveChangesAsync();
    }

    private AuthResponse BuildAuthResponse(User user, IEnumerable<string> roles)
    {
        var roleList = roles.ToArray();
        var (token, expiresAt) = _token.CreateToken(user.Id, user.FullName, user.Email, roleList);
        return new AuthResponse(token, expiresAt, new UserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, roleList));
    }
}

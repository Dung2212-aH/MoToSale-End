using AuthService.DTOs;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private const string CustomerRoleName = "Customer";
    private const string ActiveStatus = "Active";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        AuthDbContext dbContext,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.SoDienThoai.Trim();

        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException("Email da duoc su dung.");
        }

        if (await _userRepository.PhoneExistsAsync(phone))
        {
            throw new InvalidOperationException("So dien thoai da duoc su dung.");
        }

        var customerRole = await _roleRepository.GetByNameAsync(CustomerRoleName);
        if (customerRole is null)
        {
            throw new InvalidOperationException("Role Customer chua ton tai trong database.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            HoTen = request.HoTen.Trim(),
            Email = email,
            SoDienThoai = phone,
            MatKhau = _passwordHasher.Hash(request.MatKhau),
            TrangThai = ActiveStatus,
            NgayTao = now,
            NgayCapNhat = now
        };

        await _userRepository.AddWithRoleAsync(user, customerRole);

        return CreateAuthResponse(user, [customerRole.TenVaiTro]);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var login = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByLoginWithRolesAsync(login);

        if (user is null || !_passwordHasher.Verify(request.MatKhau, user.MatKhau))
        {
            throw new UnauthorizedAccessException("Email/số điện thoại hoặc mật khẩu không đúng.");
        }

        if (user.TrangThai != ActiveStatus)
        {
            throw new UnauthorizedAccessException("Tài khoản không ở trạng thái hoạt động.");
        }

        return CreateAuthResponse(user, GetRoleNames(user));
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var message = "Nếu email tồn tại, hệ thống đã tạo liên kết đặt lại mật khẩu.";
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || user.TrangThai != ActiveStatus)
        {
            return new ForgotPasswordResponse { Message = message };
        }

        var token = CreateToken();
        var now = DateTime.UtcNow;

        _dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(token),
            ExpiresAt = now.AddMinutes(30),
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync();

        return new ForgotPasswordResponse
        {
            Message = message,
            ResetToken = token,
            ResetUrl = BuildResetUrl(email, token)
        };
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var tokenHash = HashToken(request.Token.Trim());
        var now = DateTime.UtcNow;

        var resetToken = await _dbContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.TokenHash == tokenHash &&
                t.User.Email == email &&
                t.UsedAt == null &&
                t.ExpiresAt > now);

        if (resetToken is null)
        {
            throw new InvalidOperationException("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        resetToken.User.MatKhau = _passwordHasher.Hash(request.MatKhauMoi);
        resetToken.User.NgayCapNhat = now;
        resetToken.UsedAt = now;

        await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == resetToken.UserId && t.UsedAt == null && t.Id != resetToken.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UsedAt, now));

        await _dbContext.SaveChangesAsync();
    }

    private static List<string> GetRoleNames(User user)
    {
        return user.UserRoles
            .Select(ur => ur.Role.TenVaiTro)
            .ToList();
    }

    private AuthResponse CreateAuthResponse(User user, List<string> roles)
    {
        var jwtToken = _jwtTokenGenerator.Generate(user, roles);

        return new AuthResponse
        {
            Token = jwtToken.Token,
            ExpiresAt = jwtToken.ExpiresAt,
            User = new UserResponse
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                TrangThai = user.TrangThai,
                Roles = roles
            }
        };
    }

    private string BuildResetUrl(string email, string token)
    {
        var baseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
        return $"{baseUrl}/forgot-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
    }

    private static string CreateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

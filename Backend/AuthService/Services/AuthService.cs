using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Repositories;
using AuthService.Security;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private const string CustomerRoleName = "Customer";
    private const string ActiveStatus = "Active";

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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
            throw new UnauthorizedAccessException("Email/so dien thoai hoac mat khau khong dung.");
        }

        if (user.TrangThai != ActiveStatus)
        {
            throw new UnauthorizedAccessException("Tai khoan khong o trang thai Active.");
        }

        return CreateAuthResponse(user, GetRoleNames(user));
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
}

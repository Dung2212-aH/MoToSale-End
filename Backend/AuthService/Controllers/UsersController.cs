using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private const string ActiveStatus = "Active";

    private readonly AuthDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(AuthDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        return user is null ? Unauthorized() : Ok(ToProfile(user));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.SoDienThoai.Trim();

        if (await _dbContext.Users.AnyAsync(u => u.Id != user.Id && u.Email == email))
        {
            return BadRequest(new { message = "Email da duoc su dung." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Id != user.Id && u.SoDienThoai == phone))
        {
            return BadRequest(new { message = "So dien thoai da duoc su dung." });
        }

        user.HoTen = request.HoTen.Trim();
        user.Email = email;
        user.SoDienThoai = phone;
        user.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(ToProfile(user));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Unauthorized();
        }

        if (!_passwordHasher.Verify(request.MatKhauHienTai, user.MatKhau))
        {
            return BadRequest(new { message = "Mat khau hien tai khong dung." });
        }

        user.MatKhau = _passwordHasher.Hash(request.MatKhauMoi);
        user.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Da doi mat khau." });
    }

    [HttpGet("me/address")]
    public async Task<IActionResult> GetDefaultAddress()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var address = await _dbContext.UserAddresses
            .AsNoTracking()
            .Where(a => a.MaNguoiDung == userId.Value)
            .OrderByDescending(a => a.LaMacDinh)
            .ThenByDescending(a => a.NgayCapNhat)
            .FirstOrDefaultAsync();

        return Ok(address is null ? new { } : ToAddress(address));
    }

    [HttpPut("me/address")]
    public async Task<IActionResult> UpsertDefaultAddress(UpdateAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var address = await _dbContext.UserAddresses
            .Where(a => a.MaNguoiDung == userId.Value)
            .OrderByDescending(a => a.LaMacDinh)
            .ThenByDescending(a => a.NgayCapNhat)
            .FirstOrDefaultAsync();

        if (address is null)
        {
            address = new UserAddress
            {
                MaNguoiDung = userId.Value,
                NgayTao = now
            };
            _dbContext.UserAddresses.Add(address);
        }

        address.HoTenNhanHang = request.HoTenNhanHang.Trim();
        address.SoDienThoaiNhanHang = request.SoDienThoaiNhanHang.Trim();
        address.DiaChiNhanHang = request.DiaChiNhanHang.Trim();
        address.PhuongXa = TrimToNull(request.Ward);
        address.QuanHuyen = TrimToNull(request.District);
        address.TinhThanh = request.Province.Trim();
        address.GhiChu = TrimToNull(request.GhiChu);
        address.LaMacDinh = true;
        address.NgayCapNhat = now;

        await _dbContext.SaveChangesAsync();
        return Ok(ToAddress(address));
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        return userId.HasValue ? await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId.Value) : null;
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static object ToProfile(User user)
    {
        return new
        {
            id = user.Id,
            userId = user.Id,
            username = user.Email,
            name = user.HoTen,
            email = user.Email,
            phone = user.SoDienThoai,
            status = user.TrangThai,
            created = user.NgayTao
        };
    }

    private static object ToAddress(UserAddress address)
    {
        return new
        {
            id = address.MaDiaChi,
            fullName = address.HoTenNhanHang,
            phoneNumber = address.SoDienThoaiNhanHang,
            addressLine = address.DiaChiNhanHang,
            ward = address.PhuongXa,
            district = address.QuanHuyen,
            province = address.TinhThanh,
            note = address.GhiChu,
            isDefault = address.LaMacDinh
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(150)]
    public string HoTen { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SoDienThoai { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string MatKhauHienTai { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string MatKhauMoi { get; set; } = string.Empty;
}

public class UpdateAddressRequest
{
    [Required]
    [MaxLength(150)]
    public string HoTenNhanHang { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SoDienThoaiNhanHang { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DiaChiNhanHang { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Ward { get; set; }

    [MaxLength(100)]
    public string? District { get; set; }

    [Required]
    [MaxLength(100)]
    public string Province { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? GhiChu { get; set; }
}

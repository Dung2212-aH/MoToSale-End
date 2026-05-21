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

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.HoTen.ToLower().Contains(s) || u.Email.ToLower().Contains(s) || u.SoDienThoai.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.TrangThai == status);
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                id = u.Id,
                hoTen = u.HoTen,
                email = u.Email,
                soDienThoai = u.SoDienThoai,
                trangThai = u.TrangThai,
                ngayTao = u.NgayTao,
                roles = _dbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.TenVaiTro)
                    .ToList()
            })
            .ToListAsync();

        return Ok(new { items = users, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new
            {
                id = u.Id,
                hoTen = u.HoTen,
                email = u.Email,
                soDienThoai = u.SoDienThoai,
                trangThai = u.TrangThai,
                ngayTao = u.NgayTao,
                roles = _dbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.TenVaiTro)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return user is null ? NotFound(new { message = "Khong tim thay nguoi dung." }) : Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateUser(AdminCreateUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.SoDienThoai.Trim();

        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest(new { message = "Email da duoc su dung." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.SoDienThoai == phone))
        {
            return BadRequest(new { message = "So dien thoai da duoc su dung." });
        }

        var roleNames = NormalizeRoleNames(request.Roles, request.Role);
        var roles = await GetRolesAsync(roleNames);
        if (roles.Count != roleNames.Count)
        {
            return BadRequest(new { message = "Vai tro khong hop le." });
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            HoTen = request.HoTen.Trim(),
            Email = email,
            SoDienThoai = phone,
            MatKhau = _passwordHasher.Hash(request.MatKhau),
            TrangThai = string.IsNullOrWhiteSpace(request.TrangThai) ? ActiveStatus : request.TrangThai.Trim(),
            NgayTao = now,
            NgayCapNhat = now
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        foreach (var role in roles)
        {
            _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, NgayTao = now });
        }

        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, await BuildAdminUserResponseAsync(user.Id));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, AdminUpdateUserRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.SoDienThoai.Trim();

        if (await _dbContext.Users.AnyAsync(u => u.Id != id && u.Email == email))
        {
            return BadRequest(new { message = "Email da duoc su dung." });
        }

        if (await _dbContext.Users.AnyAsync(u => u.Id != id && u.SoDienThoai == phone))
        {
            return BadRequest(new { message = "So dien thoai da duoc su dung." });
        }

        user.HoTen = request.HoTen.Trim();
        user.Email = email;
        user.SoDienThoai = phone;
        user.TrangThai = string.IsNullOrWhiteSpace(request.TrangThai) ? user.TrangThai : request.TrangThai.Trim();
        user.NgayCapNhat = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.MatKhau))
        {
            user.MatKhau = _passwordHasher.Hash(request.MatKhau);
        }

        if ((request.Roles?.Count ?? 0) > 0 || !string.IsNullOrWhiteSpace(request.Role))
        {
            var roleNames = NormalizeRoleNames(request.Roles, request.Role);
            var roles = await GetRolesAsync(roleNames);
            if (roles.Count != roleNames.Count)
            {
                return BadRequest(new { message = "Vai tro khong hop le." });
            }

            var existing = await _dbContext.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            _dbContext.UserRoles.RemoveRange(existing);
            foreach (var role in roles)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = id, RoleId = role.Id, NgayTao = DateTime.UtcNow });
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok(await BuildAdminUserResponseAsync(id));
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, AdminUpdateUserStatusRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        var status = string.IsNullOrWhiteSpace(request.TrangThai) ? request.Status : request.TrangThai;
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { message = "Trang thai khong hop le." });
        }

        user.TrangThai = status.Trim();
        user.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return Ok(await BuildAdminUserResponseAsync(id));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _dbContext.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return NoContent();
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

    private async Task<object> BuildAdminUserResponseAsync(int id)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstAsync(u => u.Id == id);
        var roles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == id)
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.TenVaiTro)
            .ToListAsync();

        return new
        {
            id = user.Id,
            hoTen = user.HoTen,
            email = user.Email,
            soDienThoai = user.SoDienThoai,
            trangThai = user.TrangThai,
            ngayTao = user.NgayTao,
            roles
        };
    }

    private async Task<List<Role>> GetRolesAsync(List<string> roleNames)
    {
        return await _dbContext.Roles
            .Where(r => roleNames.Contains(r.TenVaiTro))
            .ToListAsync();
    }

    private static List<string> NormalizeRoleNames(ICollection<string>? roles, string? role)
    {
        var values = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).ToList() ?? new List<string>();
        if (!string.IsNullOrWhiteSpace(role))
        {
            values.Add(role.Trim());
        }

        return values.Count == 0 ? new List<string> { "Customer" } : values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
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

public class AdminCreateUserRequest
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

    [Required]
    [MinLength(6)]
    public string MatKhau { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? TrangThai { get; set; }

    public string? Role { get; set; }
    public List<string>? Roles { get; set; }
}

public class AdminUpdateUserRequest
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

    public string? MatKhau { get; set; }

    [MaxLength(20)]
    public string? TrangThai { get; set; }

    public string? Role { get; set; }
    public List<string>? Roles { get; set; }
}

public class AdminUpdateUserStatusRequest
{
    [MaxLength(20)]
    public string TrangThai { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Status { get; set; }
}

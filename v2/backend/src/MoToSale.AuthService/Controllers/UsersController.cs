using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common;
using MoToSale.Common.Auth;
using MoToSale.Common.Helpers;
using MoToSale.DTO.Auth;
using MoToSale.DTO.Common;
using MoToSale.Entities.Identity;
using MoToSale.Repository.Identity;
using MoToSale.Services.Identity;

namespace MoToSale.AuthService.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IAddressRepository _addresses;
    private readonly IAuthService _auth;
    private readonly IPasswordHasher _hasher;

    public UsersController(IUserRepository users, IAddressRepository addresses, IAuthService auth, IPasswordHasher hasher)
    {
        _users = users;
        _addresses = addresses;
        _auth = auth;
        _hasher = hasher;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var profile = await _auth.GetProfileAsync(CurrentUserId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestCompat request)
    {
        var user = await _users.GetByIdWithRolesAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.FullName = request.EffectiveFullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.EffectivePhoneNumber) ? null : request.EffectivePhoneNumber.Trim();

        // Frontend khách hàng cũ gửi cả email trong PUT /users/me. Cập nhật email nếu đổi,
        // và kiểm tra trùng lặp trước khi lưu.
        var newEmail = request.EffectiveEmail?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != user.Email)
        {
            if (await _users.EmailExistsAsync(newEmail))
            {
                return BadRequest(new { message = "Email đã được sử dụng." });
            }
            user.Email = newEmail;
        }

        user.UpdatedDate = DateTime.UtcNow;
        await _users.SaveChangesAsync();

        return Ok(await _auth.GetProfileAsync(CurrentUserId));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestCompat request)
    {
        var user = await _users.GetByIdWithRolesAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!_hasher.Verify(request.EffectiveCurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });
        }

        user.PasswordHash = _hasher.Hash(request.EffectiveNewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        await _users.SaveChangesAsync();
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetAddresses() => Ok(new { items = await _addresses.GetByUserAsync(CurrentUserId) });

    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress(AddressRequest request)
    {
        if (request.IsDefault)
        {
            await _addresses.ClearDefaultAsync(CurrentUserId);
        }

        var address = new Address
        {
            UserId = CurrentUserId,
            RecipientName = request.RecipientName.Trim(),
            Phone = request.Phone.Trim(),
            Line = request.Line.Trim(),
            Ward = request.Ward?.Trim(),
            District = request.District?.Trim(),
            Province = request.Province?.Trim(),
            IsDefault = request.IsDefault,
            CreatedDate = DateTime.UtcNow,
            Status = (int)EntityStatus.Active,
        };

        _addresses.Add(address);
        await _addresses.SaveChangesAsync();
        return Ok(new { id = address.Id });
    }

    // Endpoint một-địa-chỉ kiểu cũ: trả về địa chỉ mặc định (hoặc địa chỉ đầu tiên), hoặc null.
    [HttpGet("me/address")]
    public async Task<IActionResult> GetDefaultAddress()
    {
        var addresses = await _addresses.GetByUserAsync(CurrentUserId);
        // GetByUserAsync đã sắp xếp IsDefault giảm dần rồi đến Id, nên phần tử đầu là mặc định nếu có.
        var address = addresses.FirstOrDefault();
        if (address is null) return Ok((object?)null);

        return Ok(new
        {
            address.Id,
            address.RecipientName,
            address.Phone,
            address.Line,
            address.Ward,
            address.District,
            address.Province,
            address.IsDefault,
        });
    }

    [HttpPut("me/addresses/{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, AddressRequest request)
    {
        var address = await _addresses.GetByIdForUserAsync(CurrentUserId, id);
        if (address is null) return NotFound();

        if (request.IsDefault)
        {
            await _addresses.ClearDefaultAsync(CurrentUserId);
        }

        address.RecipientName = request.RecipientName.Trim();
        address.Phone = request.Phone.Trim();
        address.Line = request.Line.Trim();
        address.Ward = request.Ward?.Trim();
        address.District = request.District?.Trim();
        address.Province = request.Province?.Trim();
        address.IsDefault = request.IsDefault;
        address.UpdatedDate = DateTime.UtcNow;

        await _addresses.SaveChangesAsync();
        return Ok(new { id = address.Id });
    }

    [HttpPut("me/addresses/{id:int}/default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var address = await _addresses.GetByIdForUserAsync(CurrentUserId, id);
        if (address is null) return NotFound();

        await _addresses.ClearDefaultAsync(CurrentUserId);
        address.IsDefault = true;
        address.UpdatedDate = DateTime.UtcNow;

        await _addresses.SaveChangesAsync();
        return Ok(new { id = address.Id });
    }

    [HttpDelete("me/addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var address = await _addresses.GetByIdForUserAsync(CurrentUserId, id);
        if (address is null) return NotFound();

        _addresses.Delete(address);
        await _addresses.SaveChangesAsync();
        return Ok(new { message = "Đã xóa địa chỉ." });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PagingRequest request)
    {
        var page = await _users.SearchAsync(request);
        var result = new PagingResponse<object>
        {
            Items = page.Items.Select(u => (object)new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Status,
                roles = u.UserRoles.Select(ur => ur.Role.Code),
                u.CreatedDate,
            }).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalItems = page.TotalItems,
        };
        return Ok(result);
    }

    // ===== Khách hàng =====
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery] PagingRequest request)
    {
        var page = await _users.SearchCustomersAsync(request);
        return Ok(new PagingResponse<object>
        {
            Items = page.Items.Select(u => (object)new CustomerDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Status, u.CareNote, u.CreatedDate)).ToList(),
            Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems,
        });
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPatch("customers/{id:int}/care-note")]
    public async Task<IActionResult> UpdateCareNote(int id, CareNoteRequest request)
    {
        var user = await _users.GetByIdWithRolesAsync(id);
        if (user is null) return NotFound();
        user.CareNote = request.CareNote;
        user.UpdatedDate = DateTime.UtcNow;
        await _users.SaveChangesAsync();
        return Ok(new { id });
    }

    // ===== Quản trị người dùng (Admin) =====
    [Authorize(Roles = RoleConstant.Admin)]
    [HttpGet("all")]
    public Task<IActionResult> All([FromQuery] PagingRequest request) => List(request);

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _users.GetByIdWithRolesAsync(id);
        if (u is null) return NotFound();
        return Ok(new { u.Id, u.FullName, u.Email, u.PhoneNumber, u.Status, roles = u.UserRoles.Select(ur => ur.Role.Code), u.CareNote, u.CreatedDate });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email)) return BadRequest(new { message = "Email đã được sử dụng." });
        var role = await _users.GetRoleByCodeAsync(NormalizeRole(request.Role));
        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = request.FullName.Trim(), Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = _hasher.Hash(string.IsNullOrWhiteSpace(request.Password) ? "Changeme@123" : request.Password),
            Status = (int)EntityStatus.Active, CreatedDate = now,
            UserRoles = { new UserRole { RoleId = role.Id } },
        };
        _users.Add(user);
        await _users.SaveChangesAsync();
        return Ok(new { id = user.Id });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AdminUpdateUserRequest request)
    {
        var user = await _users.GetByIdWithRolesAsync(id);
        if (user is null) return NotFound();
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.Status = request.Status;
        var role = await _users.GetRoleByCodeAsync(NormalizeRole(request.Role));
        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole { RoleId = role.Id });
        user.UpdatedDate = DateTime.UtcNow;
        await _users.SaveChangesAsync();
        return Ok(new { id });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, UpdateStatusRequest request)
    {
        var user = await _users.GetByIdWithRolesAsync(id);
        if (user is null) return NotFound();
        user.Status = request.Status;
        user.UpdatedDate = DateTime.UtcNow;
        await _users.SaveChangesAsync();
        return Ok(new { id });
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _users.GetByIdWithRolesAsync(id);
        if (user is null) return NotFound();
        user.UserRoles.Clear();
        _users.Delete(user);
        await _users.SaveChangesAsync();
        return Ok(new { message = "Đã xóa người dùng." });
    }

    private static string NormalizeRole(string? role) => role switch
    {
        RoleConstant.Admin => RoleConstant.Admin,
        RoleConstant.Staff => RoleConstant.Staff,
        _ => RoleConstant.Customer,
    };
}

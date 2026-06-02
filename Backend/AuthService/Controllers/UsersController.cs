using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Security;
using AuthService.Services;
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
    private readonly IAuditLogService _auditLog;

    public UsersController(AuthDbContext dbContext, IPasswordHasher passwordHasher, IAuditLogService auditLog)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _auditLog = auditLog;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
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

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleName = role.Trim();
            query = query.Where(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.Id && ur.Role.TenVaiTro == roleName));
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

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        await EnsureCustomerNoteTableAsync();
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.Users
            .AsNoTracking()
            .Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.Role.TenVaiTro == "Customer"));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.HoTen.ToLower().Contains(s) || u.Email.ToLower().Contains(s) || u.SoDienThoai.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(u => u.TrangThai == status.Trim());
        }

        var total = await query.CountAsync();
        var customers = await query
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
                ngayTao = u.NgayTao
            })
            .ToListAsync();

        var customerIds = customers.Select(c => c.id).ToList();
        var notes = await _dbContext.Database.SqlQueryRaw<CustomerCareNoteRow>(
            "SELECT MaNguoiDung, GhiChuChamSoc, NgayCapNhat FROM dbo.KHACHHANG_GHICHU_CHAMSOC"
        ).ToListAsync();
        var noteMap = notes.Where(n => customerIds.Contains(n.MaNguoiDung)).ToDictionary(n => n.MaNguoiDung, n => n);

        var items = customers.Select(c => new
        {
            c.id,
            c.hoTen,
            c.email,
            c.soDienThoai,
            c.trangThai,
            c.ngayTao,
            ghiChuChamSoc = noteMap.TryGetValue(c.id, out var note) ? note.GhiChuChamSoc : null,
            ngayCapNhatGhiChu = noteMap.TryGetValue(c.id, out var noteDate) ? (DateTime?)noteDate.NgayCapNhat : null
        });

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("customers/{id:int}/care-note")]
    public async Task<IActionResult> UpdateCustomerCareNote(int id, CustomerCareNoteRequest request)
    {
        await EnsureCustomerNoteTableAsync();
        var isCustomer = await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == id && ur.Role.TenVaiTro == "Customer");
        if (!isCustomer)
        {
            return NotFound(new { message = "Khong tim thay khach hang." });
        }

        var note = string.IsNullOrWhiteSpace(request.GhiChuChamSoc) ? null : request.GhiChuChamSoc.Trim();
        var now = DateTime.UtcNow;
        var actorId = GetCurrentUserId();
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            MERGE dbo.KHACHHANG_GHICHU_CHAMSOC AS target
            USING (SELECT {id} AS MaNguoiDung) AS source
            ON target.MaNguoiDung = source.MaNguoiDung
            WHEN MATCHED THEN
                UPDATE SET GhiChuChamSoc = {note}, MaNguoiCapNhat = {actorId}, NgayCapNhat = {now}
            WHEN NOT MATCHED THEN
                INSERT (MaNguoiDung, GhiChuChamSoc, MaNguoiCapNhat, NgayCapNhat)
                VALUES ({id}, {note}, {actorId}, {now});
            """);
        await _auditLog.WriteAsync(this, "Customer", id.ToString(), "UpdateCareNote", null, new { GhiChuChamSoc = note });

        return Ok(new { id, ghiChuChamSoc = note, ngayCapNhat = now });
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

        if (roleNames.Contains("Admin", StringComparer.OrdinalIgnoreCase) && await CountAdminsAsync() > 0)
        {
            return BadRequest(new { message = "He thong chi cho phep mot tai khoan Admin. Hay tao tai khoan Staff cho nhan su van hanh." });
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
        await _auditLog.WriteAsync(this, "User", user.Id.ToString(), "Create", null, new { user.Id, user.HoTen, user.Email, user.SoDienThoai, user.TrangThai, Roles = roleNames });
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

        var currentUserId = GetCurrentUserId();
        var existingIsAdmin = await IsAdminAsync(id);
        var oldRoles = await GetUserRoleNamesAsync(id);
        var oldValue = new { user.HoTen, user.Email, user.SoDienThoai, user.TrangThai, Roles = oldRoles };

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

        var nextStatus = string.IsNullOrWhiteSpace(request.TrangThai) ? user.TrangThai : request.TrangThai.Trim();
        var hasRoleChange = (request.Roles?.Count ?? 0) > 0 || !string.IsNullOrWhiteSpace(request.Role);
        var nextRoleNames = hasRoleChange
            ? NormalizeRoleNames(request.Roles, request.Role)
            : await GetUserRoleNamesAsync(id);
        var nextIsAdmin = nextRoleNames.Contains("Admin", StringComparer.OrdinalIgnoreCase);

        if (!existingIsAdmin && nextIsAdmin && await CountAdminsAsync() > 0)
        {
            return BadRequest(new { message = "He thong chi cho phep mot tai khoan Admin. Hay dung vai tro Staff cho nhan su van hanh." });
        }

        var validationError = await ValidateAdminProtectionAsync(id, currentUserId, existingIsAdmin, user.TrangThai, nextIsAdmin, nextStatus);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        user.HoTen = request.HoTen.Trim();
        user.Email = email;
        user.SoDienThoai = phone;
        user.TrangThai = nextStatus;
        user.NgayCapNhat = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.MatKhau))
        {
            user.MatKhau = _passwordHasher.Hash(request.MatKhau);
        }

        if (hasRoleChange)
        {
            var roles = await GetRolesAsync(nextRoleNames);
            if (roles.Count != nextRoleNames.Count)
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
        await _auditLog.WriteAsync(this, "User", id.ToString(), "Update", oldValue, new { user.HoTen, user.Email, user.SoDienThoai, user.TrangThai, Roles = nextRoleNames, PasswordChanged = !string.IsNullOrWhiteSpace(request.MatKhau) });
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

        var existingIsAdmin = await IsAdminAsync(id);
        var validationError = await ValidateAdminProtectionAsync(
            id,
            GetCurrentUserId(),
            existingIsAdmin,
            user.TrangThai,
            existingIsAdmin,
            status.Trim());
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }
        var oldValue = new { user.TrangThai };

        user.TrangThai = status.Trim();
        user.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "User", id.ToString(), "UpdateStatus", oldValue, new { user.TrangThai });
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

        if (GetCurrentUserId() == id)
        {
            return BadRequest(new { message = "Khong the xoa tai khoan dang dang nhap." });
        }

        if (await IsAdminAsync(id) && user.TrangThai == ActiveStatus && await CountActiveAdminsAsync() <= 1)
        {
            return BadRequest(new { message = "Khong the xoa quan tri vien hoat dong cuoi cung." });
        }
        var oldValue = new { user.Id, user.HoTen, user.Email, user.SoDienThoai, user.TrangThai, Roles = user.UserRoles.Select(ur => ur.RoleId).ToList() };

        user.TrangThai = "Inactive";
        user.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "User", id.ToString(), "Deactivate", oldValue, new { user.TrangThai });
        return Ok(await BuildAdminUserResponseAsync(id));
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

    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetMyAddresses()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var addresses = await _dbContext.UserAddresses
            .AsNoTracking()
            .Where(a => a.MaNguoiDung == userId.Value)
            .OrderByDescending(a => a.LaMacDinh)
            .ThenByDescending(a => a.NgayCapNhat)
            .ToListAsync();

        return Ok(new { items = addresses.Select(ToAddress).ToList() });
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
            await ClearDefaultAddressAsync(userId.Value);
            await _dbContext.SaveChangesAsync();

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

        if (address.MaDiaChi > 0)
        {
            await ClearDefaultAddressAsync(userId.Value, address.MaDiaChi);
        }
        await _dbContext.SaveChangesAsync();
        return Ok(ToAddress(address));
    }

    [HttpPost("me/addresses")]
    public async Task<IActionResult> CreateAddress(UpdateAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var shouldBeDefault = await ShouldAddressBeDefaultAsync(userId.Value, request);
        var address = new UserAddress
        {
            MaNguoiDung = userId.Value,
            HoTenNhanHang = request.HoTenNhanHang.Trim(),
            SoDienThoaiNhanHang = request.SoDienThoaiNhanHang.Trim(),
            DiaChiNhanHang = request.DiaChiNhanHang.Trim(),
            PhuongXa = TrimToNull(request.Ward),
            QuanHuyen = TrimToNull(request.District),
            TinhThanh = request.Province.Trim(),
            GhiChu = TrimToNull(request.GhiChu),
            LaMacDinh = shouldBeDefault,
            NgayTao = now,
            NgayCapNhat = now
        };

        if (shouldBeDefault)
        {
            await ClearDefaultAddressAsync(userId.Value);
            await _dbContext.SaveChangesAsync();
        }

        _dbContext.UserAddresses.Add(address);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyAddresses), ToAddress(address));
    }

    [HttpPut("me/addresses/{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, UpdateAddressRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var address = await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.MaDiaChi == id && a.MaNguoiDung == userId.Value);

        if (address is null)
        {
            return NotFound(new { message = "Khong tim thay dia chi nhan hang." });
        }

        var makeDefault = request.LaMacDinh == true || request.IsDefault == true;
        if (makeDefault)
        {
            await ClearDefaultAddressAsync(userId.Value, id);
            await _dbContext.SaveChangesAsync();
        }

        address.HoTenNhanHang = request.HoTenNhanHang.Trim();
        address.SoDienThoaiNhanHang = request.SoDienThoaiNhanHang.Trim();
        address.DiaChiNhanHang = request.DiaChiNhanHang.Trim();
        address.PhuongXa = TrimToNull(request.Ward);
        address.QuanHuyen = TrimToNull(request.District);
        address.TinhThanh = request.Province.Trim();
        address.GhiChu = TrimToNull(request.GhiChu);
        address.LaMacDinh = makeDefault || address.LaMacDinh;
        address.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(ToAddress(address));
    }

    [HttpPut("me/addresses/{id:int}/default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var address = await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.MaDiaChi == id && a.MaNguoiDung == userId.Value);

        if (address is null)
        {
            return NotFound(new { message = "Khong tim thay dia chi nhan hang." });
        }

        await ClearDefaultAddressAsync(userId.Value, id);
        await _dbContext.SaveChangesAsync();
        address.LaMacDinh = true;
        address.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(ToAddress(address));
    }

    [HttpDelete("me/addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var address = await _dbContext.UserAddresses
            .FirstOrDefaultAsync(a => a.MaDiaChi == id && a.MaNguoiDung == userId.Value);

        if (address is null)
        {
            return NotFound(new { message = "Khong tim thay dia chi nhan hang." });
        }

        var wasDefault = address.LaMacDinh;
        _dbContext.UserAddresses.Remove(address);
        await _dbContext.SaveChangesAsync();

        if (wasDefault)
        {
            var nextDefault = await _dbContext.UserAddresses
                .Where(a => a.MaNguoiDung == userId.Value)
                .OrderByDescending(a => a.NgayCapNhat)
                .FirstOrDefaultAsync();

            if (nextDefault is not null)
            {
                nextDefault.LaMacDinh = true;
                nextDefault.NgayCapNhat = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        return NoContent();
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

    private async Task EnsureCustomerNoteTableAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.KHACHHANG_GHICHU_CHAMSOC', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.KHACHHANG_GHICHU_CHAMSOC (
                    MaNguoiDung INT NOT NULL PRIMARY KEY,
                    GhiChuChamSoc NVARCHAR(1000) NULL,
                    MaNguoiCapNhat INT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL
                );
            END;
            """);
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

    private async Task<bool> ShouldAddressBeDefaultAsync(int userId, UpdateAddressRequest request)
    {
        if (request.LaMacDinh == true || request.IsDefault == true)
        {
            return true;
        }

        return !await _dbContext.UserAddresses.AnyAsync(a => a.MaNguoiDung == userId);
    }

    private async Task ClearDefaultAddressAsync(int userId, int? exceptAddressId = null)
    {
        var defaultAddresses = await _dbContext.UserAddresses
            .Where(a => a.MaNguoiDung == userId && a.LaMacDinh && (!exceptAddressId.HasValue || a.MaDiaChi != exceptAddressId.Value))
            .ToListAsync();

        foreach (var item in defaultAddresses)
        {
            item.LaMacDinh = false;
            item.NgayCapNhat = DateTime.UtcNow;
        }
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

    private async Task<List<string>> GetUserRoleNamesAsync(int userId)
    {
        return await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.TenVaiTro)
            .ToListAsync();
    }

    private async Task<bool> IsAdminAsync(int userId)
    {
        return await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role.TenVaiTro == "Admin");
    }

    private async Task<int> CountActiveAdminsAsync()
    {
        return await _dbContext.Users
            .Where(u => u.TrangThai == ActiveStatus)
            .CountAsync(u => _dbContext.UserRoles
                .Any(ur => ur.UserId == u.Id && ur.Role.TenVaiTro == "Admin"));
    }

    private async Task<int> CountAdminsAsync()
    {
        return await _dbContext.UserRoles
            .CountAsync(ur => ur.Role.TenVaiTro == "Admin");
    }

    private async Task<string?> ValidateAdminProtectionAsync(
        int targetUserId,
        int? currentUserId,
        bool currentIsAdmin,
        string currentStatus,
        bool nextIsAdmin,
        string nextStatus)
    {
        var isSelf = currentUserId == targetUserId;
        var nextIsActive = string.Equals(nextStatus, ActiveStatus, StringComparison.OrdinalIgnoreCase);

        if (isSelf && !nextIsActive)
        {
            return "Khong the khoa tai khoan dang dang nhap.";
        }

        if (isSelf && currentIsAdmin && !nextIsAdmin)
        {
            return "Khong the go vai tro Admin cua chinh minh.";
        }

        var removesActiveAdmin = currentIsAdmin
            && string.Equals(currentStatus, ActiveStatus, StringComparison.OrdinalIgnoreCase)
            && (!nextIsAdmin || !nextIsActive);

        if (removesActiveAdmin && await CountActiveAdminsAsync() <= 1)
        {
            return "Khong the vo hieu hoa quan tri vien hoat dong cuoi cung.";
        }

        return null;
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

    public bool? LaMacDinh { get; set; }

    public bool? IsDefault { get; set; }
}

public class CustomerCareNoteRequest
{
    public string? GhiChuChamSoc { get; set; }
}

public class CustomerCareNoteRow
{
    public int MaNguoiDung { get; set; }
    public string? GhiChuChamSoc { get; set; }
    public DateTime NgayCapNhat { get; set; }
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

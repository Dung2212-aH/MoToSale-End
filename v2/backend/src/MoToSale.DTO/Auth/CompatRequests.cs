namespace MoToSale.DTO.Auth;

// ============================================================================
// Lớp request "khoan dung" (tolerant) cho frontend khách hàng CŨ.
//
// Frontend khách hàng (chưa đổi) gửi body với KHÓA tiếng Việt (matKhau, hoTen,
// soDienThoai, ...). v2 dùng property tiếng Anh nên binding thất bại.
//
// System.Text.Json bind theo TÊN property không phân biệt hoa/thường, nên một
// class có cả property tiếng Anh LẪN tiếng Việt sẽ nhận được giá trị dù client
// gửi khóa nào. Các getter "Effective*" chọn giá trị khác null đầu tiên.
//
// LƯU Ý: KHÔNG được sửa các record trong AuthDtos.cs (test + admin phụ thuộc).
// Đây là lớp ADDITIVE: controller nhận lớp này rồi map sang record/service cũ.
// Frontend admin (gửi khóa tiếng Anh) vẫn hoạt động vì các property tiếng Anh
// vẫn tồn tại.
// ============================================================================

/// <summary>POST /auth/login — chấp nhận { email, matKhau } và { Email, Password }.</summary>
public class LoginRequestCompat
{
    public string? Email { get; set; }

    // Khóa tiếng Anh (admin / hợp đồng v2)
    public string? Password { get; set; }

    // Khóa tiếng Việt (frontend khách hàng cũ)
    public string? MatKhau { get; set; }

    public string EffectiveEmail => Email ?? string.Empty;
    public string EffectivePassword => Password ?? MatKhau ?? string.Empty;

    public LoginRequest ToRequest() => new(EffectiveEmail, EffectivePassword);
}

/// <summary>POST /auth/register — chấp nhận { hoTen, email, soDienThoai, matKhau } và khóa tiếng Anh.</summary>
public class RegisterRequestCompat
{
    public string? Email { get; set; }

    // Khóa tiếng Anh
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }

    // Khóa tiếng Việt
    public string? HoTen { get; set; }
    public string? SoDienThoai { get; set; }
    public string? MatKhau { get; set; }

    public string EffectiveFullName => FullName ?? HoTen ?? string.Empty;
    public string EffectiveEmail => Email ?? string.Empty;
    public string? EffectivePhoneNumber => PhoneNumber ?? SoDienThoai;
    public string EffectivePassword => Password ?? MatKhau ?? string.Empty;

    public RegisterRequest ToRequest() =>
        new(EffectiveFullName, EffectiveEmail, EffectivePhoneNumber, EffectivePassword);
}

/// <summary>POST /auth/reset-password — chấp nhận { email, token, matKhauMoi } và { ..., NewPassword }.</summary>
public class ResetPasswordRequestCompat
{
    public string? Email { get; set; }
    public string? Token { get; set; }

    // Khóa tiếng Anh
    public string? NewPassword { get; set; }

    // Khóa tiếng Việt
    public string? MatKhauMoi { get; set; }

    public string EffectiveEmail => Email ?? string.Empty;
    public string EffectiveToken => Token ?? string.Empty;
    public string EffectiveNewPassword => NewPassword ?? MatKhauMoi ?? string.Empty;

    public ResetPasswordRequest ToRequest() =>
        new(EffectiveEmail, EffectiveToken, EffectiveNewPassword);
}

/// <summary>PUT /users/me — chấp nhận { hoTen, email, soDienThoai } và khóa tiếng Anh. Bao gồm Email (cập nhật được).</summary>
public class UpdateProfileRequestCompat
{
    public string? Email { get; set; }

    // Khóa tiếng Anh
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }

    // Khóa tiếng Việt
    public string? HoTen { get; set; }
    public string? SoDienThoai { get; set; }

    public string EffectiveFullName => FullName ?? HoTen ?? string.Empty;
    public string? EffectivePhoneNumber => PhoneNumber ?? SoDienThoai;

    /// <summary>Email mong muốn, hoặc null nếu client không gửi (không đổi email).</summary>
    public string? EffectiveEmail => string.IsNullOrWhiteSpace(Email) ? null : Email;
}

/// <summary>PUT /users/me/password — chấp nhận { matKhauHienTai, matKhauMoi } và { CurrentPassword, NewPassword }.</summary>
public class ChangePasswordRequestCompat
{
    // Khóa tiếng Anh
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }

    // Khóa tiếng Việt
    public string? MatKhauHienTai { get; set; }
    public string? MatKhauMoi { get; set; }

    public string EffectiveCurrentPassword => CurrentPassword ?? MatKhauHienTai ?? string.Empty;
    public string EffectiveNewPassword => NewPassword ?? MatKhauMoi ?? string.Empty;
}

/// <summary>
/// Địa chỉ — chấp nhận khóa tiếng Việt của frontend khách hàng cũ và khóa tiếng Anh v2.
/// hoTenNhanHang→RecipientName, soDienThoaiNhanHang→Phone, diaChiNhanHang→Line,
/// ward→Ward, district→District, province→Province, laMacDinh→IsDefault.
/// ghiChu: Address entity KHÔNG có cột Note → nối vào Line để không mất dữ liệu.
/// </summary>
public class AddressRequestCompat
{
    // Khóa tiếng Anh (admin / hợp đồng v2)
    public string? RecipientName { get; set; }
    public string? Phone { get; set; }
    public string? Line { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public bool? IsDefault { get; set; }

    // Khóa tiếng Việt (frontend khách hàng cũ)
    public string? HoTenNhanHang { get; set; }
    public string? SoDienThoaiNhanHang { get; set; }
    public string? DiaChiNhanHang { get; set; }
    public string? GhiChu { get; set; }
    public bool? LaMacDinh { get; set; }

    public string EffectiveRecipientName => RecipientName ?? HoTenNhanHang ?? string.Empty;
    public string EffectivePhone => Phone ?? SoDienThoaiNhanHang ?? string.Empty;
    public string? EffectiveWard => Ward;
    public string? EffectiveDistrict => District;
    public string? EffectiveProvince => Province;
    public bool EffectiveIsDefault => IsDefault ?? LaMacDinh ?? false;

    /// <summary>
    /// Dòng địa chỉ. Vì entity không có cột Note, ghiChu (nếu có) được nối vào sau Line.
    /// </summary>
    public string EffectiveLine
    {
        get
        {
            var line = (Line ?? DiaChiNhanHang ?? string.Empty).Trim();
            var note = GhiChu?.Trim();
            if (!string.IsNullOrWhiteSpace(note))
            {
                line = string.IsNullOrWhiteSpace(line) ? note : $"{line} ({note})";
            }
            return line;
        }
    }

    public AddressRequest ToRequest() =>
        new(EffectiveRecipientName, EffectivePhone, EffectiveLine,
            EffectiveWard, EffectiveDistrict, EffectiveProvince, EffectiveIsDefault);
}

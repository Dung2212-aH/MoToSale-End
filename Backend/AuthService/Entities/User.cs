namespace AuthService.Entities;

public class User
{
    public int Id { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get;   set; }
    public DateTime NgayCapNhat { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

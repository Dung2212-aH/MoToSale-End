namespace AuthService.DTOs;

public class UserResponse
{
    public int Id { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

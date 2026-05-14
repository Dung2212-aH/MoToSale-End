namespace AuthService.Entities;

public class Role
{
    public byte Id { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

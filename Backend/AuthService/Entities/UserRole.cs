namespace AuthService.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public byte RoleId { get; set; }
    public DateTime NgayTao { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

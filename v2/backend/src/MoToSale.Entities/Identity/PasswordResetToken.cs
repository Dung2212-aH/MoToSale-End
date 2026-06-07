using MoToSale.Common;

namespace MoToSale.Entities.Identity;

/// <summary>Token đặt lại mật khẩu — sinh ra cho mỗi yêu cầu forgot-password.</summary>
public class PasswordResetToken : BaseEntity
{
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty; // Hash của token (SHA256)
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    public User User { get; set; } = null!;
}

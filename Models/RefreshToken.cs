namespace BlogApi.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = "";

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked =>
        RevokedAt != null;

    public bool IsActive =>
        !IsExpired && !IsRevoked;
}
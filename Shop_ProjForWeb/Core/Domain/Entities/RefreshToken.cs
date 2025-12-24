namespace Shop_ProjForWeb.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? CreatedByIp { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}


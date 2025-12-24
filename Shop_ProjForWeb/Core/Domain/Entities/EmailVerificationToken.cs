namespace Shop_ProjForWeb.Domain.Entities;

public class EmailVerificationToken : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}


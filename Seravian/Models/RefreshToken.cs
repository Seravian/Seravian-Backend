public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
}

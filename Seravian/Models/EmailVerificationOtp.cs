public class EmailVerificationOtp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Code { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsConsumed { get; set; }

    public virtual User User { get; set; }
}

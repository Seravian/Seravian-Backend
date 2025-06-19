public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? FullName { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public string PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public UserRole? Role { get; set; }
    public bool IsProfileSetupComplete { get; set; }
    public byte[] RowVersion { get; set; } // RowVersion column for concurrency

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<EmailVerificationOtp> EmailVerificationOtps { get; set; }
}

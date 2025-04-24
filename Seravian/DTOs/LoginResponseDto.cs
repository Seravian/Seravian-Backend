public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string? FullName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public UserRole? Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool? IsDoctorVerified { get; set; } = null;

    public bool IsProfileSetupComplete { get; set; }
    public AuthTokens? Tokens { get; set; }
}

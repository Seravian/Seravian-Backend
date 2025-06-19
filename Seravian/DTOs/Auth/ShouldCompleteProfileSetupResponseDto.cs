public class ShouldCompleteProfileSetupResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public UserRole? Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public TokenWithoutRoleDto Token { get; set; }
}

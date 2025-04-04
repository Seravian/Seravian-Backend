public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerificationDate { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
}

public enum UserRole
{
    Patient = 0,
    Doctor = 1,
    Admin = 2,
}

public enum Gender
{
    Male = 0,
    Gender = 1,
}

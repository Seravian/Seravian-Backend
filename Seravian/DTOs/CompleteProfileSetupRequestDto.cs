public class CompleteProfileSetupRequestDto
{
    public string FullName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
}

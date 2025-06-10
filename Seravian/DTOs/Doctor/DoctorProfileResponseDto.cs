namespace Seravian.DTOs.Doctor;

public class DoctorProfileResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? FullName { get; set; }
    public DoctorTitle? Title { get; set; }
    public string? Description { get; set; }
    public int? SessionPrice { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }
}

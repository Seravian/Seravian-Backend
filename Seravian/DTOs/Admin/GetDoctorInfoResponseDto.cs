namespace Seravian.DTOs.Admin;

partial class GetDoctorInfoResponseDto
{
    public Guid Id { get; set; }
    public DoctorTitle? Title { get; set; }

    public string? Description { get; set; }

    public string DoctorFullName { get; set; }
    public string DoctorEmail { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender DoctorGender { get; set; }
    public int? SessionPrice { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
}

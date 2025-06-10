namespace Seravian.DTOs.Admin;

public class GetDoctorVerificationRequestResponseDto
{
    public int Id { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorFullName { get; set; }
    public string DoctorEmail { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender DoctorGender { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public List<DoctorVerificationRequestAttachmentDto> Attachments { get; set; } = [];
    public RequestStatus Status { get; set; }
    public DoctorTitle Title { get; set; }
    public string Description { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionNotes { get; set; }

    public Guid? ReviewerId { get; set; }
}

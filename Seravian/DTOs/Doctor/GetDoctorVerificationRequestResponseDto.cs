namespace Seravian.DTOs.Doctor;

public class GetDoctorVerificationRequestResponseDto
{
    public long Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<DoctorVerificationRequestAttachmentDto> Attachments { get; set; } = [];
    public int SessionPrice { get; set; }
    public RequestStatus Status { get; set; }
    public DoctorTitle Title { get; set; }
    public string Description { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }
    public string? RejectionNote { get; set; }
}

namespace Seravian.DTOs.Admin;

public class DoctorVerificationRequestAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public long SizeInBytes { get; set; }
}

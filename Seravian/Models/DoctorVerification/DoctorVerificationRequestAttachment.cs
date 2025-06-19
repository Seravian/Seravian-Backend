public class DoctorVerificationRequestAttachment
{
    public Guid Id { get; set; }
    public long DoctorVerificationRequestId { get; set; }
    public virtual DoctorVerificationRequest DoctorVerificationRequest { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string? Description { get; set; }
}

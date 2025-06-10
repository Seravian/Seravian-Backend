public class DoctorVerificationRequest
{
    public int Id { get; set; }
    public Guid DoctorId { get; set; }
    public DoctorTitle Title { get; set; }
    public string Description { get; set; }
    public int SessionPrice { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; } = null;
    public DateTime? DeletedAtUtc { get; set; } = null;
    public Guid? ReviewerId { get; set; }
    public string? RejectionNotes { get; set; } = null;
    public virtual Doctor Doctor { get; set; }
    public virtual Admin? Reviewer { get; set; }
    public virtual ICollection<DoctorVerificationRequestAttachment> Attachments { get; set; } = [];

    public byte[] RowVersion { get; set; } // concurrency token
}

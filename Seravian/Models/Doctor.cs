public class Doctor
{
    public Guid UserId { get; set; }
    public DoctorTitle? Title { get; set; }
    public int? SessionPrice { get; set; } = null;

    // Optional detailed description or biography
    public string? Description { get; set; }
    public virtual User User { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public virtual ICollection<DoctorVerificationRequest> DoctorVerificationRequests { get; set; } =
        [];
}

public class SessionBooking
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }

    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime PatientIsAvailableFromUtc { get; set; }
    public DateTime PatientIsAvailableToUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public SessionBookingStatus Status { get; set; }
    public string? DoctorNote { get; set; }
    public string? PatientNote { get; set; }
    public byte[] RowVersion { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public virtual Patient Patient { get; set; }
    public virtual Doctor Doctor { get; set; }
}

public enum SessionBookingStatus
{
    Pending = 0,
    Rejected,
    Accepted,
    Paid,
    Completed,
}

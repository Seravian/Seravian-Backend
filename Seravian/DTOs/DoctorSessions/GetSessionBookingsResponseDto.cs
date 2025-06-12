namespace Seravian.DTOs.DoctorSessions;

public class GetSessionBookingResponseDto
{
    public Guid Id { get; set; }
    public Gender PatientGender { get; set; }
    public int PatientAge { get; set; }

    public DateTime PatientIsAvailableFromUtc { get; set; }
    public DateTime PatientIsAvailableToUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public int SessionPrice { get; set; }
    public string? PatientNote { get; set; }

    public string? DoctorNote { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public SessionBookingStatus Status { get; set; }
}

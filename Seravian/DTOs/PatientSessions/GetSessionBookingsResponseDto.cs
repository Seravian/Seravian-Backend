namespace Seravian.DTOs.PatientSessions;

public class GetSessionBookingsResponseDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }

    public string DoctorFullName { get; set; }

    public DoctorTitle DoctorTitle { get; set; }
    public string DoctorDescription { get; set; }
    public int DoctorAge { get; set; }
    public Gender DoctorGender { get; set; }
    public DateTime PatientIsAvailableFromUtc { get; set; }
    public DateTime PatientIsAvailableToUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public int SessionPrice { get; set; }
    public string? PatientNote { get; set; }

    public string? DoctorNote { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public SessionBookingStatus Status { get; set; }
    public string? DoctorImageUrl { get; set; }
}

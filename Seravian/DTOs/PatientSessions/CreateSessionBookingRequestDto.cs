namespace Seravian.DTOs.PatientSessions;

public class CreateSessionBookingRequestDto
{
    public Guid DoctorId { get; set; }
    public DateTime PatientIsAvailableFromUtc { get; set; }
    public DateTime PatientIsAvailableToUtc { get; set; }
    public string? PatientNote { get; set; }
}

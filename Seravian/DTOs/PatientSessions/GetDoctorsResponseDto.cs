namespace Seravian.DTOs.PatientSessions;

public class GetDoctorsResponseDto
{
    public Guid DoctorId { get; set; }
    public DoctorTitle DoctorTitle { get; set; }
    public string DoctorDescription { get; set; }
    public int DoctorSessionPrice { get; set; }

    public string DoctorFullName { get; set; }
    public int DoctorAge { get; set; }
    public Gender DoctorGender { get; set; }
    public string? DoctorImageUrl { get; set; }
}

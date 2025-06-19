namespace Seravian.DTOs.Doctor;

public class SendVerificationRequestRequestDto
{
    public DoctorTitle Title { get; set; }
    public string Description { get; set; }
    public int SessionPrice { get; set; }
    public List<IFormFile> Attachments { get; set; } = [];
}

namespace Seravian.DTOs.Admin;

public class ReviewDoctorVerificationRequestRequestDto
{
    public int RequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionNotes { get; set; }
}

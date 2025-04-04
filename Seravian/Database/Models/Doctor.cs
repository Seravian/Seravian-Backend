using System.ComponentModel.DataAnnotations;

public class Doctor
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    public bool IsVerified { get; set; } // Field to indicate if the doctor is verified
    public DateTime? VerificationDate { get; set; }
}

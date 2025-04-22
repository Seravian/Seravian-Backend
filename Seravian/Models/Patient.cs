public class Patient
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}

public class Chat
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public virtual Patient Patient { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = [];
    public virtual ICollection<ChatDiagnosis> ChatDiagnoses { get; set; } = [];
}

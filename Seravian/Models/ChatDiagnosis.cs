public class ChatDiagnosis
{
    public long Id { get; set; } // Identity column

    public Guid ChatId { get; set; }

    public string? Description { get; set; }

    public DateTime RequestedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public long StartMessageId { get; set; }
    public long ToMessageId { get; set; }
    public bool IsDeleted { get; set; }
    public virtual Chat Chat { get; set; }

    public virtual ChatMessage StartMessage { get; set; }
    public virtual ChatMessage ToMessage { get; set; }
}

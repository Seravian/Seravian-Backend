using System.Numerics;

public class ChatMessage
{
    public long Id { get; set; }
    public string? Content { get; set; }

    public DateTime TimestampUtc { get; set; }
    public Guid ChatId { get; set; }
    public virtual Chat Chat { get; set; }
    public MessageType MessageType { get; set; }

    public virtual ChatMessageMedia? Media { get; set; }
    public virtual ChatVoiceAnalysis? VoiceAnalysis { get; set; }
    public bool IsAI { get; set; }
    public bool IsDeleted { get; set; }
}

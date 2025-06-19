namespace Seravian.DTOs.Chat;

public class ChatMessageDto
{
    public long Id { get; set; }
    public string Content { get; set; }
    public DateTime TimestampUtc { get; set; }
    public MessageType MessageType { get; set; }
    public bool IsAI { get; set; }
}

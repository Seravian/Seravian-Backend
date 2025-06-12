namespace Seravian.DTOs.ChatHub;

public class ReceiveAIResponseDto
{
    public Guid ChatId { get; set; }
    public long Id { get; set; }
    public string Message { get; set; }
    public DateTime TimestampUtc { get; set; }
    public MessageType MessageType { get; set; }
}

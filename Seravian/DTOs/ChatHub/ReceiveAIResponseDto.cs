namespace Seravian.Hubs;

public class ReceiveAIResponseDto
{
    public long Id { get; set; }
    public string Message { get; set; }
    public DateTime TimestampUtc { get; set; }
    public MessageType MessageType { get; set; }
}

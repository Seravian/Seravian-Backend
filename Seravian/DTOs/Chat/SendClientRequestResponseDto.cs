namespace Seravian.DTOs.Chat;

public class SendClientRequestResponseDto
{
    public Guid ChatId { get; set; }
    public Guid ClientMessageId { get; set; }

    public long MessageId { get; set; }
    public DateTime TimestampUtc { get; set; }
}

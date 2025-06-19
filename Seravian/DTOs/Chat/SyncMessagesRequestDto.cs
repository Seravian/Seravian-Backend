namespace Seravian.DTOs.Chat;

public class SyncMessagesRequestDto
{
    public long? LastMessageId { get; set; }
    public Guid ChatId { get; set; }
}

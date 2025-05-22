public class ConfirmClientRequestDto
{
    public Guid ChatId { get; set; }
    public Guid ClientMessageId { get; set; }
    public long MessageId { get; set; }
    public DateTime TimestampUtc { get; set; }
}

public class ConfirmClientRequestDto
{
    public DateTime TimestampUtc { get; set; }
    public long MessageId { get; internal set; }
    public Guid ClientMessageId { get; internal set; }
}

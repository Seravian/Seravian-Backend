public class ConfirmClientRequestDto
{
    public DateTime TimestampUtc { get; set; }
    public long MessageId { get; set; }
    public Guid ClientMessageId { get; internal set; }
}

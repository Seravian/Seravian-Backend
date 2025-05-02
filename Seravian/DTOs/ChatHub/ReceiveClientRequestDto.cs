namespace Seravian.Hubs;

public class ReceiveClientRequestDto
{
    public long Id { get; set; }
    public string Message { get; set; }
    public DateTime TimestampUtc { get; set; }
}

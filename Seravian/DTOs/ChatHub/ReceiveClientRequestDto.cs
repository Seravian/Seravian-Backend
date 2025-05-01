namespace Seravian.Hubs;

public class ReceiveClientRequestDto
{
    public string Message { get; set; }
    public DateTime TimestampUtc { get; set; }
}

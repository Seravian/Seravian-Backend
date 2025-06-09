namespace Seravian.Hubs;

public class SendClientRequestDto
{
    public Guid MessageClientId { get; set; }
    public string Message { get; set; }
}

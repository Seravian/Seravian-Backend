namespace Seravian.DTOs.Chat
{
    public class SendClientRequestRequestDto
    {
        public Guid ChatId { get; set; }
        public Guid ClientMessageId { get; set; }
        public string Message { get; set; }
    }
}

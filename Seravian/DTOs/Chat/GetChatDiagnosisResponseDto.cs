namespace Seravian.DTOs.Chat
{
    public class GetChatDiagnosisResponseDto
    {
        public long Id { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}

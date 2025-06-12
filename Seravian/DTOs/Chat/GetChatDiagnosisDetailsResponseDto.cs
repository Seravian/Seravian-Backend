namespace Seravian.DTOs.Chat
{
    public class GetChatDiagnosisDetailsResponseDto
    {
        public long Id { get; set; }
        public string? Description { get; set; }

        public DateTime RequestedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}

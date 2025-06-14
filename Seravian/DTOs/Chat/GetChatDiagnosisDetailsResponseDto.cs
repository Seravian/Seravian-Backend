namespace Seravian.DTOs.Chat
{
    public class GetChatDiagnosisDetailsResponseDto
    {
        public long Id { get; set; }

        public string? FailureReason { get; set; }
        public string? DiagnosedProblem { get; set; }
        public string? Reasoning { get; set; }
        public List<string>? Prescriptions { get; set; }

        public DateTime RequestedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}

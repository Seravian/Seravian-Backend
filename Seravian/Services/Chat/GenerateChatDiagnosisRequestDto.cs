namespace Seravian.DTOs.Services.Dtos;

class GenerateChatDiagnosisRequestDto
{
    public Guid ChatId { get; set; }
    public long ChatDiagnosisId { get; set; }

    public List<GenerateChatDiagnosisMessageEntry> Messages { get; set; } = [];
}
